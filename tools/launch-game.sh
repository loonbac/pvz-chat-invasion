#!/usr/bin/env bash
# Lanza PvZ: Replanted con MelonLoader.
# - Si Steam está corriendo: lanza vía Steam (evita el doble arranque: la instancia
#   directa muere a los ~30s cuando Steamworks le pide a Steam tomar el control).
# - Si no: lanzamiento directo por Proton (requiere Steam para el DRM igualmente,
#   así que lo arrancamos antes).
set -u

APPID=3654560
GAME_DIR="/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted"
SLR="/home/loonbac/Juegos/SteamLibrary/steamapps/common/SteamLinuxRuntime_sniper"
PROTON="/home/loonbac/.local/share/Steam/steamapps/common/Proton - Experimental/proton"

if pgrep -x steam >/dev/null; then
    exec steam -applaunch "$APPID"
fi

# Steam apagado: levantarlo (DRM) y lanzar directo por Proton.
nohup steam -silent >/dev/null 2>&1 &
for _ in $(seq 1 45); do pgrep -x steamwebhelper >/dev/null && break; sleep 2; done
sleep 5

export STEAM_COMPAT_CLIENT_INSTALL_PATH="/home/loonbac/.local/share/Steam"
export STEAM_COMPAT_DATA_PATH="/home/loonbac/Juegos/SteamLibrary/steamapps/compatdata/$APPID"
export STEAM_COMPAT_MOUNTS="/home/loonbac/Juegos/SteamLibrary"
# Imprescindible para que MelonLoader inyecte: su version.dll debe ganar a la nativa.
export WINEDLLOVERRIDES="version=n,b"

cd "$GAME_DIR"
exec "$SLR/_v2-entry-point" --verb=run -- "$PROTON" run "$GAME_DIR/Replanted.exe"
