namespace SteamCore.Steam
{
    public interface ISteamManager
    {
        bool IsInitialized { get; }
        bool IsOnline { get; }
        ulong LocalSteamId { get; }
        string LocalDisplayName { get; }
    }
}
