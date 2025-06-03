namespace Network
{
    public class NetworkRunnerHandler
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
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }
    }
}