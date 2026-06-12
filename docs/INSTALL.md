# Instalación — PvZ Chat Invasion (Arch Linux + Proton)

Juego: **PvZ: Replanted** (Unity 6 / IL2CPP) · AppID `3654560`
Ruta: `/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted`

La cadena de dependencias importa. Haz los pasos EN ORDEN; cada uno habilita al siguiente.

---

## 0. SDK de .NET en el host (para compilar el mod)

```fish
sudo pacman -S dotnet-sdk
dotnet --version   # verifica
```

> El mod apunta a `net6.0`, pero el SDK puede ser más nuevo (8/9): compila igual.

---

## 1. `protontricks` + `dotnetdesktop6` en el prefijo Proton

IL2CPP necesita el runtime .NET 6 **dentro del prefijo del juego**, no en el host.

```fish
paru -S protontricks            # o: flatpak install com.github.Matoking.protontricks
protontricks 3654560 dotnetdesktop6
```

> Si usas la versión flatpak, el comando es:
> `flatpak run com.github.Matoking.protontricks 3654560 dotnetdesktop6`

---

## 2. Instalar MelonLoader en el juego

Tienes el instalador nativo en `downloads/MelonLoader.Installer.Linux`.

```fish
cd /home/loonbac/Projects/pvz/downloads
./MelonLoader.Installer.Linux
```

En la GUI: selecciona el `Replanted.exe` del juego y deja que instale (versión **0.7.3**, IL2CPP).

**Alternativa manual** (si el instalador no detecta el juego): descomprime
`MelonLoader.x64.zip` dentro de la carpeta del juego (junto a `Replanted.exe`).

---

## 3. Launch option en Steam (CRÍTICO)

Steam → PvZ Replanted → Propiedades → Opciones de lanzamiento:

```
WINEDLLOVERRIDES="version=n,b" %command%
```

Sin esto, MelonLoader NO se inyecta bajo Proton.

---

## 4. Primer arranque — genera los Il2CppAssemblies

Lanza el juego desde Steam. La PRIMERA vez tarda ~1-2 min generando los
assemblies de interop. Cuando veas la consola de MelonLoader y el menú del
juego, ciérralo. Verifica que existan:

```fish
ls "/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted/MelonLoader/Il2CppAssemblies/" | head
```

> Si Cpp2IL falla por permisos (raro en 0.7.3, ya lo automatiza):
> `chmod +x ".../MelonLoader/Dependencies/Il2CppAssemblyGenerator/Cpp2IL/Cpp2IL"`

---

## 5. Instalar ReplantAPI (+ BloomEngine opcional)

Descarga desde GameBanana / GitHub y copia los `.dll` a la carpeta `Mods/`:

- **ReplantAPI** → https://gamebanana.com/mods/629661  (o el GitHub de HenHen)
- **BloomEngine** (opcional, menú in-game) → https://github.com/PalmForest0/BloomEngine

```
PVZ Replanted/Mods/ReplantAPI.dll
PVZ Replanted/Mods/BloomEngine.dll   (opcional)
```

> Recuerda el orden de carga: ReplantAPI debe cargar ANTES que nuestro mod.
> MelonLoader carga alfabético; "PvZChatInvasion" > "ReplantAPI", así que ok.

Arranca el juego otra vez y confirma en la consola que ReplantAPI cargó.

---

## 6. Compilar e instalar el mod

```fish
cd /home/loonbac/Projects/pvz/src/PvZChatInvasion
dotnet build -c Release
```

El `.csproj` copia el DLL a `Mods/` automáticamente tras compilar.

**Dependencias managed (TwitchLib)**: copia las DLLs de TwitchLib y sus
dependencias (de `bin/Release/net6.0/`) a la carpeta `UserLibs/` del juego:

```fish
set G "/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted"
mkdir -p "$G/UserLibs"
cp bin/Release/net6.0/TwitchLib*.dll bin/Release/net6.0/Newtonsoft.Json.dll "$G/UserLibs/" 2>/dev/null
```

---

## 7. Configurar (SIN recompilar)

Toda la config vive en `[juego]/UserData/PvZChatInvasion.cfg` (se crea sola
en el primer arranque). Edítala con cualquier editor de texto:

```toml
[PvZChatInvasion]
EnableTwitch = true
TwitchChannel = "tu_canal"      # sin #, en minúsculas
SpawnCooldownSeconds = 1.5
TwitchUseSsl = false            # false bajo Wine/Proton (SChannel roto); true en Windows nativo
```

Teclas dentro del juego:

- **F8** → spawn de prueba local (zombie con cono), sin chat.
- **F9** → recarga la config y reconecta el chat **sin reiniciar el juego**.

## 8. Probar

1. **Fase 1**: entra a un nivel y pulsa **F8**. Debe aparecer un zombie. ✅
2. **Fase 2**: escribe tu canal en `PvZChatInvasion.cfg`, pulsa **F9** (o
   reinicia). El log debe decir `Twitch conectado como anónimo a #tu_canal`.
   En tu chat escribe `!zombie cono` → debe spawnear. ✅

> Nota Wine/Proton: el chat usa IRC sin TLS (puerto 6667) porque el SChannel
> de Wine rompe el handshake TLS de .NET (0x80090304) y Twitch ya no acepta
> WebSocket sin TLS. Es solo LECTURA anónima: no viaja ninguna credencial.

---

## Verificación de rutas (si `dotnet build` falla por referencias)

Las rutas del `.csproj` asumen la estructura estándar de MelonLoader 0.7.
Si alguna no existe, ajústala. Para encontrarlas:

```fish
set G "/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted"
find "$G/MelonLoader" -name "MelonLoader.dll"
find "$G/MelonLoader" -name "UnityEngine.CoreModule.dll"
```
