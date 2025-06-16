using Fusion;
using UnityEngine;

public class ItemAmmunition : NetworkBehaviour, IItemPickup
{
    [SerializeField] private ModelPlayer.SpecialType specialType = ModelPlayer.SpecialType.Fire; // o Stun
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
            model.SetSpecial(specialType);
            _itemSpawner?.NotifyItemPicked(Object);
            Runner.Despawn(Object);
        }
    }
}