using Fusion;
using UnityEngine;

namespace Gameplay.Items
{
    public class ItemLife : NetworkBehaviour, IItemPickup
    {
        [SerializeField] private int healAmount = 50;
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
                model.ModifyLife(healAmount);
                _itemSpawner?.NotifyItemPicked(Object);
                Runner.Despawn(Object);
            }
        }
    }
}