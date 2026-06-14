using SteamCore.Steam;

namespace SteamCore.Networking
{
    public interface INetworkGameManager
    {
        bool IsHost { get; }
        bool IsClient { get; }
        bool IsServer { get; }
        bool IsOnline { get; }
        bool IsSteamTransport { get; }
        int ConnectedPlayerCount { get; }

        void Initialize(ISteamAuth authService, ISteamLobby lobbyService);
        void StartHosting();
        void JoinGame(string hostAddress);
        void Disconnect();
    }
}
