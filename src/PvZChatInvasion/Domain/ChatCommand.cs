namespace PvZChatInvasion.Domain
{
    /// <summary>
    /// Comando de chat NORMALIZADO, independiente de la plataforma de origen.
    /// Este es el "lenguaje común" del dominio: el juego solo entiende esto,
    /// nunca un mensaje crudo de Twitch/YouTube/Kick.
    ///
    /// Es el corazón de la arquitectura hexagonal: cada adaptador de plataforma
    /// traduce SU formato a este tipo, y el núcleo del juego no sabe de dónde vino.
    /// </summary>
    public sealed class ChatCommand
    {
        /// <summary>Plataforma de origen: "twitch", "youtube", "kick", "debug"...</summary>
        public string Platform { get; }

        /// <summary>Nombre del espectador que escribió el comando.</summary>
        public string User { get; }

        /// <summary>Verbo del comando, sin el prefijo "!". Ej: "zombie", "vote".</summary>
        public string Verb { get; }

        /// <summary>Argumento del comando. Ej: "cono", "balde". Puede ser vacío.</summary>
        public string Argument { get; }

        public ChatCommand(string platform, string user, string verb, string argument)
        {
            Platform = platform;
            User = user;
            Verb = verb;
            Argument = argument;
        }
    }
}
