using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Asset.SceneManagement;
using ProjectBase.Common.Patterns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace SteamCore.Networking
{
    public class NetworkSceneFlow : MonoBehaviour
    {
        [SerializeField] private AssetReference _menuScene;
        [SerializeField] private AssetReference _gameplayScene;

        public async UniTask TransitionToGameplayAsync(CancellationToken ct)
        {
#if MIRROR
            var networkManager = Mirror.NetworkManager.singleton as NetworkGameManager;
            if (networkManager != null && networkManager.IsServer)
            {
                var sceneName = _gameplayScene.RuntimeKey.ToString();
                EventBus.Publish(new NetworkSceneChangeStartedEvent { SceneName = sceneName });
                networkManager.ServerChangeScene(sceneName);
                return;
            }
#endif
            await SceneLoader.Instance.LoadSceneAsync(_gameplayScene, LoadSceneMode.Single, ct: ct);
        }

        public async UniTask ReturnToMenuAsync(CancellationToken ct)
        {
#if MIRROR
            var networkManager = Mirror.NetworkManager.singleton as NetworkGameManager;
            if (networkManager != null && networkManager.IsOnline)
            {
                networkManager.Disconnect();
                await UniTask.Yield(ct);
            }
#endif
            await SceneLoader.Instance.LoadSceneAsync(_menuScene, LoadSceneMode.Single, ct: ct);
        }

        public async UniTask LoadOfflineGameplayAsync(CancellationToken ct)
        {
            await SceneLoader.Instance.LoadSceneAsync(_gameplayScene, LoadSceneMode.Single, ct: ct);
        }
    }
}
