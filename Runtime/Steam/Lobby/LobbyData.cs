namespace SteamCore.Steam
{
    public readonly struct LobbyInfo
    {
        public readonly ulong LobbyId;
        public readonly string HostName;
        public readonly string MapName;
        public readonly string GameVersion;
        public readonly int CurrentMembers;
        public readonly int MaxMembers;

        public LobbyInfo(ulong lobbyId, string hostName, string mapName,
            string gameVersion, int currentMembers, int maxMembers)
        {
            LobbyId = lobbyId;
            HostName = hostName;
            MapName = mapName;
            GameVersion = gameVersion;
            CurrentMembers = currentMembers;
            MaxMembers = maxMembers;
        }
    }

    public readonly struct LobbySearchFilter
    {
        public readonly string RequiredVersion;
        public readonly int MinSlotsAvailable;

        public LobbySearchFilter(string requiredVersion, int minSlotsAvailable = 1)
        {
            RequiredVersion = requiredVersion;
            MinSlotsAvailable = minSlotsAvailable;
        }
    }

    public static class LobbyDataKeys
    {
        public const string GameVersion = "game_version";
        public const string HostAddress = "host_address";
        public const string HostName = "host_name";
        public const string MapName = "map_name";
        public const string GameMode = "game_mode";
        public const string GameState = "game_state";
    }
}
