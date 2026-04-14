using Fusion;
using UnityEngine;

public class BotNetworkSpawnController : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef _botPrefab;
    
    public void SpawnBot(int count)
    {
        if (HasStateAuthority)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = Random.insideUnitSphere * 50;
                Runner.Spawn(_botPrefab, pos, Quaternion.identity);
            }
        }
    }
}
