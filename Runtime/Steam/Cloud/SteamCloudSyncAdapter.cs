using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Data.Core;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamCloudSyncAdapter : IDataSyncAdapter, ISteamCloudService, System.IDisposable
    {
        public bool IsOnline => SteamManager.Instance != null
                                && SteamManager.Instance.IsInitialized
                                && IsCloudEnabled;

        public bool IsCloudEnabled
        {
            get
            {
#if STEAMWORKS_NET
                return SteamRemoteStorage.IsCloudEnabledForAccount()
                       && SteamRemoteStorage.IsCloudEnabledForApp();
#else
                return false;
#endif
            }
        }

        public async UniTask<string> LoadFromServerAsync(string moduleId, CancellationToken ct = default)
        {
#if STEAMWORKS_NET
            var filename = $"save_{moduleId}.json";
            if (!SteamRemoteStorage.FileExists(filename))
                return null;

            var size = SteamRemoteStorage.GetFileSize(filename);
            var buffer = new byte[size];
            SteamRemoteStorage.FileRead(filename, buffer, size);
            return System.Text.Encoding.UTF8.GetString(buffer);
#else
            await UniTask.CompletedTask;
            return null;
#endif
        }

        public async UniTask<bool> SaveToServerAsync(string moduleId, string jsonData, CancellationToken ct = default)
        {
#if STEAMWORKS_NET
            var filename = $"save_{moduleId}.json";
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            return SteamRemoteStorage.FileWrite(filename, bytes, bytes.Length);
#else
            await UniTask.CompletedTask;
            return false;
#endif
        }

        public bool FileExists(string filename)
        {
#if STEAMWORKS_NET
            return SteamRemoteStorage.FileExists(filename);
#else
            return false;
#endif
        }

        public byte[] ReadFile(string filename)
        {
#if STEAMWORKS_NET
            if (!SteamRemoteStorage.FileExists(filename)) return null;
            var size = SteamRemoteStorage.GetFileSize(filename);
            var buffer = new byte[size];
            SteamRemoteStorage.FileRead(filename, buffer, size);
            return buffer;
#else
            return null;
#endif
        }

        public bool WriteFile(string filename, byte[] data)
        {
#if STEAMWORKS_NET
            return SteamRemoteStorage.FileWrite(filename, data, data.Length);
#else
            return false;
#endif
        }

        public bool DeleteFile(string filename)
        {
#if STEAMWORKS_NET
            return SteamRemoteStorage.FileDelete(filename);
#else
            return false;
#endif
        }

        public void Dispose() { }
    }
}
