using Fusion;
using UnityEngine;

public class ItemLife : NetworkBehaviour
{
    [SerializeField] private int healAmount = 50;

    private void OnTriggerEnter(Collider other)
    {
        var model = other.GetComponent<ModelPlayer>();
        if (model != null && !model.IsDead)
        {
            model.ModifyLife(healAmount);
            Runner.Despawn(Object);
        }
    }
}