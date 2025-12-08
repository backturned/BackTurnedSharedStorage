using Rocket.API;

namespace BackTurnedSharedStorage
{
    public class SharedStorageConfiguration : IRocketPluginConfiguration
    {
        public bool Enabled { get; set; }
        public int MaxPlayersPerStorage { get; set; }

        public void LoadDefaults()
        {
            Enabled = true; // Enables or disables the shared storage functionality. true = multiple players can open the same storage, false = only one player per storage
            MaxPlayersPerStorage = 5; // Maximum number of players that can use the same storage simultaneously. Recommended value between 2 and 10
        }
    }
}