using Fusion;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked] public string Nickname { get; set; }
}
