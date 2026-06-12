using System;
using System.Collections.Generic;
using MelonLoader;
using ReplantAPI.Managers;          // BoardManager
using Il2CppReloaded.Gameplay;      // Board, ZombieType (tipos del juego vía interop)
using PvZChatInvasion.Infrastructure;

namespace PvZChatInvasion.Domain
{
    /// <summary>
    /// Traduce un ChatCommand normalizado a una acción concreta del juego.
    /// El spawn de zombies NO existe en ReplantAPI 2.0.1, así que llamamos al
    /// método del juego directamente: BoardManager.Board.AddZombieInRow(...).
    ///
    /// Aislado a propósito: toda la lógica de "qué hace cada comando" vive aquí,
    /// separada de cómo llega (chat) y de cuándo se ejecuta (hilo del juego).
    /// </summary>
    public sealed class GameActionMapper
    {
        private readonly MelonLogger.Instance _log;
        private readonly ModConfig _config;

        // Anti-spam global: un spawn cada SpawnCooldownSeconds (config) como máximo.
        private DateTime _lastSpawn = DateTime.MinValue;

        /// <summary>
        /// Palabra del chat (español) -> ZombieType real del juego.
        /// Valores confirmados del enum Il2CppReloaded.Gameplay.ZombieType.
        /// </summary>
        private static readonly Dictionary<string, ZombieType> ZombieByWord =
            new Dictionary<string, ZombieType>(StringComparer.OrdinalIgnoreCase)
            {
                ["normal"]    = ZombieType.Normal,
                ["cono"]      = ZombieType.TrafficCone,
                ["balde"]     = ZombieType.Pail,
                ["cubeta"]    = ZombieType.Pail,
                ["bandera"]   = ZombieType.Flag,
                ["polo"]      = ZombieType.Polevaulter,
                ["saltador"]  = ZombieType.Polevaulter,
                ["periodico"] = ZombieType.Newspaper,
                ["puerta"]    = ZombieType.Door,
                ["futbol"]    = ZombieType.Football,
                ["bailarin"]  = ZombieType.Dancer,
                ["buzo"]      = ZombieType.Snorkel,
                ["zamboni"]   = ZombieType.Zamboni,
                ["delfin"]    = ZombieType.DolphinRider,
                ["caja"]      = ZombieType.JackInTheBox,
                ["globo"]     = ZombieType.Balloon,
                ["minero"]    = ZombieType.Digger,
                ["pogo"]      = ZombieType.Pogo,
                ["yeti"]      = ZombieType.Yeti,
                ["escalera"]  = ZombieType.Ladder,
                ["catapulta"] = ZombieType.Catapult,
                ["gigante"]    = ZombieType.Gargantuar,
                ["gargantua"]  = ZombieType.Gargantuar,
                ["gargantuar"] = ZombieType.Gargantuar,
                ["diablillo"]  = ZombieType.Imp,
                ["imp"]        = ZombieType.Imp,
            };

        public GameActionMapper(MelonLogger.Instance log, ModConfig config)
        {
            _log = log;
            _config = config;
        }

        /// <summary>Decide qué hacer según el verbo del comando.</summary>
        public void Execute(ChatCommand command)
        {
            switch (command.Verb.ToLowerInvariant())
            {
                case "zombie":
                case "z":
                    SpawnZombie(command);
                    break;

                case "sol":
                case "sun":
                    BoardManager.AddSun(25, 0);
                    _log.Msg($"[{command.Platform}] {command.User} -> +25 sol");
                    break;

                // Futuro (Fase 3): "vote", "wave", "ola"...
                default:
                    break; // comando desconocido: ignorar
            }
        }

        private void SpawnZombie(ChatCommand command)
        {
            if ((DateTime.UtcNow - _lastSpawn).TotalSeconds < _config.SpawnCooldownSeconds)
                return; // en cooldown

            string word = string.IsNullOrWhiteSpace(command.Argument) ? "normal" : command.Argument.Trim();
            if (!ZombieByWord.TryGetValue(word, out ZombieType type))
            {
                type = ZombieType.Normal;
                _log.Msg($"[{command.Platform}] palabra desconocida '{word}' -> zombie normal (tipos validos: ver README)");
            }

            Board board = BoardManager.Board;
            if (board == null) return;

            // La FILA la elige el propio juego: PickRowForNewZombie respeta las
            // filas sin césped (Dirt) de los niveles tempranos y manda los zombies
            // acuáticos a la piscina. Un random(0, GetNumRows()) spawneaba en tierra.
            int row = board.PickRowForNewZombie(type);
            if (row < 0 || !board.RowCanHaveZombies(row))
            {
                _log.Msg($"[{command.Platform}] {command.User} -> '{word}' descartado: sin fila válida en este nivel");
                return;
            }

            // Llamada REAL al juego. Segura aquí porque el Core nos invoca desde
            // OnUpdate (hilo del juego) tras comprobar BoardManager.IsLoaded.
            // Firma: AddZombieInRow(ZombieType type, int row, int fromWave, bool)
            board.AddZombieInRow(type, row, 0, false);

            _lastSpawn = DateTime.UtcNow;
            _log.Msg($"[{command.Platform}] {command.User} -> zombie '{word}' ({type}) en fila {row}");
        }
    }
}
