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
                }

                var inventoryType = typeof(PlayerInventory);
                var openStorageMethod = inventoryType.GetMethod("openStorage", BindingFlags.Public | BindingFlags.Instance);
                var closeStorageMethod = inventoryType.GetMethod("closeStorage", BindingFlags.Public | BindingFlags.Instance);

                if (openStorageMethod != null)
                {
                    harmony.Patch(openStorageMethod,
                        postfix: new HarmonyMethod(typeof(SharedStoragePlugin).GetMethod("Postfix_OpenStorage", BindingFlags.NonPublic | BindingFlags.Static)));
                }

                if (closeStorageMethod != null)
                {
                    harmony.Patch(closeStorageMethod,
                        postfix: new HarmonyMethod(typeof(SharedStoragePlugin).GetMethod("Postfix_CloseStorage", BindingFlags.NonPublic | BindingFlags.Static)));
                }

                Provider.onEnemyDisconnected += OnPlayerDisconnected;

                Rocket.Core.Logging.Logger.Log("BackTurned | SharedStorage: Plugin successfully loaded!");
                Rocket.Core.Logging.Logger.Log("BackTurned | SharedStorage: You can find more plugins in our Discord channel - https://discord.gg/daysdyHZ7f");
                Rocket.Core.Logging.Logger.Log("BackTurned | SharedStorage: If you found a bug in the plugin or have suggestions for plugins, you can write in Discord");
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error loading {ex}");
            }
        }

        protected override void Unload()
        {
            harmony?.UnpatchAll();
            Provider.onEnemyDisconnected -= OnPlayerDisconnected;

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

            Rocket.Core.Logging.Logger.Log("BackTurned | SharedStorage: plugin successfully unloaded!");
        }
        private void OnPlayerDisconnected(SteamPlayer player)
        {
            try
            {
                if (player?.playerID.steamID != null)
                {
                    OnStorageClosed(player.playerID.steamID);
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in OnPlayerDisconnected: {ex}");
            }
        }

        private static void Postfix_OpenStorage(PlayerInventory __instance, InteractableStorage newStorage)
        {
            try
            {
                if (Instance == null || !Instance.Configuration.Instance.Enabled) return;

                if (__instance?.player != null && newStorage != null)
                {
                    Instance.OnStorageOpened(__instance.player, newStorage);
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in Postfix_OpenStorage: {ex}");
            }
        }

        private static void Postfix_CloseStorage(PlayerInventory __instance)
        {
            try
            {
                if (Instance == null || !Instance.Configuration.Instance.Enabled) return;

                if (__instance?.player != null)
                {
                    Instance.OnStorageClosed(__instance.player);
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in Postfix_CloseStorage: {ex}");
            }
        }

        private void OnStorageClosed(CSteamID steamId)
        {
            try
            {
                if (!Configuration.Instance.Enabled) return;

                if (playerUsingStorage.ContainsKey(steamId))
                {
                    InteractableStorage storage = playerUsingStorage[steamId];

                    if (storageUsers.ContainsKey(storage))
                    {
                        storageUsers[storage].Remove(steamId);

                        if (storageUsers[storage].Count == 0)
                        {
                            storageUsers.Remove(storage);
                        }
                    }

                    playerUsingStorage.Remove(steamId);
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in OnStorageClosed: {ex}");
            }
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
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in CanPlayerUseStorage: {ex}");
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
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in OnStorageOpened: {ex}");
            }
        }

        public void OnStorageClosed(Player player)
        {
            try
            {
                if (!Configuration.Instance.Enabled) return;

                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                if (uPlayer == null) return;

                OnStorageClosed(uPlayer.CSteamID);
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"BackTurned | SharedStorage: Error in OnStorageClosed: {ex}");
            }
        }
    }
}