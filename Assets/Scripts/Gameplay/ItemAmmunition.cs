using Fusion;
using UnityEngine;

public class ItemAmmunition : NetworkBehaviour
{
    [SerializeField] private ModelPlayer.SpecialType specialType = ModelPlayer.SpecialType.Fire; // o Stun
    private ItemSpawner _itemSpawner;

    
    private void OnTriggerEnter(Collider other)
    {
        var model = other.GetComponent<ModelPlayer>();
        if (model != null && !model.IsDead)
        {
            model.SetSpecial(specialType);
            //TODO: ver como hacer para que todos los items avisen cuando son pickeados a "itemSpawner"
            //SpawnManager.Instance?.NotifyItemPicked(Object);
            Runner.Despawn(Object);
        }
    }
}