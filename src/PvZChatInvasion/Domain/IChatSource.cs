using System;

namespace PvZChatInvasion.Domain
{
    /// <summary>
    /// PUERTO DE ENTRADA (hexagonal). Cualquier plataforma de chat implementa
    /// este contrato. El núcleo del juego depende de esta interfaz, NUNCA de
    /// TwitchLib ni de ninguna librería concreta.
    ///
    /// Añadir YouTube o Kick mañana = una clase nueva que implementa IChatSource.
    /// CERO cambios en el juego. Eso es lo que te compra esta arquitectura.
    /// </summary>
    public interface IChatSource
    {
        /// <summary>
        /// Se dispara cuando llega un comando válido del chat.
        /// IMPORTANTE: se invoca desde el hilo de red del adaptador, NO desde
        /// el hilo del juego. Por eso el Core encola en vez de actuar aquí.
        /// </summary>
        event Action<ChatCommand> OnCommand;

        void Connect();
        void Disconnect();
    }
}
