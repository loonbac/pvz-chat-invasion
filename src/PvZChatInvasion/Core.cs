using MelonLoader;
using UnityEngine;
using ReplantAPI.Managers;          // BoardManager, HotkeyManager
using Il2CppReloaded.Gameplay;      // Board (estado del nivel)
using PvZChatInvasion.Domain;
using PvZChatInvasion.Adapters;
using PvZChatInvasion.Infrastructure;

[assembly: MelonInfo(typeof(PvZChatInvasion.Core), "PvZ Chat Invasion", "0.2.0", "loonbac")]
[assembly: MelonGame(null, null)]

namespace PvZChatInvasion
{
    /// <summary>
    /// ENTRY POINT del mod (la "aplicación" hexagonal). Orquesta las piezas pero
    /// NO contiene lógica de negocio: conecta el adaptador de chat con la cola y
    /// con el mapper, y drena la cola en el hilo del juego.
    ///
    /// Config: UserData/PvZChatInvasion.cfg (canal de Twitch, cooldowns...).
    /// F8 = spawn de prueba local. F9 = recargar config y reconectar el chat.
    /// </summary>
    public sealed class Core : MelonMod
    {
        private readonly CommandQueue _queue = new CommandQueue();
        private ModConfig _config;
        private GameActionMapper _mapper;
        private IChatSource _chat;

        public override void OnInitializeMelon()
        {
            _config = new ModConfig();
            _mapper = new GameActionMapper(LoggerInstance, _config);
            LoggerInstance.Msg("PvZ Chat Invasion cargado.");

            // F8: prueba sin chat — encola un zombie con cono.
            HotkeyManager.Register(
                "pvzchat_debug_spawn",
                KeyCode.F8,
                () => _queue.Enqueue(new ChatCommand("debug", "tu", "zombie", "cono")),
                false, false, false);

            // F9: hot-reload de la config (cambia el canal SIN reiniciar el juego).
            HotkeyManager.Register(
                "pvzchat_reload_config",
                KeyCode.F9,
                ReloadConfigAndReconnect,
                false, false, false);

            ConnectChat();
        }

        public override void OnUpdate()
        {
            // Drenamos en el HILO DEL JUEGO: único lugar seguro para tocar el Board.
            if (_queue.IsEmpty) return;

            // Solo se actúa con una partida ACTIVA. Fuera de eso (menú, nivel ya
            // ganado/terminado) los comandos se DESCARTAN: ni spawns en la pantalla
            // de victoria ni colas que emboscan al empezar el siguiente nivel.
            if (!BoardManager.IsLoaded) { _queue.Clear(); return; }

            Board board = BoardManager.Board;
            if (board == null || board.mLevelComplete) { _queue.Clear(); return; }

            while (_queue.TryDequeue(out var command))
                _mapper.Execute(command);
        }

        public override void OnDeinitializeMelon() => _chat?.Disconnect();

        private void ConnectChat()
        {
            if (!_config.EnableTwitch)
            {
                LoggerInstance.Msg("Twitch desactivado (EnableTwitch = false en UserData/PvZChatInvasion.cfg).");
                return;
            }

            string channel = _config.TwitchChannel;
            if (channel.Length == 0)
            {
                LoggerInstance.Warning(
                    "Sin canal de Twitch. Escribe tu canal en UserData/PvZChatInvasion.cfg " +
                    "(TwitchChannel = \"tu_canal\") y pulsa F9 en el juego para reconectar.");
                return;
            }

            try
            {
                _chat = new TwitchChatSource(channel, _config.TwitchUseSsl, LoggerInstance);
                _chat.OnCommand += command => _queue.Enqueue(command); // encolar, NO ejecutar aquí
                _chat.Connect();
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Warning($"No se pudo conectar a Twitch: {ex.Message}. Revisa la config y pulsa F9 para reintentar.");
                _chat = null;
            }
        }

        private void ReloadConfigAndReconnect()
        {
            _config.Reload();
            LoggerInstance.Msg($"Config recargada. Canal: '{_config.TwitchChannel}', Twitch: {_config.EnableTwitch}");

            _chat?.Disconnect();
            _chat = null;
            ConnectChat();
        }
    }
}
