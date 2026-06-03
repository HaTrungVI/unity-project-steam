#if MIRROR
using Mirror;
#endif

namespace SteamCore.Networking
{
#if MIRROR
    public struct AuthTicketMessage : NetworkMessage
    {
        public byte[] TicketData;
        public int TicketSize;
        public ulong SteamId;
    }

    public struct ChatMessage : NetworkMessage
    {
        public string SenderName;
        public string Content;
        public int Channel;
    }

    public struct KickPlayerMessage : NetworkMessage
    {
        public string Reason;
    }

    public struct VoiceDataMessage : NetworkMessage
    {
        public ulong SenderSteamId;
        public byte[] CompressedData;
        public uint DataLength;
    }
#endif

    public static class ChatChannels
    {
        public const int Global = 0;
        public const int Team = 1;
        public const int Proximity = 2;
    }
}
