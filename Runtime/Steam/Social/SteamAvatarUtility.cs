using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if STEAMWORKS_NET
using Steamworks;
#endif

namespace SteamCore.Steam
{
    public static class SteamAvatarUtility
    {
#if STEAMWORKS_NET
        private static Callback<AvatarImageLoaded_t> _avatarLoadedCallback;
        private static UniTaskCompletionSource<Texture2D> _pendingAvatarTcs;
        private static CSteamID _pendingAvatarSteamId;

        static SteamAvatarUtility()
        {
            _avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }
#endif

        public static Texture2D GetAvatar(ulong steamId)
        {
#if STEAMWORKS_NET
            var id = new CSteamID(steamId);
            int handle = SteamFriends.GetLargeFriendAvatar(id);

            if (handle <= 0)
                return null;

            return ConvertImageToTexture(handle);
#else
            return null;
#endif
        }

        public static async UniTask<Texture2D> GetAvatarAsync(ulong steamId, CancellationToken ct = default)
        {
#if STEAMWORKS_NET
            var id = new CSteamID(steamId);
            int handle = SteamFriends.GetLargeFriendAvatar(id);

            if (handle > 0)
                return ConvertImageToTexture(handle);

            if (handle == -1)
            {
                _pendingAvatarTcs = new UniTaskCompletionSource<Texture2D>();
                _pendingAvatarSteamId = id;

                using (ct.Register(() => _pendingAvatarTcs.TrySetCanceled()))
                {
                    return await _pendingAvatarTcs.Task;
                }
            }

            return null;
#else
            await UniTask.CompletedTask;
            return null;
#endif
        }

#if STEAMWORKS_NET
        private static Texture2D ConvertImageToTexture(int imageHandle)
        {
            if (!SteamUtils.GetImageSize(imageHandle, out uint width, out uint height))
                return null;

            if (width == 0 || height == 0)
                return null;

            int bufferSize = (int)(width * height * 4);
            var buffer = new byte[bufferSize];

            if (!SteamUtils.GetImageRGBA(imageHandle, buffer, bufferSize))
                return null;

            var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            FlipVertical(buffer, (int)width, (int)height);
            texture.LoadRawTextureData(buffer);
            texture.Apply();

            return texture;
        }

        private static void FlipVertical(byte[] buffer, int width, int height)
        {
            int stride = width * 4;
            var rowBuffer = new byte[stride];

            for (int y = 0; y < height / 2; y++)
            {
                int topOffset = y * stride;
                int bottomOffset = (height - 1 - y) * stride;

                Buffer.BlockCopy(buffer, topOffset, rowBuffer, 0, stride);
                Buffer.BlockCopy(buffer, bottomOffset, buffer, topOffset, stride);
                Buffer.BlockCopy(rowBuffer, 0, buffer, bottomOffset, stride);
            }
        }

        private static void OnAvatarImageLoaded(AvatarImageLoaded_t result)
        {
            if (_pendingAvatarTcs == null) return;
            if (result.m_steamID != _pendingAvatarSteamId) return;

            var texture = ConvertImageToTexture(result.m_iImage);
            _pendingAvatarTcs.TrySetResult(texture);
            _pendingAvatarTcs = null;
        }
#endif
    }
}
