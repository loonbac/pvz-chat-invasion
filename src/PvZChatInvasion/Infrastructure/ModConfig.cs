using System.IO;
using MelonLoader;
using MelonLoader.Utils;

namespace PvZChatInvasion.Infrastructure
{
    /// <summary>
    /// Configuración del mod vía MelonPreferences. Vive en su PROPIO archivo:
    ///   [carpeta del juego]/UserData/PvZChatInvasion.cfg
    /// El usuario lo edita con cualquier editor de texto — sin recompilar.
    /// F9 dentro del juego lo recarga en caliente (ver Core).
    /// </summary>
    public sealed class ModConfig
    {
        private readonly MelonPreferences_Category _category;
        private readonly MelonPreferences_Entry<bool> _enableTwitch;
        private readonly MelonPreferences_Entry<string> _twitchChannel;
        private readonly MelonPreferences_Entry<double> _spawnCooldown;
        private readonly MelonPreferences_Entry<bool> _useSsl;

        public ModConfig()
        {
            _category = MelonPreferences.CreateCategory("PvZChatInvasion", "PvZ Chat Invasion");
            // Archivo dedicado: mas facil de encontrar que MelonPreferences.cfg.
            _category.SetFilePath(
                Path.Combine(MelonEnvironment.UserDataDirectory, "PvZChatInvasion.cfg"));

            _enableTwitch = _category.CreateEntry(
                "EnableTwitch", true,
                description: "true = conectar al chat de Twitch al iniciar. false = solo modo local (F8).");

            _twitchChannel = _category.CreateEntry(
                "TwitchChannel", "",
                description: "Nombre de tu canal de Twitch, sin #. Ejemplo: \"loonbac\". Vacio = no conecta.");

            _spawnCooldown = _category.CreateEntry(
                "SpawnCooldownSeconds", 1.5,
                description: "Segundos minimos entre spawns de zombies pedidos por el chat.");

            _useSsl = _category.CreateEntry(
                "TwitchUseSsl", false,
                description: "TLS hacia Twitch. DEJAR en false bajo Wine/Proton (su SChannel falla el handshake). En Windows nativo puede ser true.");

            // Materializa el archivo con los valores por defecto la primera vez.
            _category.SaveToFile(false);
        }

        public bool EnableTwitch => _enableTwitch.Value;

        /// <summary>Canal normalizado: sin espacios, sin '#', en minúsculas (IRC lo exige).</summary>
        public string TwitchChannel
        {
            get
            {
                string raw = _twitchChannel.Value;
                if (string.IsNullOrWhiteSpace(raw)) return "";
                return raw.Trim().TrimStart('#').ToLowerInvariant();
            }
        }

        public double SpawnCooldownSeconds => _spawnCooldown.Value;

        public bool TwitchUseSsl => _useSsl.Value;

        /// <summary>Relee el archivo desde disco (hot-reload con F9).</summary>
        public void Reload() => _category.LoadFromFile(false);
    }
}
