using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;
using UnityEngine;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamLobbyService : ISteamLobby, ISteamLobbySearch
    {
        private readonly SteamConfig _config;
        private ulong _currentLobbyId;

#if STEAMWORKS_NET
        private CallResult<LobbyCreated_t> _createLobbyResult;
        private CallResult<LobbyEnter_t> _joinLobbyResult;
        private CallResult<LobbyMatchList_t> _searchResult;
        private Callback<GameLobbyJoinRequested_t> _joinRequestedCallback;
        private Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
        private Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;

        private UniTaskCompletionSource<ulong> _createTcs;
        private UniTaskCompletionSource<bool> _joinTcs;
        private UniTaskCompletionSource<int> _searchTcs;
#endif

        public ulong CurrentLobbyId => _currentLobbyId;
        public bool IsInLobby => _currentLobbyId != 0;

        public SteamLobbyService(SteamConfig config)
        {
            _config = config;

#if STEAMWORKS_NET
            _createLobbyResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
            _joinLobbyResult = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
            _searchResult = CallResult<LobbyMatchList_t>.Create(OnLobbySearchCompleted);
            _joinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
#endif
        }

        public async UniTask<ulong> CreateLobbyAsync(int lobbyType, int maxMembers, CancellationToken ct = default)
        {
#if STEAMWORKS_NET
            if (!SteamManager.Instance.IsInitialized) return 0;

            _createTcs = new UniTaskCompletionSource<ulong>();
            var call = SteamMatchmaking.CreateLobby((ELobbyType)lobbyType, maxMembers);
            _createLobbyResult.Set(call);

            using (ct.Register(() => _createTcs.TrySetCanceled()))
            {
                var lobbyId = await _createTcs.Task;
                if (lobbyId == 0) return 0;

                _currentLobbyId = lobbyId;
                var csLobby = new CSteamID(lobbyId);

                SteamMatchmaking.SetLobbyData(csLobby, LobbyDataKeys.GameVersion, _config.GameVersion);
                SteamMatchmaking.SetLobbyData(csLobby, LobbyDataKeys.HostAddress,
                    SteamUser.GetSteamID().m_SteamID.ToString());
                SteamMatchmaking.SetLobbyData(csLobby, LobbyDataKeys.HostName,
                    SteamFriends.GetPersonaName());

                EventBus.Publish(new LobbyCreatedEvent { LobbyId = lobbyId });
                return lobbyId;
            }
#else
            await UniTask.CompletedTask;
            return 0;
#endif
        }

        public async UniTask<bool> JoinLobbyAsync(ulong lobbyId, CancellationToken ct = default)
        {
#if STEAMWORKS_NET
            if (!SteamManager.Instance.IsInitialized) return false;

            _joinTcs = new UniTaskCompletionSource<bool>();
            var call = SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
            _joinLobbyResult.Set(call);

            using (ct.Register(() => _joinTcs.TrySetCanceled()))
            {
                var success = await _joinTcs.Task;
                if (success)
                {
                    _currentLobbyId = lobbyId;
                    EventBus.Publish(new LobbyJoinedEvent { LobbyId = lobbyId });
                }
                return success;
            }
#else
            await UniTask.CompletedTask;
            return false;
#endif
        }

        public void LeaveLobby()
        {
#if STEAMWORKS_NET
            if (_currentLobbyId == 0) return;

            SteamMatchmaking.LeaveLobby(new CSteamID(_currentLobbyId));
            var lobbyId = _currentLobbyId;
            _currentLobbyId = 0;
            EventBus.Publish(new LobbyLeftEvent { LobbyId = lobbyId });
#endif
        }

        public void SetLobbyData(string key, string value)
        {
#if STEAMWORKS_NET
            if (_currentLobbyId != 0)
                SteamMatchmaking.SetLobbyData(new CSteamID(_currentLobbyId), key, value);
#endif
        }

        public string GetLobbyData(string key)
        {
#if STEAMWORKS_NET
            if (_currentLobbyId != 0)
                return SteamMatchmaking.GetLobbyData(new CSteamID(_currentLobbyId), key);
#endif
            return string.Empty;
        }

        public int GetLobbyMemberCount()
        {
#if STEAMWORKS_NET
            if (_currentLobbyId != 0)
                return SteamMatchmaking.GetNumLobbyMembers(new CSteamID(_currentLobbyId));
#endif
            return 0;
        }

        public ulong GetLobbyMemberByIndex(int index)
        {
#if STEAMWORKS_NET
            if (_currentLobbyId != 0)
                return SteamMatchmaking.GetLobbyMemberByIndex(
                    new CSteamID(_currentLobbyId), index).m_SteamID;
#endif
            return 0;
        }

        public void OpenInviteOverlay()
        {
#if STEAMWORKS_NET
            if (_currentLobbyId != 0)
                SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(_currentLobbyId));
#endif
        }

        public async UniTask<List<LobbyInfo>> SearchLobbiesAsync(LobbySearchFilter filter,
            CancellationToken ct = default)
        {
            var results = new List<LobbyInfo>();

#if STEAMWORKS_NET
            if (!SteamManager.Instance.IsInitialized) return results;

            SteamMatchmaking.AddRequestLobbyListStringFilter(
                LobbyDataKeys.GameVersion, filter.RequiredVersion,
                ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(filter.MinSlotsAvailable);

            _searchTcs = new UniTaskCompletionSource<int>();
            var call = SteamMatchmaking.RequestLobbyList();
            _searchResult.Set(call);

            using (ct.Register(() => _searchTcs.TrySetCanceled()))
            {
                var count = await _searchTcs.Task;

                for (int i = 0; i < count; i++)
                {
                    var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                    results.Add(new LobbyInfo(
                        lobbyId.m_SteamID,
                        SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.HostName),
                        SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.MapName),
                        SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.GameVersion),
                        SteamMatchmaking.GetNumLobbyMembers(lobbyId),
                        SteamMatchmaking.GetLobbyMemberLimit(lobbyId)
                    ));
                }
            }
#endif
            return results;
        }

        public void Dispose()
        {
            if (IsInLobby) LeaveLobby();
#if STEAMWORKS_NET
            _createLobbyResult = null;
            _joinLobbyResult = null;
            _searchResult = null;
            _joinRequestedCallback = null;
            _lobbyChatUpdateCallback = null;
            _lobbyDataUpdateCallback = null;
#endif
        }

#if STEAMWORKS_NET
        private void OnLobbyCreated(LobbyCreated_t result, bool ioFailure)
        {
            if (ioFailure || result.m_eResult != EResult.k_EResultOK)
            {
                _createTcs?.TrySetResult(0);
                return;
            }
            _createTcs?.TrySetResult(result.m_ulSteamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t result, bool ioFailure)
        {
            _joinTcs?.TrySetResult(!ioFailure && result.m_EChatRoomEnterResponse == 1);
        }

        private void OnJoinRequested(GameLobbyJoinRequested_t result)
        {
            EventBus.Publish(new LobbyJoinRequestedEvent
            {
                LobbyId = result.m_steamIDLobby.m_SteamID,
                FriendId = result.m_steamIDFriend.m_SteamID
            });
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t result)
        {
            var changeType = (EChatMemberStateChange)result.m_rgfChatMemberStateChange;
            EventBus.Publish(new LobbyMemberChangedEvent
            {
                LobbyId = result.m_ulSteamIDLobby,
                MemberId = result.m_ulSteamIDUserChanged,
                ChangeType = changeType switch
                {
                    EChatMemberStateChange.k_EChatMemberStateChangeEntered => LobbyMemberChangeType.Joined,
                    EChatMemberStateChange.k_EChatMemberStateChangeLeft => LobbyMemberChangeType.Left,
                    EChatMemberStateChange.k_EChatMemberStateChangeDisconnected => LobbyMemberChangeType.Disconnected,
                    EChatMemberStateChange.k_EChatMemberStateChangeKicked => LobbyMemberChangeType.Kicked,
                    EChatMemberStateChange.k_EChatMemberStateChangeBanned => LobbyMemberChangeType.Banned,
                    _ => LobbyMemberChangeType.Left
                }
            });
        }

        private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
        {
            EventBus.Publish(new LobbyDataChangedEvent { LobbyId = result.m_ulSteamIDLobby });
        }

        private void OnLobbySearchCompleted(LobbyMatchList_t result, bool ioFailure)
        {
            _searchTcs?.TrySetResult(ioFailure ? 0 : (int)result.m_nLobbiesMatching);
        }
#endif
    }
}
