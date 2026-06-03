using ProjectBase.Common.Patterns;
using UnityEngine;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamManager : MonoSingleton<SteamManager>, ISteamManager
    {
        [SerializeField] private SteamConfig _config;

        private bool _isInitialized;

#if STEAMWORKS_NET
        private Callback<GameOverlayActivated_t> _overlayCallback;
#endif

        public bool IsInitialized => _isInitialized;
        public bool IsOnline => _isInitialized;
        public SteamConfig Config => _config;

        public ulong LocalSteamId
        {
            get
            {
#if STEAMWORKS_NET
                return _isInitialized ? SteamUser.GetSteamID().m_SteamID : 0;
#else
                return 0;
#endif
            }
        }

        public string LocalDisplayName
        {
            get
            {
#if STEAMWORKS_NET
                return _isInitialized ? SteamFriends.GetPersonaName() : "Offline Player";
#else
                return "Offline Player";
#endif
            }
        }

        protected override void OnInitialized()
        {
#if STEAMWORKS_NET
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(_config != null ? _config.AppId : 480)))
            {
                Application.Quit();
                return;
            }

            _isInitialized = SteamAPI.Init();
            if (!_isInitialized)
            {
                Debug.LogWarning("[SteamManager] Steam API init failed. Running in offline mode.");
                EventBus.Publish(new SteamInitFailedEvent { Reason = "SteamAPI.Init() returned false" });
                return;
            }

            _overlayCallback = Callback<GameOverlayActivated_t>.Create(OnOverlayActivated);

            Debug.Log($"[SteamManager] Steam initialized. User: {LocalDisplayName} ({LocalSteamId})");
            EventBus.Publish(new SteamInitializedEvent());
#else
            Debug.Log("[SteamManager] Steamworks.NET not imported. Running in offline mode.");
            _isInitialized = false;
#endif
        }

        private void Update()
        {
#if STEAMWORKS_NET
            if (_isInitialized)
                SteamAPI.RunCallbacks();
#endif
        }

        private void OnApplicationQuit()
        {
#if STEAMWORKS_NET
            if (_isInitialized)
            {
                EventBus.Publish(new SteamShutdownEvent());
                SteamAPI.Shutdown();
                _isInitialized = false;
            }
#endif
        }

#if STEAMWORKS_NET
        private void OnOverlayActivated(GameOverlayActivated_t result)
        {
            EventBus.Publish(new SteamOverlayActivatedEvent { IsActive = result.m_bActive != 0 });
        }
#endif
    }
}
