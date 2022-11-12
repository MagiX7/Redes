using System.Collections;
using System.Collections.Generic;
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

    private void Update()
    {
        // --- Check to see if the target has been hit. We don't want to update the position if the target was hit ---
        if (targetHit) return;

        // --- moves the game object in the forward direction at the defined speed ---
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
            Destroy(collision.gameObject);
            instigator.GetComponent<PlayerMovement>().SetScore();
        }

        Destroy(gameObject, 5.0f);
    }

    private void Explode()
    {
        Instantiate(rocketExplosion, transform.position, rocketExplosion.transform.rotation, null);
    }
}
