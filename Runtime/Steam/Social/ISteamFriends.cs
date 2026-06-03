using System;
using UnityEngine;

namespace SteamCore.Steam
{
    public interface ISteamFriends : IDisposable
    {
        void SetRichPresence(string key, string value);
        void ClearRichPresence();
        int GetFriendCount();
        ulong GetFriendByIndex(int index);
        string GetFriendPersonaName(ulong friendId);
        void ActivateGameOverlay(string dialogType);
        void ActivateGameOverlayToUser(string dialogType, ulong steamId);
        int GetPlayerSteamLevel();
        string GetPersonaStateName();
        int GetOnlineFriendCount();
        Texture2D GetLocalPlayerAvatar();
    }
}
