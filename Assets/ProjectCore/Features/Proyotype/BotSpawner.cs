using Fusion;
using UnityEngine;

public class BotSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _botPrefab;
    public void SpawnBot()
    {
        if (HasStateAuthority)
        {
            for (int i = 0; i < 1000; i++)
            {
                var pos = Random.insideUnitSphere * 30;
                Runner.Spawn(_botPrefab, pos, Quaternion.identity);
            }
        }
    }
}
