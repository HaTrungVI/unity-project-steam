#if MIRROR
using Mirror;
using ProjectBase.Common.Patterns;
using SteamCore.Steam;

namespace SteamCore.Networking
{
    public class NetworkPlayerController : NetworkBehaviour, INetworkPlayer
    {
        [SyncVar(hook = nameof(OnSteamIdChanged))]
        private ulong _steamId;

        [SyncVar(hook = nameof(OnDisplayNameChanged))]
        private string _displayName = "";

        [SyncVar(hook = nameof(OnReadyChanged))]
        private bool _isReady;

        public uint NetId => netId;
        public new bool IsLocalPlayer => isLocalPlayer;
        public new bool IsServer => isServer;
        public ulong SteamId => _steamId;
        public string DisplayName => _displayName;
        public bool IsReady => _isReady;

        public override void OnStartLocalPlayer()
        {
            var steam = SteamManager.Instance;
            CmdSetPlayerInfo(steam.LocalSteamId, steam.LocalDisplayName);
            EventBus.Publish(new LocalPlayerSpawnedEvent { Player = this });
        }

        public override void OnStartClient()
        {
            NetworkPlayerRegistry.Add(this);
            EventBus.Publish(new PlayerJoinedEvent { Player = this });
        }

        public override void OnStopClient()
        {
            EventBus.Publish(new PlayerLeftEvent { Player = this });
            NetworkPlayerRegistry.Remove(this);
        }

        [Command]
        private void CmdSetPlayerInfo(ulong steamId, string displayName)
        {
            var manager = NetworkManager.singleton as NetworkGameManager;
            if (manager != null)
            {
                var tracker = manager.ConnectionTracker;
                if (tracker.TryGetByConnId(connectionToClient.connectionId, out var info))
                {
                    if (info.SteamId != 0 && info.SteamId != steamId)
                    {
                        connectionToClient.Disconnect();
                        return;
                    }
                }
            }

            _steamId = steamId;
            _displayName = displayName;
        }

        [Command]
        public void CmdSetReady(bool ready)
        {
            _isReady = ready;
            CheckAllPlayersReady();
        }

        [Server]
        private void CheckAllPlayersReady()
        {
            var players = new System.Collections.Generic.List<NetworkPlayerController>();
            NetworkPlayerRegistry.GetAll(players);

            if (players.Count < 2) return;

            foreach (var p in players)
            {
                if (!p.IsReady) return;
            }

            EventBus.Publish(new AllPlayersReadyEvent());
        }

        private void OnSteamIdChanged(ulong oldVal, ulong newVal)
        {
            EventBus.Publish(new PlayerInfoChangedEvent { Player = this });
        }

        private void OnDisplayNameChanged(string oldVal, string newVal)
        {
            EventBus.Publish(new PlayerInfoChangedEvent { Player = this });
        }

        private void OnReadyChanged(bool oldVal, bool newVal)
        {
            EventBus.Publish(new PlayerReadyChangedEvent { Player = this, IsReady = newVal });
        }
    }
}
#endif
