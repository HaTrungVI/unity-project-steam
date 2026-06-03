using System;
using ProjectBase.Common.Patterns;
using UnityEngine;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamAchievementService : ISteamAchievements
    {
        private readonly float _storeInterval;
        private bool _isReady;
        private bool _statsDirty;
        private float _lastStoreTime;

#if STEAMWORKS_NET
        private Callback<UserStatsReceived_t> _statsReceivedCallback;
        private Callback<UserStatsStored_t> _statsStoredCallback;
#endif

        public bool IsReady => _isReady;

        public SteamAchievementService(float storeInterval = 5f)
        {
            _storeInterval = storeInterval;

#if STEAMWORKS_NET
            _statsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
            _statsStoredCallback = Callback<UserStatsStored_t>.Create(OnStatsStored);
            // New Steamworks API auto-syncs stats before game process starts
            _isReady = SteamManager.Instance.IsInitialized;
#endif
        }

        public bool UnlockAchievement(string achievementId)
        {
#if STEAMWORKS_NET
            if (!_isReady) return false;

            bool alreadyUnlocked;
            SteamUserStats.GetAchievement(achievementId, out alreadyUnlocked);
            if (alreadyUnlocked) return false;

            SteamUserStats.SetAchievement(achievementId);
            _statsDirty = true;
            EventBus.Publish(new SteamAchievementUnlockedEvent { AchievementId = achievementId });
            TryFlushIfInterval();
            return true;
#else
            return false;
#endif
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
#if STEAMWORKS_NET
            if (!_isReady) return false;
            SteamUserStats.GetAchievement(achievementId, out bool unlocked);
            return unlocked;
#else
            return false;
#endif
        }

        public void SetStat(string statName, int value)
        {
#if STEAMWORKS_NET
            if (!_isReady) return;
            SteamUserStats.SetStat(statName, value);
            _statsDirty = true;
            TryFlushIfInterval();
#endif
        }

        public void SetStat(string statName, float value)
        {
#if STEAMWORKS_NET
            if (!_isReady) return;
            SteamUserStats.SetStat(statName, value);
            _statsDirty = true;
            TryFlushIfInterval();
#endif
        }

        public int GetStatInt(string statName)
        {
#if STEAMWORKS_NET
            if (!_isReady) return 0;
            SteamUserStats.GetStat(statName, out int value);
            return value;
#else
            return 0;
#endif
        }

        public float GetStatFloat(string statName)
        {
#if STEAMWORKS_NET
            if (!_isReady) return 0f;
            SteamUserStats.GetStat(statName, out float value);
            return value;
#else
            return 0f;
#endif
        }

        public void FlushStats()
        {
#if STEAMWORKS_NET
            if (!_statsDirty || !_isReady) return;
            SteamUserStats.StoreStats();
            _statsDirty = false;
            _lastStoreTime = Time.realtimeSinceStartup;
#endif
        }

        public void Dispose()
        {
            FlushStats();
#if STEAMWORKS_NET
            _statsReceivedCallback = null;
            _statsStoredCallback = null;
#endif
        }

        private void TryFlushIfInterval()
        {
            if (Time.realtimeSinceStartup - _lastStoreTime >= _storeInterval)
                FlushStats();
        }

#if STEAMWORKS_NET
        private void OnStatsReceived(UserStatsReceived_t result)
        {
            _isReady = result.m_eResult == EResult.k_EResultOK;
            EventBus.Publish(new SteamStatsReceivedEvent { Success = _isReady });
        }

        private void OnStatsStored(UserStatsStored_t result)
        {
            EventBus.Publish(new SteamStatsStoredEvent
            {
                Success = result.m_eResult == EResult.k_EResultOK
            });
        }
#endif
    }
}
