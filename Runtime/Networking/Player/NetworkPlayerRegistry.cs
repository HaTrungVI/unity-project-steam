using System.Collections.Generic;

namespace SteamCore.Networking
{
#if MIRROR
    public static class NetworkPlayerRegistry
    {
        private static readonly Dictionary<uint, NetworkPlayerController> _players = new();

        public static int Count => _players.Count;
        public static NetworkPlayerController LocalPlayer { get; private set; }

        public static void Add(NetworkPlayerController player)
        {
            _players[player.NetId] = player;
            if (player.IsLocalPlayer)
                LocalPlayer = player;
        }

        public static void Remove(NetworkPlayerController player)
        {
            _players.Remove(player.NetId);
            if (LocalPlayer == player)
                LocalPlayer = null;
        }

        public static bool TryGet(uint netId, out NetworkPlayerController player)
        {
            return _players.TryGetValue(netId, out player);
        }

        public static void GetAll(List<NetworkPlayerController> result)
        {
            result.Clear();
            result.AddRange(_players.Values);
        }

        public static void Clear()
        {
            _players.Clear();
            LocalPlayer = null;
        }
    }
#else
    public static class NetworkPlayerRegistry
    {
        public static int Count => 0;
        public static void Clear() { }
    }
#endif
}
