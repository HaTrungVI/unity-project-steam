namespace SteamCore.Steam
{
    public interface ISteamVoice
    {
        bool IsRecording { get; }
        void StartRecording();
        void StopRecording();
        bool TryGetVoiceData(byte[] buffer, out uint bytesWritten);
        bool TryDecompressVoice(byte[] compressed, uint compressedSize,
            byte[] decompressed, out uint bytesWritten);
        uint GetOptimalSampleRate();
    }
}
