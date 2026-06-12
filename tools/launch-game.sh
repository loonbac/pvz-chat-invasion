#!/usr/bin/env bash
# Lanza PvZ: Replanted con MelonLoader vía Proton directo (sin pasar por la UI de Steam).
set -u

GAME_DIR="/home/loonbac/Juegos/SteamLibrary/steamapps/common/PVZ Replanted"
SLR="/home/loonbac/Juegos/SteamLibrary/steamapps/common/SteamLinuxRuntime_sniper"
PROTON="/home/loonbac/.local/share/Steam/steamapps/common/Proton - Experimental/proton"

export STEAM_COMPAT_CLIENT_INSTALL_PATH="/home/loonbac/.local/share/Steam"
export STEAM_COMPAT_DATA_PATH="/home/loonbac/Juegos/SteamLibrary/steamapps/compatdata/3654560"
export STEAM_COMPAT_MOUNTS="/home/loonbac/Juegos/SteamLibrary"
# Imprescindible para que MelonLoader inyecte: su version.dll debe ganar a la nativa.
export WINEDLLOVERRIDES="version=n,b"

cd "$GAME_DIR"
exec "$SLR/_v2-entry-point" --verb=run -- "$PROTON" run "$GAME_DIR/Replanted.exe"
