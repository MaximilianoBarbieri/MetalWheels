using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    private NetworkInputData _inputData;

    private bool _isJumpPressed;
    private bool _isNitroPressed;
    private bool _isShootNormalPressed;
    private bool _isShootSpecialPressed;

    void Start()
    {
        _inputData = new NetworkInputData();
    }
    
    void Update()
    {
        _inputData.movementInputHorizontal = Input.GetAxis("Horizontal");
        _inputData.movementInputVertical = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space)) _isJumpPressed = true;
        if (Input.GetKeyDown(KeyCode.Q)) _isShootNormalPressed = true;
        if (Input.GetKeyDown(KeyCode.E)) _isShootNormalPressed = true;
        
        _isNitroPressed |= Input.GetKey(KeyCode.LeftShift);
    }

    public NetworkInputData GetLocalInputs()
    {
        _inputData.isJumpPressed = _isJumpPressed;
        _isJumpPressed = false;
        
        _inputData.isShootNormalPressed = _isShootNormalPressed;
        _isShootNormalPressed = false;
        
        _inputData.isShootSpecialPressed = _isShootSpecialPressed;
        _isShootSpecialPressed = false;
        
        _inputData.networkButtons.Set(MyButtons.NITRO, _isNitroPressed);
        _isNitroPressed = false;
        
        return _inputData;
    }
}