using Fusion;

public struct NetworkInputData : INetworkInput
{
    public float movementInputVertical;
    public float movementInputHorizontal;

    public NetworkBool isShootNormalPressed;
    public NetworkBool isShootSpecialPressed;
    public NetworkBool isJumpPressed;
    public NetworkBool isNitroPressed;

    public NetworkButtons networkButtons;
}

enum MyButtons
{
    NITRO = 0,
}