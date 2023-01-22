using UnityEngine;

public class RocketLauncherController : MonoBehaviour
{
    // --- Audio ---
    public AudioClip GunShotClip;
    public AudioClip ReloadClip;
    public AudioSource source;
    public AudioSource reloadSource;
    public Vector2 audioPitch = new Vector2(.9f, 1.1f);

    // --- Muzzle ---
    public GameObject muzzlePrefab;
    public GameObject muzzlePosition;

    // --- Config ---
    public bool autoFire;
    public float shotDelay = .5f;
    public bool rotate = true;
    public float rotationSpeed = .25f;

    // --- Options ---
    public GameObject scope;
    public bool scopeActive = true;
    private bool canShoot = true;

    // --- Projectile ---
    public GameObject projectilePrefab;
    public GameObject projectileToDisableOnFire;
    public GameObject instigator;

    // --- Timing ---
    [SerializeField] private float timeLastFired;


    private void Start()
    {
        if (source != null) source.clip = GunShotClip;
        canShoot = true;
    }

    public Transform FireWeapon()
    {
        // --- Spawn muzzle flash ---
        Instantiate(muzzlePrefab, muzzlePosition.transform);

        Transform rocketTransform = null;
        // --- Shoot Projectile Object ---
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, muzzlePosition.transform.position, muzzlePosition.transform.rotation);
            projectile.GetComponent<RocketProjectile>().instigator = instigator;
            Physics.IgnoreCollision(projectile.GetComponent<Collider>(), instigator.GetComponent<Collider>(), true);
            rocketTransform = projectile.transform;
        }

        // --- Disable any gameobjects, if needed ---
        if (projectileToDisableOnFire != null)
        {
            projectileToDisableOnFire.SetActive(false);
            Invoke("ReEnableDisabledProjectile", 3.0f);
        }

        if (source != null)
        {
            source.Play();
        }

        return rocketTransform;
    }

    public Transform FireWeapon(Vector3 pos, Quaternion rotation)
    {
        // --- Spawn muzzle flash ---
        Instantiate(muzzlePrefab, muzzlePosition.transform);

        Transform rocketTransform = null;
        // --- Shoot Projectile Object ---
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, pos, rotation);
            projectile.GetComponent<RocketProjectile>().instigator = instigator;
            Physics.IgnoreCollision(projectile.GetComponent<Collider>(), instigator.GetComponent<Collider>(), true);
            rocketTransform = projectile.transform;
        }

        // --- Disable any gameobjects, if needed ---
        if (projectileToDisableOnFire != null)
        {
            projectileToDisableOnFire.SetActive(false);
            Invoke("ReEnableDisabledProjectile", 3.0f);
        }

        if (source != null)
        {
            source.Play();
        }

        return rocketTransform;
    }

    private void ReEnableDisabledProjectile()
    {
        reloadSource.Play();
        projectileToDisableOnFire.SetActive(true);
        canShoot = true;
    }
}
