using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SteamCore.Steam
{
    public interface ISteamAuth : IDisposable
    {
        UniTask<AuthTicketResult> GetAuthSessionTicketAsync(CancellationToken ct = default);
        void CancelAuthTicket(uint ticketHandle);
        int BeginAuthSession(byte[] ticketData, int ticketSize, ulong steamId);
        void EndAuthSession(ulong steamId);
    }

    public readonly struct AuthTicketResult
    {
        public readonly uint Handle;
        public readonly byte[] TicketData;
        public readonly int TicketSize;
        public readonly bool Success;

        public AuthTicketResult(uint handle, byte[] ticketData, int ticketSize, bool success)
        {
            Handle = handle;
            TicketData = ticketData;
            TicketSize = ticketSize;
            Success = success;
        }

        public static AuthTicketResult Failed => new(0, null, 0, false);
    }
}
