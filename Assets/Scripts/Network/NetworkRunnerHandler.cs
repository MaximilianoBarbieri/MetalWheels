using Fusion;
using UnityEngine;

public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField] public NetworkRunner runnerPrefab;
    private static NetworkRunner currentRunner;

    public void StartGame(GameMode mode)
    {
        if (currentRunner != null)
            return;

        var runner = Instantiate(runnerPrefab);
        runner.name = "NetworkRunner";
        runner.ProvideInput = true;
        DontDestroyOnLoad(runner.gameObject); // <-- esto es clave

        runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = "TwistedMetalRoom",
            Scene = null,
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        currentRunner = runner;
    }
}