using System;
using MelonLoader;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using PvZChatInvasion.Domain;
// TwitchLib.Client.Models también define un 'ChatCommand'; aliasamos el nuestro
// para evitar la ambigüedad (CS0104).
using ChatCommand = PvZChatInvasion.Domain.ChatCommand;

namespace PvZChatInvasion.Adapters
{
    /// <summary>
    /// ADAPTADOR de Twitch (hexagonal). Implementa IChatSource leyendo el chat
    /// en modo ANÓNIMO: para SOLO LEER no hace falta OAuth ni registrar una app.
    /// Twitch acepta conexiones IRC de lectura con un nick "justinfanXXXXX".
    ///
    /// Corre en el hilo de red de TwitchLib. Por eso NO toca el juego: solo
    /// dispara OnCommand, y el Core se encarga de encolar y ejecutar en su hilo.
    /// </summary>
    public sealed class TwitchChatSource : IChatSource
    {
        public event Action<ChatCommand> OnCommand;

        private readonly TwitchClient _client;
        private readonly string _channel;
        private readonly MelonLogger.Instance _log;

        public TwitchChatSource(string channel, bool useSsl, MelonLogger.Instance log)
        {
            _channel = channel;
            _log = log;

            // Credenciales anónimas: nick justinfan + número, password vacío => solo lectura.
            var anonNick = $"justinfan{new Random().Next(10000, 99999)}";
            var credentials = new ConnectionCredentials(anonNick, "");

            // useSsl=false => IRC crudo en irc.chat.twitch.tv:6667 (TcpClient). Necesario
            // bajo Wine/Proton: su SChannel rompe el TLS de .NET (0x80090304), y Twitch
            // ya NO acepta WebSocket sin TLS (el puerto 80 devuelve 301 -> wss).
            // Aceptable PORQUE somos lectura anónima: no viaja ninguna credencial.
            // Si algún día se añade OAuth (escribir en chat), esto DEBE volver a true.
            TwitchLib.Communication.Interfaces.IClient socketClient = useSsl
                ? new WebSocketClient(new ClientOptions())
                : new TcpClient(new ClientOptions { UseSsl = false });
            _client = new TwitchClient(socketClient);
            _client.Initialize(credentials, _channel);

            _client.OnConnected += (_, _) => _log.Msg($"Twitch conectado como anónimo a #{_channel}");
            _client.OnMessageReceived += HandleMessage;
        }

        public void Connect()
        {
            _log.Msg($"Twitch: conectando a #{_channel}...");
            _client.Connect();
        }

        public void Disconnect()
        {
            if (_client.IsConnected) _client.Disconnect();
        }

        private void HandleMessage(object sender, OnMessageReceivedArgs e)
        {
            string text = e.ChatMessage.Message.Trim();
            if (text.Length < 2 || text[0] != '!') return; // solo comandos "!algo"

            // "!zombie cono" -> verb="zombie", arg="cono"
            string body = text.Substring(1);
            int space = body.IndexOf(' ');
            string verb = space < 0 ? body : body.Substring(0, space);
            string arg = space < 0 ? "" : body.Substring(space + 1).Trim();

            OnCommand?.Invoke(new ChatCommand(
                platform: "twitch",
                user: e.ChatMessage.Username,
                verb: verb,
                argument: arg));
        }
    }
}
