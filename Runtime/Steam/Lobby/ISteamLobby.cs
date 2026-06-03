using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SteamCore.Steam
{
    public interface ISteamLobby : IDisposable
    {
        ulong CurrentLobbyId { get; }
        bool IsInLobby { get; }

        UniTask<ulong> CreateLobbyAsync(int lobbyType, int maxMembers, CancellationToken ct = default);
        UniTask<bool> JoinLobbyAsync(ulong lobbyId, CancellationToken ct = default);
        void LeaveLobby();

        void SetLobbyData(string key, string value);
        string GetLobbyData(string key);
        int GetLobbyMemberCount();
        ulong GetLobbyMemberByIndex(int index);
        void OpenInviteOverlay();
    }

    public interface ISteamLobbySearch
    {
        UniTask<List<LobbyInfo>> SearchLobbiesAsync(LobbySearchFilter filter, CancellationToken ct = default);
    }
}
