using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.GameFlow;
using SteamCore.Steam;

namespace SteamCore.Networking
{
    public class InitializeNetworkTask : IBootstrapTask
    {
        private readonly NetworkGameManager _networkManager;
        private readonly NetworkObjectPool _objectPool;

        public string Name => "Setting up networking";
        public float Weight => 0.15f;

        public InitializeNetworkTask(NetworkGameManager networkManager, NetworkObjectPool objectPool)
        {
            _networkManager = networkManager;
            _objectPool = objectPool;
        }

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            progress?.Report(0f);

            if (_networkManager != null)
            {
                ISteamAuth authService = null;
                ISteamLobby lobbyService = null;

                if (SteamManager.Instance != null && SteamManager.Instance.IsInitialized)
                {
#if STEAMWORKS_NET
                    authService = new SteamAuthService();
                    lobbyService = new SteamLobbyService(SteamManager.Instance.Config);
#endif
                }

                _networkManager.Initialize(authService, lobbyService);
            }

            progress?.Report(0.6f);

            if (_objectPool != null)
                _objectPool.RegisterPrefabsWithMirror();

            progress?.Report(1f);
            await UniTask.CompletedTask;
        }
    }
}
