namespace SteamCore.Steam
{
    public struct SteamInitializedEvent { }
    public struct SteamInitFailedEvent { public string Reason; }
    public struct SteamShutdownEvent { }
    public struct SteamOverlayActivatedEvent { public bool IsActive; }

    public struct LobbyCreatedEvent { public ulong LobbyId; }
    public struct LobbyJoinedEvent { public ulong LobbyId; }
    public struct LobbyLeftEvent { public ulong LobbyId; }
    public struct LobbyJoinRequestedEvent { public ulong LobbyId; public ulong FriendId; }
    public struct LobbyMemberChangedEvent
    {
        public ulong LobbyId;
        public ulong MemberId;
        public LobbyMemberChangeType ChangeType;
    }
    public struct LobbyDataChangedEvent { public ulong LobbyId; }
    public struct SteamAuthValidatedEvent { public ulong SteamId; public bool Success; }

    public struct SteamFriendStateChangedEvent { public ulong SteamId; }

    public struct SteamAchievementUnlockedEvent { public string AchievementId; }
    public struct SteamStatsReceivedEvent { public bool Success; }
    public struct SteamStatsStoredEvent { public bool Success; }

    public enum LobbyMemberChangeType
    {
        Joined,
        Left,
        Disconnected,
        Kicked,
        Banned
    }
}
