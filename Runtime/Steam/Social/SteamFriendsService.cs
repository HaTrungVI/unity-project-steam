using UnityEngine;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamFriendsService : ISteamFriends
    {
#if STEAMWORKS_NET
        private Callback<PersonaStateChange_t> _personaStateCallback;
#endif

        public SteamFriendsService()
        {
#if STEAMWORKS_NET
            _personaStateCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
#endif
        }

        public void SetRichPresence(string key, string value)
        {
#if STEAMWORKS_NET
            SteamFriends.SetRichPresence(key, value);
#endif
        }

        public void ClearRichPresence()
        {
#if STEAMWORKS_NET
            SteamFriends.ClearRichPresence();
#endif
        }

        public int GetFriendCount()
        {
#if STEAMWORKS_NET
            return SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
#else
            return 0;
#endif
        }

        public ulong GetFriendByIndex(int index)
        {
#if STEAMWORKS_NET
            return SteamFriends.GetFriendByIndex(index, EFriendFlags.k_EFriendFlagImmediate).m_SteamID;
#else
            return 0;
#endif
        }

        public string GetFriendPersonaName(ulong friendId)
        {
#if STEAMWORKS_NET
            return SteamFriends.GetFriendPersonaName(new CSteamID(friendId));
#else
            return "Unknown";
#endif
        }

        public void ActivateGameOverlay(string dialogType)
        {
#if STEAMWORKS_NET
            SteamFriends.ActivateGameOverlay(dialogType);
#endif
        }

        public void ActivateGameOverlayToUser(string dialogType, ulong steamId)
        {
#if STEAMWORKS_NET
            SteamFriends.ActivateGameOverlayToUser(dialogType, new CSteamID(steamId));
#endif
        }

        public int GetPlayerSteamLevel()
        {
#if STEAMWORKS_NET
            return SteamUser.GetPlayerSteamLevel();
#else
            return 0;
#endif
        }

        public string GetPersonaStateName()
        {
#if STEAMWORKS_NET
            var state = SteamFriends.GetPersonaState();
            return state switch
            {
                EPersonaState.k_EPersonaStateOnline => "Online",
                EPersonaState.k_EPersonaStateBusy => "Busy",
                EPersonaState.k_EPersonaStateAway => "Away",
                EPersonaState.k_EPersonaStateSnooze => "Snooze",
                EPersonaState.k_EPersonaStateLookingToTrade => "Looking to Trade",
                EPersonaState.k_EPersonaStateLookingToPlay => "Looking to Play",
                EPersonaState.k_EPersonaStateInvisible => "Invisible",
                _ => "Offline"
            };
#else
            return "Offline";
#endif
        }

        public int GetOnlineFriendCount()
        {
#if STEAMWORKS_NET
            int total = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            int online = 0;
            for (int i = 0; i < total; i++)
            {
                var friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                var state = SteamFriends.GetFriendPersonaState(friendId);
                if (state != EPersonaState.k_EPersonaStateOffline)
                    online++;
            }
            return online;
#else
            return 0;
#endif
        }

        public Texture2D GetLocalPlayerAvatar()
        {
            return SteamAvatarUtility.GetAvatar(SteamManager.Instance.LocalSteamId);
        }

        public void Dispose()
        {
#if STEAMWORKS_NET
            _personaStateCallback = null;
            ClearRichPresence();
#endif
        }

#if STEAMWORKS_NET
        private void OnPersonaStateChanged(PersonaStateChange_t result)
        {
            ProjectBase.Common.Patterns.EventBus.Publish(new SteamFriendStateChangedEvent
            {
                SteamId = result.m_ulSteamID
            });
        }
#endif
    }
}
