using UnityEngine;

public class ItemFloatingVisual : MonoBehaviour
{
    [Header("Ajustes de animación")]
    public float amplitude = 0.5f;    
    public float frequency = 0.5f;    
    public Vector3 axis = Vector3.up; 
    
    private Vector3 _startLocalPos;

    void Start()
    {
        _startLocalPos = transform.localPosition;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency * Mathf.PI * 2) * amplitude;
        transform.localPosition = _startLocalPos + axis * yOffset;
    }
}

