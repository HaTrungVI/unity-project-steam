#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public class SteamVoiceService : ISteamVoice, System.IDisposable
    {
        private bool _isRecording;

        public bool IsRecording => _isRecording;

        public void StartRecording()
        {
#if STEAMWORKS_NET
            SteamUser.StartVoiceRecording();
            _isRecording = true;
#endif
        }

        public void StopRecording()
        {
#if STEAMWORKS_NET
            SteamUser.StopVoiceRecording();
            _isRecording = false;
#endif
        }

        public bool TryGetVoiceData(byte[] buffer, out uint bytesWritten)
        {
            bytesWritten = 0;
#if STEAMWORKS_NET
            uint compressed;
            var available = SteamUser.GetAvailableVoice(out compressed);
            if (available != EVoiceResult.k_EVoiceResultOK || compressed == 0)
                return false;

            var result = SteamUser.GetVoice(true, buffer, (uint)buffer.Length, out bytesWritten);
            return result == EVoiceResult.k_EVoiceResultOK && bytesWritten > 0;
#else
            return false;
#endif
        }

        public bool TryDecompressVoice(byte[] compressed, uint compressedSize,
            byte[] decompressed, out uint bytesWritten)
        {
            bytesWritten = 0;
#if STEAMWORKS_NET
            var result = SteamUser.DecompressVoice(
                compressed, compressedSize,
                decompressed, (uint)decompressed.Length,
                out bytesWritten, GetOptimalSampleRate());
            return result == EVoiceResult.k_EVoiceResultOK;
#else
            return false;
#endif
        }

        public uint GetOptimalSampleRate()
        {
#if STEAMWORKS_NET
            return SteamUser.GetVoiceOptimalSampleRate();
#else
            return 11025;
#endif
        }

        public void Dispose()
        {
            if (_isRecording)
                StopRecording();
        }
    }
}
