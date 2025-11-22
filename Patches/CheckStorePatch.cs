using SDG.Unturned;
using Steamworks;

namespace BackTurnedSharedStorage.Patches
{
    public class CheckStorePatch
    {
        public static bool Prefix(InteractableStorage __instance, CSteamID enemyPlayer, CSteamID enemyGroup, ref bool __result)
        {
            try
            {
                if (SharedStoragePlugin.Instance == null) return true;

                if (!SharedStoragePlugin.Instance.Configuration.Instance.Enabled) return true;

                var player = PlayerTool.getPlayer(enemyPlayer);
                if (player == null) return true;

                bool canUse = SharedStoragePlugin.Instance.CanPlayerUseStorage(player, __instance);

                if (!canUse)
                {
                    __result = false;
                    return false;
                }

                int currentUsers = SharedStoragePlugin.Instance.GetStorageUserCount(__instance);
                if (currentUsers > 0)
                {
                    __result = true;
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"[BackTurnedSharedStorage] Error in CheckStore patch: {ex}");
            }

            return true;
        }
    }
}