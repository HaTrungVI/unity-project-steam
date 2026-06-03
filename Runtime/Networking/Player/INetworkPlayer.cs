namespace SteamCore.Networking
{
    public interface INetworkPlayer
    {
        uint NetId { get; }
        bool IsLocalPlayer { get; }
        bool IsServer { get; }
        ulong SteamId { get; }
        string DisplayName { get; }
        bool IsReady { get; }
    }
}
