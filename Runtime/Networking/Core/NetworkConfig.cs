using System.Collections.Generic;
using UnityEngine;

namespace SteamCore.Networking
{
    [CreateAssetMenu(menuName = "SteamCore/Config/Network Config")]
    public class NetworkConfig : ScriptableObject
    {
        [Header("Server")]
        [SerializeField, Range(2, 8)] private int _maxConnections = 4;
        [SerializeField] private int _tickRate = 30;

        [Header("Client")]
        [SerializeField] private float _connectionTimeout = 10f;

        [Header("Dev")]
        [Tooltip("Force KCP transport for local testing (ignore FizzySteamworks)")]
        [SerializeField] private bool _forceKcpTransport;

        [Header("Spawning")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _gameplayPlayerPrefab;
        [SerializeField] private List<GameObject> _registeredSpawnPrefabs = new();

        public int MaxConnections => _maxConnections;
        public int TickRate => _tickRate;
        public float ConnectionTimeout => _connectionTimeout;
        public bool ForceKcpTransport => _forceKcpTransport;
        public GameObject PlayerPrefab => _playerPrefab;
        public GameObject GameplayPlayerPrefab => _gameplayPlayerPrefab;
        public IReadOnlyList<GameObject> RegisteredSpawnPrefabs => _registeredSpawnPrefabs;
    }
}
