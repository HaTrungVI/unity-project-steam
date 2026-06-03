namespace SteamCore.Steam
{
    public interface ISteamCloudService
    {
        bool IsCloudEnabled { get; }
        bool FileExists(string filename);
        byte[] ReadFile(string filename);
        bool WriteFile(string filename, byte[] data);
        bool DeleteFile(string filename);
    }
}
