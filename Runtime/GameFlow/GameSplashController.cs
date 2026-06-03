using ProjectBase.Common.Patterns;
using ProjectBase.Data.Core;
using ProjectBase.GameFlow;
using ProjectBase.GameFlow.Tasks;
using SteamCore.Networking;
using SteamCore.Steam;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteamCore.GameFlow
{
    public class GameSplashController : BaseSplashController
    {
        [Header("Data")]
        [SerializeField] private DataRegistry _dataRegistry;

        [Header("Networking")]
        [SerializeField] private NetworkGameManager _networkGameManager;
        [SerializeField] private NetworkObjectPool _networkObjectPool;

        protected override void SetupTasks()
        {
            AddTask(new InitializeServicesTask());
            AddTask(new InitializeSteamTask());
            AddTask(new InitializeDataTask(_dataRegistry));
            AddTask(new InitializeNetworkTask(_networkGameManager, _networkObjectPool));
            AddTask(new PreloadAssetsTask(Config.PreloadAssets));
            AddTask(new LoadSceneTask(Config.MenuScene, LoadSceneMode.Additive, "Loading menu"));
        }

        protected override void OnProgressUpdated(float normalizedProgress, string taskName)
        {
            EventBus.Publish(new BootstrapProgressEvent
            {
                Progress = normalizedProgress,
                TaskName = taskName
            });
        }

        protected override void OnAllCompleted()
        {
            EventBus.Publish(new BootstrapCompletedEvent());
        }
    }
}
