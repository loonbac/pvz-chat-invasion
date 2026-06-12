# PvZ Chat Invasion

Mod para **Plants vs. Zombies: Replanted** (Steam) que deja al chat de tus
streams invadir la partida: los espectadores invocan zombies y eventos en
tiempo real. Multiplataforma por diseño, empezando por **Twitch**.

## Stack

- **Juego**: PvZ: Replanted — Unity 6 (6000.0.52f1) / IL2CPP
- **Mod loader**: MelonLoader 0.7.3
- **API de juego**: ReplantAPI (eventos + spawns sobre Harmony)
- **Chat**: TwitchLib (IRC anónimo, solo lectura) — sin OAuth para empezar
- **Lenguaje**: C# / .NET 6
- **SO de desarrollo**: Arch Linux + Proton

## Arquitectura (hexagonal / ports & adapters)

```
  [TwitchChatSource]  ──┐                         ┌─→ Board.AddZombieInRow (Il2Cpp)
  [YouTubeChatSource] ──┼─→ ChatCommand ─→ Core ──┤
  [KickChatSource]    ──┘   (normalizado)  (OnUpdate)  └─→ BoardManager.AddSun, ...
        ADAPTERS              DOMINIO        APP            JUEGO
```

- **Domain/** — núcleo puro: `ChatCommand`, `IChatSource` (puerto), `GameActionMapper`.
  No sabe qué es Twitch ni MelonLoader.
- **Adapters/** — `TwitchChatSource` traduce IRC → `ChatCommand`. Un archivo por plataforma.
- **Infrastructure/** — `CommandQueue` (puente thread-safe red↔juego) y `ModConfig`
  (MelonPreferences → `UserData/PvZChatInvasion.cfg`).
- **Core.cs** — entry point `MelonMod`. Orquesta, no decide.

## Configuración (sin recompilar)

`[juego]/UserData/PvZChatInvasion.cfg` — canal de Twitch, cooldown, TLS.
**F9** dentro del juego recarga la config y reconecta el chat al vuelo.
Detalles en [`INSTALL.md`](INSTALL.md).

### El detalle crítico: threading

TwitchLib entrega mensajes en su hilo de red. ReplantAPI/Unity SOLO se tocan
desde el hilo del juego. Por eso los comandos se **encolan** (`CommandQueue`) y
se **drenan en `OnUpdate`**. Saltarte esto = crash.

## Roadmap

- [x] **Fase 0** — Entorno verificado (Unity 6 IL2CPP, MelonLoader 0.7.3)
- [x] **Fase 1** — Spawn por tecla F8 (falta solo confirmación visual en nivel)
- [x] **Fase 2** — Twitch chat → spawn (`!zombie`) — conexión IRC verificada en runtime;
  config en caliente con F9
- [ ] **Fase 3** — Más comandos, votaciones, cooldowns por usuario
- [ ] **Fase 4** — Multiplataforma (YouTube/Kick o Streamer.bot) + config con BloomEngine
- [ ] **Fase 5** — Pulido de stream (overlay, créditos, dificultad dinámica)

## Instalación

Ver [`INSTALL.md`](INSTALL.md). Resumen: instalar .NET SDK → MelonLoader
en el juego (Proton) → ReplantAPI en `Mods/` → `dotnet build`.
```fish
cd src/PvZChatInvasion && dotnet build -c Release
```
```
