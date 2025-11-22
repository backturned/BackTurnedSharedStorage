using HarmonyLib;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using BackTurnedSharedStorage.Patches;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BackTurnedSharedStorage
{
    public class SharedStoragePlugin : RocketPlugin<SharedStorageConfiguration>
    {
        public static SharedStoragePlugin Instance;
        private Harmony harmony;

        private readonly Dictionary<CSteamID, InteractableStorage> playerUsingStorage = new Dictionary<CSteamID, InteractableStorage>();
        private readonly Dictionary<InteractableStorage, List<CSteamID>> storageUsers = new Dictionary<InteractableStorage, List<CSteamID>>();

        // Read-only methods
        public bool IsPlayerUsingStorage(CSteamID steamId) => playerUsingStorage.ContainsKey(steamId);
        public InteractableStorage GetPlayerStorage(CSteamID steamId) => playerUsingStorage[steamId];
        public bool TryGetPlayerStorage(CSteamID steamId, out InteractableStorage storage) =>
            playerUsingStorage.TryGetValue(steamId, out storage);

        public IReadOnlyList<CSteamID> GetStorageUsers(InteractableStorage storage) =>
            storageUsers.ContainsKey(storage) ? storageUsers[storage].AsReadOnly() : new List<CSteamID>().AsReadOnly();

        public int GetStorageUserCount(InteractableStorage storage) =>
            storageUsers.ContainsKey(storage) ? storageUsers[storage].Count : 0;

        public bool IsStorageBeingUsed(InteractableStorage storage) =>
            storageUsers.ContainsKey(storage) && storageUsers[storage].Count > 0;

        protected override void Load()
        {
            Instance = this;
            harmony = new Harmony("com.BackTurnedSharedStorage.plugin");

            try
            {
                var storageType = typeof(InteractableStorage);
                var checkStoreMethod = storageType.GetMethod("checkStore", BindingFlags.Public | BindingFlags.Instance);

                if (checkStoreMethod != null)
                {
                    harmony.Patch(checkStoreMethod,
                        prefix: new HarmonyMethod(typeof(CheckStorePatch).GetMethod("Prefix")));
                    Rocket.Core.Logging.Logger.Log("BackTurnedSharedStorage: checkStore method successfully patched!");
                }

                Rocket.Core.Logging.Logger.Log("BackTurnedSharedStorage plugin successfully loaded!");
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"Error loading BackTurnedSharedStorage: {ex}");
            }
        }

        protected override void Unload()
        {
            harmony?.UnpatchAll();

            foreach (var steamId in new List<CSteamID>(playerUsingStorage.Keys))
            {
                var player = UnturnedPlayer.FromCSteamID(steamId);
                if (player?.Player?.inventory != null && player.Player.inventory.isStoring)
                {
                    player.Player.inventory.closeStorage();
                }
            }

            playerUsingStorage.Clear();
            storageUsers.Clear();

            Rocket.Core.Logging.Logger.Log("BackTurnedSharedStorage plugin successfully unloaded!");
        }

        public bool CanPlayerUseStorage(Player player, InteractableStorage storage)
        {
            try
            {
                if (!Configuration.Instance.Enabled) return true;

                if (storageUsers.ContainsKey(storage))
                {
                    int currentUsers = storageUsers[storage].Count;

                    if (currentUsers >= Configuration.Instance.MaxPlayersPerStorage)
                    {
                        return false;
                    }

                    var uPlayer = UnturnedPlayer.FromPlayer(player);
                    if (uPlayer != null && playerUsingStorage.ContainsKey(uPlayer.CSteamID) &&
                        playerUsingStorage[uPlayer.CSteamID] != storage)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"[BackTurnedSharedStorage] Error in CanPlayerUseStorage: {ex}");
                return true;
            }
        }

        public void OnStorageOpened(Player player, InteractableStorage storage)
        {
            try
            {
                if (!Configuration.Instance.Enabled) return;

                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                if (uPlayer == null) return;

                if (!storageUsers.ContainsKey(storage))
                {
                    storageUsers[storage] = new List<CSteamID>();
                }

                if (!storageUsers[storage].Contains(uPlayer.CSteamID))
                {
                    storageUsers[storage].Add(uPlayer.CSteamID);
                }

                playerUsingStorage[uPlayer.CSteamID] = storage;
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"[BackTurnedSharedStorage] Error in OnStorageOpened: {ex}");
            }
        }

        public void OnStorageClosed(Player player)
        {
            try
            {
                if (!Configuration.Instance.Enabled) return;

                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                if (uPlayer == null) return;

                if (playerUsingStorage.ContainsKey(uPlayer.CSteamID))
                {
                    InteractableStorage storage = playerUsingStorage[uPlayer.CSteamID];

                    if (storageUsers.ContainsKey(storage))
                    {
                        storageUsers[storage].Remove(uPlayer.CSteamID);

                        if (storageUsers[storage].Count == 0)
                        {
                            storageUsers.Remove(storage);
                        }
                    }

                    playerUsingStorage.Remove(uPlayer.CSteamID);
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"[BackTurnedSharedStorage] Error in OnStorageClosed: {ex}");
            }
        }

        public void Update()
        {
            try
            {
                if (!Configuration.Instance.Enabled) return;

                foreach (var steamPlayer in Provider.clients)
                {
                    if (steamPlayer?.player != null)
                    {
                        var player = steamPlayer.player;
                        var inventory = player.inventory;
                        var uPlayer = UnturnedPlayer.FromPlayer(player);

                        if (uPlayer == null) continue;

                        if (inventory.isStoring && inventory.storage != null)
                        {
                            if (!playerUsingStorage.ContainsKey(uPlayer.CSteamID) ||
                                playerUsingStorage[uPlayer.CSteamID] != inventory.storage)
                            {
                                OnStorageOpened(player, inventory.storage);
                            }
                        }
                        else if (!inventory.isStoring && playerUsingStorage.ContainsKey(uPlayer.CSteamID))
                        {
                            OnStorageClosed(player);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silent catch for update loop
            }
        }
    }
}