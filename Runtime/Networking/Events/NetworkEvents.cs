namespace SteamCore.Networking
{
    public struct ServerStartedEvent { }
    public struct ServerStoppedEvent { }

    public struct ClientJoinedServerEvent
    {
        public int ConnectionId;
        public ulong SteamId;
    }

    public struct ClientLeftServerEvent
    {
        public int ConnectionId;
        public ulong SteamId;
        public string Reason;
    }

    public struct LocalClientConnectedEvent { }

    public struct LocalClientDisconnectedEvent
    {
        public string Reason;
    }

    public struct LocalPlayerSpawnedEvent
    {
#if MIRROR
        public NetworkPlayerController Player;
#endif
    }

    public struct PlayerJoinedEvent
    {
#if MIRROR
        public NetworkPlayerController Player;
#endif
    }

    public struct PlayerLeftEvent
    {
#if MIRROR
        public NetworkPlayerController Player;
#endif
    }

    public struct AllPlayersReadyEvent { }

    public struct PlayerInfoChangedEvent
    {
#if MIRROR
        public NetworkPlayerController Player;
#endif
    }

    public struct PlayerReadyChangedEvent
    {
#if MIRROR
        public NetworkPlayerController Player;
#endif
        public bool IsReady;
    }

    public struct NetworkSceneChangeStartedEvent
    {
        public string SceneName;
    }

    public struct NetworkSceneChangeCompletedEvent
    {
        public string SceneName;
    }
}
