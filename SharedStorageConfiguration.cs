using Rocket.API;

namespace BackTurnedSharedStorage
{
    public class SharedStorageConfiguration : IRocketPluginConfiguration
    {
        public bool Enabled { get; set; }
        public int MaxPlayersPerStorage { get; set; }

        public void LoadDefaults()
        {
            Enabled = true;
            MaxPlayersPerStorage = 4;
        }
    }
}