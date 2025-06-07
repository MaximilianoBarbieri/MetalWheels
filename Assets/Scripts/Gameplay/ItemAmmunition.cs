using Fusion;
using UnityEngine;

public class ItemAmmunition : NetworkBehaviour
{
    [SerializeField] private ModelPlayer.SpecialType specialType = ModelPlayer.SpecialType.Fire; // o Stun

    private void OnTriggerEnter(Collider other)
    {
        var model = other.GetComponent<ModelPlayer>();
        if (model != null && !model.IsDead)
        {
            model.SetSpecial(specialType);
            SpawnManager.Instance?.NotifyItemPicked(Object);
            Runner.Despawn(Object);
        }
    }
}