using Fusion;
using UnityEngine;

public class SpecialPickUp : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController pc))
        {
            pc.AddSpecial();
            Runner.Despawn(Object);
        }
    }
}