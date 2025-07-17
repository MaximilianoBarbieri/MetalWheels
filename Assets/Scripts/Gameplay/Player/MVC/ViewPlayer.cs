using UnityEngine;

public class ViewPlayer : MonoBehaviour
{
    [SerializeField] private Renderer carRenderer;
    [SerializeField] private GameObject stunFX;
    [SerializeField] private GameObject damageFX;
    [SerializeField] private GameObject fireFX;

    public void PlayStunFX()
    {
        if (stunFX) stunFX.SetActive(true);
    }
    
    public void PlayDamageFX()
    {
        if (damageFX) damageFX.SetActive(true);
    }
    
    public void PlayFireFX()
    {
        if (fireFX) fireFX.SetActive(true);
    }

    public void SetCarMaterial(Material mat)
    {
        if (carRenderer) carRenderer.material = mat;
    }
}