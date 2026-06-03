using System.Collections.Generic;

#if MIRROR
using Mirror;
#endif

namespace SteamCore.Networking
{
    public readonly struct PlayerConnectionInfo
    {
        public readonly int ConnectionId;
        public readonly ulong SteamId;
        public readonly string DisplayName;
#if MIRROR
        public readonly NetworkConnectionToClient Connection;

        public PlayerConnectionInfo(int connectionId, ulong steamId, string displayName,
            NetworkConnectionToClient connection)
        {
            ConnectionId = connectionId;
            SteamId = steamId;
            DisplayName = displayName;
            Connection = connection;
        }
#else
        public PlayerConnectionInfo(int connectionId, ulong steamId, string displayName)
        {
            ConnectionId = connectionId;
            SteamId = steamId;
            DisplayName = displayName;
        }
#endif
    }

    public class NetworkConnectionTracker
    {
        private readonly Dictionary<int, PlayerConnectionInfo> _connections = new();
        private readonly Dictionary<ulong, int> _steamIdToConnId = new();

        public int Count => _connections.Count;

#if MIRROR
        public void Add(int connId, ulong steamId, string displayName, NetworkConnectionToClient conn)
        {
            var info = new PlayerConnectionInfo(connId, steamId, displayName, conn);
            _connections[connId] = info;
            if (steamId != 0)
                _steamIdToConnId[steamId] = connId;
        }
#endif

        public void Remove(int connId)
        {
            if (_connections.TryGetValue(connId, out var info))
            {
                if (info.SteamId != 0)
                    _steamIdToConnId.Remove(info.SteamId);
                _connections.Remove(connId);
            }
        }

        public bool TryGetBySteamId(ulong steamId, out PlayerConnectionInfo info)
        {
            if (_steamIdToConnId.TryGetValue(steamId, out int connId))
                return _connections.TryGetValue(connId, out info);
            info = default;
            return false;
        }

        public bool TryGetByConnId(int connId, out PlayerConnectionInfo info)
        {
            return _connections.TryGetValue(connId, out info);
        }

        public IReadOnlyDictionary<int, PlayerConnectionInfo> GetAll() => _connections;

        public void Clear()
        {
            _connections.Clear();
            _steamIdToConnId.Clear();
        }
    }
}
