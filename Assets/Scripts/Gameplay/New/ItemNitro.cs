using Fusion;
using UnityEngine;

public class ItemNitro : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var model = other.GetComponent<ModelPlayer>();
        if (model != null && !model.IsDead)
        {
            model.Nitro = 1f;
            Runner.Despawn(Object);
        }
    }
}