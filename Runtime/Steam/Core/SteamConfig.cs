using UnityEngine;

namespace SteamCore.Steam
{
    [CreateAssetMenu(menuName = "SteamCore/Config/Steam Config")]
    public class SteamConfig : ScriptableObject
    {
        [Header("Steam App")]
        [SerializeField] private uint _appId = 480;
        [SerializeField] private string _gameVersion = "0.1.0";

        [Header("Lobby")]
        [SerializeField] private int _defaultLobbyType = 1;
        [SerializeField, Range(2, 8)] private int _maxLobbyMembers = 4;

        [Header("Stats")]
        [SerializeField] private float _statsStoreInterval = 5f;

        [Header("Cloud")]
        [SerializeField] private bool _enableCloudSave = true;

        public uint AppId => _appId;
        public string GameVersion => _gameVersion;
        public int DefaultLobbyType => _defaultLobbyType;
        public int MaxLobbyMembers => _maxLobbyMembers;
        public float StatsStoreInterval => _statsStoreInterval;
        public bool EnableCloudSave => _enableCloudSave;
    }
}
