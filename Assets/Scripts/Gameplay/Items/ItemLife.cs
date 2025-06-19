using Fusion;
using UnityEngine;

public class ItemLife : NetworkBehaviour, IItemPickup
{
    [SerializeField] private GameObject visual;
    [SerializeField] private int healAmount = 50;

    [Networked] private TickTimer RespawnTimer { get; set; }

    private ItemSpawner _spawner;

    public void SetSpawner(ItemSpawner spawner)
    {
        _spawner = spawner;
    }

    public override void FixedUpdateNetwork()
    {
        if (RespawnTimer.ExpiredOrNotRunning(Runner)) return;

        if (visual != null)
            visual.SetActive(false);

        if (RespawnTimer.Expired(Runner))
        {
            if (_spawner != null)
                Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        if (other.CompareTag("Player"))
        {
            var model = other.GetComponent<ModelPlayer>();
            model.ModifyLife(healAmount);

            RespawnTimer = TickTimer.CreateFromSeconds(Runner, 3f);
            _spawner.NotifyItemPicked(Object);
            Runner.Despawn(Object);
        }
    }
}
