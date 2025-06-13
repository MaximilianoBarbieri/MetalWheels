using Fusion;
using UnityEngine;
//TODO: codigo aparentemente innecesario
public class LobbyManager : NetworkBehaviour
{
    public GameObject[] carPrefabs;
    public Transform[] spawnPoints;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            int selected = PlayerData.CarSelected;
            var car = Runner.Spawn(carPrefabs[selected], spawnPoints[Object.InputAuthority.RawEncoded % spawnPoints.Length].position, Quaternion.identity, Object.InputAuthority);
        }
    }
}
