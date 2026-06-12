using System.Collections.Concurrent;
using PvZChatInvasion.Domain;

namespace PvZChatInvasion.Infrastructure
{
    /// <summary>
    /// PUENTE entre dos hilos:
    ///   - el hilo de RED (TwitchLib) que produce comandos, y
    ///   - el hilo del JUEGO (Unity/MelonLoader OnUpdate) que los consume.
    ///
    /// Este es EL detalle que más gente quema: tocar ReplantAPI/Unity desde el
    /// callback de Twitch (otro hilo) provoca CRASH. La solución es esta cola
    /// thread-safe: los adaptadores ENCOLAN desde su hilo, y el Core DRENA en
    /// OnUpdate, que ya corre en el hilo seguro del juego.
    /// </summary>
    public sealed class CommandQueue
    {
        private readonly ConcurrentQueue<ChatCommand> _queue = new ConcurrentQueue<ChatCommand>();

        /// <summary>Llamado desde el hilo de red del adaptador.</summary>
        public void Enqueue(ChatCommand command) => _queue.Enqueue(command);

        /// <summary>Llamado desde el hilo del juego (OnUpdate).</summary>
        public bool TryDequeue(out ChatCommand command) => _queue.TryDequeue(out command);

        public bool IsEmpty => _queue.IsEmpty;

        /// <summary>Descarta todo lo pendiente (nivel terminado o sin partida activa).</summary>
        public void Clear()
        {
            while (_queue.TryDequeue(out _)) { }
        }
    }
}
