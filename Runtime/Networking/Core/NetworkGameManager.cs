#if MIRROR
using Mirror;
#endif
using ProjectBase.Common.Patterns;
using SteamCore.Steam;
using UnityEngine;

namespace SteamCore.Networking
{
#if MIRROR
    public class NetworkGameManager : NetworkManager, INetworkGameManager
    {
        [Header("Steam Core")]
        [SerializeField] private NetworkConfig _networkConfig;
        [SerializeField] private SteamTransportSetup _transportSetup;

        private readonly NetworkConnectionTracker _connectionTracker = new();
        private ISteamAuth _authService;
        private ISteamLobby _lobbyService;

        public bool IsHost => NetworkServer.active && NetworkClient.isConnected;
        public bool IsClient => NetworkClient.isConnected && !NetworkServer.active;
        public bool IsServer => NetworkServer.active;
        public bool IsOnline => NetworkClient.isConnected;
        public int ConnectedPlayerCount => _connectionTracker.Count;
        public NetworkConnectionTracker ConnectionTracker => _connectionTracker;

        public override void Awake()
        {
            if (transport == null)
                SetupTransport();
            base.Awake();
        }

        private void SetupTransport()
        {
#if FIZZY_STEAMWORKS
            var steamTransport = GetComponent<Mirror.FizzySteam.FizzySteamworks>();
            if (steamTransport == null)
                steamTransport = gameObject.AddComponent<Mirror.FizzySteam.FizzySteamworks>();
            transport = steamTransport;
            if (_transportSetup != null)
                _transportSetup.Configure();
            Debug.Log("[NetworkGameManager] Using FizzySteamworks transport.");
#else
            var kcp = GetComponent<kcp2k.KcpTransport>();
            if (kcp == null)
                kcp = gameObject.AddComponent<kcp2k.KcpTransport>();
            transport = kcp;
            Debug.Log("[NetworkGameManager] FizzySteamworks not available. Using KCP transport.");
#endif
        }

        public void Initialize(ISteamAuth authService, ISteamLobby lobbyService)
        {
            _authService = authService;
            _lobbyService = lobbyService;

            if (_networkConfig != null)
            {
                maxConnections = _networkConfig.MaxConnections;
                sendRate = _networkConfig.TickRate;

                if (_networkConfig.PlayerPrefab != null)
                    playerPrefab = _networkConfig.PlayerPrefab;

                foreach (var prefab in _networkConfig.RegisteredSpawnPrefabs)
                {
                    if (prefab != null && !spawnPrefabs.Contains(prefab))
                        spawnPrefabs.Add(prefab);
                }
            }
        }

        public void StartHosting()
        {
            StartHost();
        }

        public void JoinGame(string hostAddress)
        {
            networkAddress = hostAddress;
            StartClient();
        }

        public void Disconnect()
        {
            if (IsHost)
                StopHost();
            else if (IsClient)
                StopClient();
            else if (IsServer)
                StopServer();

            _lobbyService?.LeaveLobby();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<AuthTicketMessage>(OnAuthTicketReceived);
            NetworkServer.RegisterHandler<ChatMessage>(OnChatMessageReceived);
            NetworkServer.RegisterHandler<VoiceDataMessage>(OnVoiceDataReceived);
            NetworkServer.RegisterHandler<KickPlayerMessage>(OnKickPlayerReceived);
            EventBus.Publish(new ServerStartedEvent());
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            _connectionTracker.Clear();
            EventBus.Publish(new ServerStoppedEvent());
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            _connectionTracker.Add(conn.connectionId, 0, "", conn);
            EventBus.Publish(new ClientJoinedServerEvent
            {
                ConnectionId = conn.connectionId,
                SteamId = 0
            });
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (_connectionTracker.TryGetByConnId(conn.connectionId, out var info))
            {
                if (info.SteamId != 0)
                    _authService?.EndAuthSession(info.SteamId);

                EventBus.Publish(new ClientLeftServerEvent
                {
                    ConnectionId = conn.connectionId,
                    SteamId = info.SteamId,
                    Reason = "Disconnected"
                });
            }
            _connectionTracker.Remove(conn.connectionId);
            base.OnServerDisconnect(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            EventBus.Publish(new LocalClientConnectedEvent());
        }

        public override void OnClientDisconnect()
        {
            EventBus.Publish(new LocalClientDisconnectedEvent { Reason = "Connection lost" });
            base.OnClientDisconnect();
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            EventBus.Publish(new NetworkSceneChangeCompletedEvent { SceneName = sceneName });
        }

        private void OnAuthTicketReceived(NetworkConnectionToClient conn, AuthTicketMessage msg)
        {
            if (_authService == null)
            {
                _connectionTracker.Add(conn.connectionId, msg.SteamId, "", conn);
                return;
            }

            var result = _authService.BeginAuthSession(msg.TicketData, msg.TicketSize, msg.SteamId);
            if (result == 0)
            {
                _connectionTracker.Add(conn.connectionId, msg.SteamId, "", conn);
            }
            else
            {
                Debug.LogWarning($"[NetworkGameManager] Auth failed for SteamId {msg.SteamId}, result={result}");
                conn.Disconnect();
            }
        }

        private void OnChatMessageReceived(NetworkConnectionToClient conn, ChatMessage msg)
        {
            switch (msg.Channel)
            {
                case ChatChannels.Global:
                    NetworkServer.SendToAll(msg);
                    break;

                case ChatChannels.Team:
                    // TODO: implement team filtering when team system exists
                    NetworkServer.SendToAll(msg);
                    break;

                case ChatChannels.Proximity:
                    // TODO: implement proximity filtering when gameplay positions are available
                    NetworkServer.SendToAll(msg);
                    break;

                default:
                    NetworkServer.SendToAll(msg);
                    break;
            }
        }

        private void OnVoiceDataReceived(NetworkConnectionToClient sender, VoiceDataMessage msg)
        {
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn != null && conn.connectionId != sender.connectionId)
                    conn.Send(msg);
            }
        }

        private void OnKickPlayerReceived(NetworkConnectionToClient conn, KickPlayerMessage msg)
        {
            if (conn != NetworkServer.localConnection) return;

            var connections = _connectionTracker.GetAll();
            foreach (var kvp in connections)
            {
                if (kvp.Value.Connection != null && kvp.Value.Connection != conn)
                {
                    kvp.Value.Connection.Send(new ChatMessage
                    {
                        SenderName = "Server",
                        Content = $"Player kicked: {msg.Reason}",
                        Channel = ChatChannels.Global
                    });
                }
            }
        }

        public override void OnDestroy()
        {
            if (_authService is System.IDisposable authDisposable)
                authDisposable.Dispose();
            if (_lobbyService is System.IDisposable lobbyDisposable)
                lobbyDisposable.Dispose();
            base.OnDestroy();
        }
    }
#else
    public class NetworkGameManager : MonoBehaviour, INetworkGameManager
    {
        [SerializeField] private NetworkConfig _networkConfig;

        public bool IsHost => false;
        public bool IsClient => false;
        public bool IsServer => false;
        public bool IsOnline => false;
        public int ConnectedPlayerCount => 0;

        public void Initialize(ISteamAuth authService, ISteamLobby lobbyService) { }

        public void StartHosting()
        {
            Debug.LogWarning("[NetworkGameManager] Mirror not imported. Cannot start hosting.");
        }

        public void JoinGame(string hostAddress)
        {
            Debug.LogWarning("[NetworkGameManager] Mirror not imported. Cannot join game.");
        }

        public void Disconnect() { }
    }
#endif
}
