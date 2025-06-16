using Fusion;
using UnityEngine;

public class ItemNitro : NetworkBehaviour, IItemPickup
{
    private ItemSpawner _itemSpawner;

    public void SetSpawner(ItemSpawner spawner)
    {
        _itemSpawner = spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        var model = other.GetComponent<ModelPlayer>();
        if (model != null && !model.IsDead)
        {
            model.Nitro = 1f;
            _itemSpawner?.NotifyItemPicked(Object);
            Runner.Despawn(Object);
        }
    }
}