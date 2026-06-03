using UnityEngine;

namespace SteamCore.Networking
{
    public class SteamTransportSetup : MonoBehaviour
    {
#if MIRROR && FIZZY_STEAMWORKS
        [SerializeField] private Mirror.FizzySteam.FizzySteamworks _transport;

        [Header("Settings")]
        [SerializeField] private int _connectionTimeout = 25;
        [SerializeField] private bool _allowSteamRelay = true;
        [SerializeField] private bool _useNextGenNetworking = true;

        public void Configure()
        {
            if (_transport == null)
            {
                Debug.LogWarning("[SteamTransportSetup] FizzySteamworks transport not assigned.");
                return;
            }

            _transport.Timeout = _connectionTimeout;
            _transport.AllowSteamRelay = _allowSteamRelay;
            _transport.UseNextGenSteamNetworking = _useNextGenNetworking;

            Debug.Log($"[SteamTransportSetup] Steam transport configured. " +
                      $"NextGen={_useNextGenNetworking}, Relay={_allowSteamRelay}, Timeout={_connectionTimeout}s");
        }
#else
        public void Configure()
        {
            Debug.Log("[SteamTransportSetup] Steam transport not available. Using default transport.");
        }
#endif
    }
}
