using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Networked] public int Kills { get; set; }
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject specialProjectilePrefab;

    private bool hasSpecial;

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        rb.AddForce(move * 20f);

        if (Input.GetKeyDown(KeyCode.Space))
            rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);

        if (Input.GetKeyDown(KeyCode.O)) Shoot(projectilePrefab);
        if (Input.GetKeyDown(KeyCode.P) && hasSpecial)
        {
            Shoot(specialProjectilePrefab);
            hasSpecial = false;
        }
    }

    private void Shoot(GameObject prefab)
    {
        Runner.Spawn(prefab, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
    }

    public void AddSpecial() => hasSpecial = true;
}
