using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    // --- Config ---
    public float speed = 100.0f;
    public LayerMask collisionLayerMask;

    // --- Explosion VFX ---
    public GameObject rocketExplosion;

    // --- Projectile Mesh ---
    public MeshRenderer projectileMesh;

    // --- Script Variables ---
    private bool targetHit;

    // --- Audio ---
    public AudioSource inFlightAudioSource;

    // --- VFX ---
    public ParticleSystem disableOnHit;

    public GameObject instigator;

    private void Start()
    {
    }
    private void Update()
    {
        if (targetHit) return;
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;

        Explode();
        projectileMesh.enabled = false;
        targetHit = true;
        inFlightAudioSource.Stop();
        GetComponent<Collider>().enabled = false;
        disableOnHit.Stop();

        if (collision.gameObject.tag == "Player")
        {
            // Guarantee we are playing as the server
            PlayerMovement serverPlayer = GameObject.Find("0").GetComponent<PlayerMovement>();

            if (serverPlayer != null && !serverPlayer.isClient)
            {
                serverPlayer.playerData.chickenGotHit = true;
                serverPlayer.playerData.chickenHitId = int.Parse(collision.gameObject.name);

                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();

                // As you are playing as the server, you need to process life locally and send the data later
                if (enemy)
                {
                    serverPlayer.playerData.chickenHitLife = enemy.life - 1;
                    enemy.SetLife(serverPlayer.playerData.chickenHitLife);
                }
                else
                {
                    serverPlayer.playerData.chickenHitLife = serverPlayer.life - 1;
                    serverPlayer.SetLife(serverPlayer.playerData.chickenHitLife);
                }
            }
        }

        Destroy(gameObject, 5.0f);
    }

    private void Explode()
    {
        Instantiate(rocketExplosion, transform.position, rocketExplosion.transform.rotation, null);
    }
}
