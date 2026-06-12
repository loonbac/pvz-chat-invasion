# PvZ Chat Invasion — Tu chat invade tu partida

Mod para **Plants vs. Zombies: Replanted** (Steam) que conecta tu chat de
Twitch con el juego: los espectadores escriben comandos como `!zombie gigante`
y ese zombie aparece en tu partida, en vivo. Tú juegas, ellos atacan.

No necesitas saber programar para usarlo. Esta guía es todo lo que hace falta.

---

## Ponerlo a funcionar (5 minutos)

1. Abre este archivo con cualquier editor de texto (Bloc de notas, Kate, gedit...):

   ```
   .../SteamLibrary/steamapps/common/PVZ Replanted/UserData/PvZChatInvasion.cfg
   ```

2. Busca la línea `TwitchChannel` y escribe tu canal entre las comillas.
   Es el nombre que aparece en tu URL de Twitch (`twitch.tv/tu_canal`):

   ```
   TwitchChannel = "tu_canal"
   ```

3. Guarda el archivo y abre el juego desde Steam como siempre.
   (¿Ya estaba abierto? Pulsa **F9** dentro del juego y listo, sin reiniciar.)

4. Entra a un nivel y pide a alguien de tu chat que escriba `!zombie cono`.
   Si aparece un zombie con cono en el jardín: **funciona**. 🧟

> El mod solo **lee** tu chat. No escribe mensajes, no pide tu contraseña
> ni ningún permiso de tu cuenta de Twitch.

---

## Teclas dentro del juego

| Tecla | Qué hace |
|-------|----------|
| **F8** | Crea un zombie de prueba (sin chat, sin internet). Para comprobar que el mod vive. |
| **F9** | Relee la configuración y reconecta el chat. Úsala después de editar el archivo. |

---

## Comandos que puede usar tu chat

| Comando | Qué hace |
|---------|----------|
| `!zombie <tipo>` (o `!z <tipo>`) | Invoca un zombie en una fila al azar |
| `!zombie` (sin tipo) | Invoca un zombie normal |
| `!sol` | Regala +25 soles al jugador |

Tipos de zombie disponibles — **una sola palabra por zombie** (los acentos
dan igual: `fútbol` y `futbol` funcionan los dos):

| Palabra | Zombie |
|---------|--------|
| `normal` | Zombie básico |
| `bandera` | Abanderado |
| `cono` | Cono de tráfico |
| `polo` | Saltador con pértiga |
| `balde` | Cubeta de metal |
| `periodico` | Zombie con periódico |
| `puerta` | Puerta de malla |
| `futbol` | Jugador de fútbol americano |
| `bailarin` | Bailarín |
| `corista` | Bailarín de apoyo |
| `patito` | Zombie con flotador |
| `buzo` | Buzo con esnórquel |
| `zamboni` | Pulidora de hielo |
| `trineo` | Equipo de trineo |
| `delfin` | Jinete de delfín |
| `caja` | Caja sorpresa |
| `globo` | Zombie con globo |
| `minero` | Minero |
| `pogo` | Saltarín pogo |
| `yeti` | Yeti |
| `bungee` | Zombie bungee |
| `escalera` | Zombie con escalera |
| `catapulta` | Catapulta |
| `gigante` | Gargantúa |
| `diablillo` | Diablillo |
| `ojosrojos` | Gargantúa de ojos rojos |
| `guisante` | Cabeza de guisante |
| `nuez` | Cabeza de nuez |
| `jalapeno` | Cabeza de jalapeño |
| `gatling` | Cabeza de gatling |
| `calabaza` | Cabeza de calabaza |
| `nuezalta` | Cabeza de nuez alta |

> Algunos son situacionales: los acuáticos (`buzo`, `delfin`, `patito`) van a
> la piscina, y `trineo` o `bungee` pueden comportarse raro fuera de sus
> niveles. Probarlos es parte de la gracia.

Anti-spam incluido: como máximo un zombie cada 1.5 segundos, aunque el chat
escriba más rápido (se puede cambiar en la configuración).

---

## El archivo de configuración, línea por línea

Está en `UserData/PvZChatInvasion.cfg` dentro de la carpeta del juego.
Se crea solo la primera vez que abres el juego con el mod.

| Opción | Qué significa |
|--------|---------------|
| `EnableTwitch = true` | `true` = conectar al chat al abrir el juego. `false` = jugar sin chat (F8 sigue funcionando). |
| `TwitchChannel = ""` | Tu canal de Twitch, sin `#` y en minúsculas. Vacío = no conecta. |
| `SpawnCooldownSeconds = 1.5` | Segundos mínimos entre zombie y zombie del chat. Súbelo si tu chat es muy intenso. |
| `TwitchUseSsl = false` | **Déjalo como está** (`false`) si juegas en Linux/Proton. Solo ponlo en `true` en Windows. |

Después de cualquier cambio: guardar el archivo y pulsar **F9** en el juego.

---

## ¿Algo no funciona?

Junto al juego se abre una ventana de consola (texto negro). Ahí el mod cuenta
lo que está pasando. Busca estos mensajes:

| Mensaje en la consola | Qué significa | Qué hacer |
|----------------------|---------------|-----------|
| `PvZ Chat Invasion cargado.` | El mod arrancó bien | Nada 🙂 |
| `Twitch conectado como anónimo a #tu_canal` | El chat está enchufado | Nada 🙂 |
| `Sin canal de Twitch...` | Falta tu canal en la configuración | Escríbelo en el archivo y pulsa F9 |
| `No se pudo conectar a Twitch...` | No hubo conexión | Revisa tu internet y el nombre del canal; pulsa F9 para reintentar |

Otros problemas frecuentes:

- **Escribo `!zombie` y no pasa nada** → Tienes que estar **dentro de un
  nivel** jugando, no en el menú. Y revisa que el comando lleve `!` al inicio.
- **F8 no crea ningún zombie** → También necesita que estés dentro de un nivel.
- **No aparece ni la consola** → El mod no está instalado o Steam lo lanzó sin
  él. Revisa que exista el archivo `Mods/PvZChatInvasion.dll` en la carpeta
  del juego.
- Si nada de esto lo arregla, el historial completo está en el archivo
  `MelonLoader/Latest.log` dentro de la carpeta del juego — mándaselo a quien
  te ayude con el mod.

---

## Lista rápida de "todo está bien"

- [ ] Al abrir el juego, la consola dice `PvZ Chat Invasion cargado.`
- [ ] La consola dice `Twitch conectado como anónimo a #tu_canal`
- [ ] Dentro de un nivel, **F8** crea un zombie
- [ ] `!zombie cono` desde tu chat crea un zombie

Si puedes marcar las cuatro: a streamear. 🎬

---

## Para desarrolladores

La documentación técnica (arquitectura, instalación desde cero, decisiones de
diseño) está en [`docs/DEVELOPMENT.md`](docs/DEVELOPMENT.md) y
[`docs/INSTALL.md`](docs/INSTALL.md).
