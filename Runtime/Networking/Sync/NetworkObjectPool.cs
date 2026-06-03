using System.Collections.Generic;
using UnityEngine;

#if MIRROR
using Mirror;
#endif

namespace SteamCore.Networking
{
    [System.Serializable]
    public struct NetworkPoolEntry
    {
        public GameObject Prefab;
        public int PrewarmCount;
    }

    public class NetworkObjectPool : MonoBehaviour
    {
        [SerializeField] private List<NetworkPoolEntry> _poolEntries = new();

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        public void RegisterPrefabsWithMirror()
        {
#if MIRROR
            foreach (var entry in _poolEntries)
            {
                if (entry.Prefab == null) continue;

                var prefab = entry.Prefab;
                _pools[prefab] = new Queue<GameObject>();

                for (int i = 0; i < entry.PrewarmCount; i++)
                {
                    var instance = Instantiate(prefab, transform);
                    instance.SetActive(false);
                    _pools[prefab].Enqueue(instance);
                }

                NetworkClient.RegisterPrefab(prefab, SpawnHandler, UnspawnHandler);
            }
#endif
        }

#if MIRROR
        public GameObject ServerSpawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var instance = GetFromPool(prefab, position, rotation);
            NetworkServer.Spawn(instance);
            return instance;
        }

        public void ServerDespawn(GameObject instance)
        {
            NetworkServer.UnSpawn(instance);
            ReturnToPool(instance);
        }
#endif

        private GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }

            GameObject instance;
            if (queue.Count > 0)
            {
                instance = queue.Dequeue();
                instance.transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                instance = Instantiate(prefab, position, rotation);
            }

            instance.SetActive(true);
            _instanceToPrefab[instance] = prefab;
            return instance;
        }

        private void ReturnToPool(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(transform);

            if (_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                if (!_pools.TryGetValue(prefab, out var queue))
                {
                    queue = new Queue<GameObject>();
                    _pools[prefab] = queue;
                }
                queue.Enqueue(instance);
            }
        }

#if MIRROR
        private GameObject SpawnHandler(SpawnMessage msg)
        {
            foreach (var entry in _poolEntries)
            {
                if (entry.Prefab != null && entry.Prefab.GetComponent<NetworkIdentity>().assetId == msg.assetId)
                    return GetFromPool(entry.Prefab, msg.position, msg.rotation);
            }
            return null;
        }

        private void UnspawnHandler(GameObject instance)
        {
            ReturnToPool(instance);
        }
#endif

        private void OnDestroy()
        {
            foreach (var kvp in _pools)
            {
                while (kvp.Value.Count > 0)
                {
                    var instance = kvp.Value.Dequeue();
                    if (instance != null)
                        Destroy(instance);
                }
            }
            _pools.Clear();
            _instanceToPrefab.Clear();
        }
    }
}
