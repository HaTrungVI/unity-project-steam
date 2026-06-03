using System;

namespace SteamCore.Steam
{
    public interface ISteamAchievements : IDisposable
    {
        bool IsReady { get; }
        bool UnlockAchievement(string achievementId);
        bool IsAchievementUnlocked(string achievementId);
        void SetStat(string statName, int value);
        void SetStat(string statName, float value);
        int GetStatInt(string statName);
        float GetStatFloat(string statName);
        void FlushStats();
    }
}
