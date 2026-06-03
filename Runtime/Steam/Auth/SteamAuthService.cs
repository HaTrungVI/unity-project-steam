using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamAuthService : ISteamAuth
    {
#if STEAMWORKS_NET
        private Callback<GetAuthSessionTicketResponse_t> _authTicketResponseCallback;
        private Callback<ValidateAuthTicketResponse_t> _validateTicketCallback;
        private UniTaskCompletionSource<bool> _ticketTcs;
#endif

        public SteamAuthService()
        {
#if STEAMWORKS_NET
            _authTicketResponseCallback = Callback<GetAuthSessionTicketResponse_t>.Create(OnAuthTicketResponse);
            _validateTicketCallback = Callback<ValidateAuthTicketResponse_t>.Create(OnValidateTicket);
#endif
        }

        public async UniTask<AuthTicketResult> GetAuthSessionTicketAsync(CancellationToken ct = default)
        {
#if STEAMWORKS_NET
            if (!SteamManager.Instance.IsInitialized)
                return AuthTicketResult.Failed;

            var buffer = new byte[1024];
            _ticketTcs = new UniTaskCompletionSource<bool>();

            var identity = new SteamNetworkingIdentity();
            var ticket = SteamUser.GetAuthSessionTicket(buffer, buffer.Length, out uint ticketSize, ref identity);

            using (ct.Register(() => _ticketTcs.TrySetCanceled()))
            {
                var success = await _ticketTcs.Task;
                return new AuthTicketResult(ticket.m_HAuthTicket, buffer, (int)ticketSize, success);
            }
#else
            await UniTask.CompletedTask;
            return AuthTicketResult.Failed;
#endif
        }

        public void CancelAuthTicket(uint ticketHandle)
        {
#if STEAMWORKS_NET
            SteamUser.CancelAuthTicket(new HAuthTicket(ticketHandle));
#endif
        }

        public int BeginAuthSession(byte[] ticketData, int ticketSize, ulong steamId)
        {
#if STEAMWORKS_NET
            return (int)SteamUser.BeginAuthSession(ticketData, ticketSize, new CSteamID(steamId));
#else
            return 0;
#endif
        }

        public void EndAuthSession(ulong steamId)
        {
#if STEAMWORKS_NET
            SteamUser.EndAuthSession(new CSteamID(steamId));
#endif
        }

        public void Dispose()
        {
#if STEAMWORKS_NET
            _authTicketResponseCallback = null;
            _validateTicketCallback = null;
#endif
        }

#if STEAMWORKS_NET
        private void OnAuthTicketResponse(GetAuthSessionTicketResponse_t result)
        {
            _ticketTcs?.TrySetResult(result.m_eResult == EResult.k_EResultOK);
        }

        private void OnValidateTicket(ValidateAuthTicketResponse_t result)
        {
            bool success = result.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseOK;
            EventBus.Publish(new SteamAuthValidatedEvent
            {
                SteamId = result.m_SteamID.m_SteamID,
                Success = success
            });
        }
#endif
    }
}
