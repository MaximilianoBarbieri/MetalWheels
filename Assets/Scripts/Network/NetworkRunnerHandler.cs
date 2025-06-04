using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner runnerPrefab;

    public void StartGame(GameMode mode)
    {
        var runner = Instantiate(runnerPrefab);
        runner.name = "NetworkRunner";
        runner.ProvideInput = true;
        runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = "TwistedMetalRoom",
            Scene = null,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
}