using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.GameFlow;

namespace SteamCore.Steam
{
    public class InitializeSteamTask : IBootstrapTask
    {
        public string Name => "Initializing Steam";
        public float Weight => 0.15f;

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            progress?.Report(0f);

            var steam = SteamManager.Instance;
            progress?.Report(0.5f);

            if (!steam.IsInitialized)
            {
                progress?.Report(1f);
                return;
            }

            progress?.Report(1f);
            await UniTask.CompletedTask;
        }
    }
}
