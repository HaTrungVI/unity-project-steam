using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Data.Core;
using ProjectBase.GameFlow;
using SteamCore.Steam;

namespace SteamCore.GameFlow
{
    public class InitializeDataTask : IBootstrapTask
    {
        private readonly IDataRegistry _registry;

        public string Name => "Loading save data";
        public float Weight => 0.2f;

        public InitializeDataTask(IDataRegistry registry)
        {
            _registry = registry;
        }

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            progress?.Report(0f);

            IDataSyncAdapter syncAdapter = null;

            if (SteamManager.Instance != null && SteamManager.Instance.IsInitialized)
            {
                var config = SteamManager.Instance.Config;
                if (config != null && config.EnableCloudSave)
                    syncAdapter = new SteamCloudSyncAdapter();
            }

            progress?.Report(0.3f);

            if (_registry != null)
            {
                _registry.Initialize(syncAdapter);
                await _registry.LoadAllAsync(ct);
            }

            progress?.Report(1f);
        }
    }
}
