using UnityEngine;

    public class GunfireController : MonoBehaviour
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

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                FireWeapon();
                canShoot = false;
            }
        }

        public void FireWeapon()
        {
            // --- Spawn muzzle flash ---
            Instantiate(muzzlePrefab, muzzlePosition.transform);

            // --- Shoot Projectile Object ---
            if (projectilePrefab != null)
            {
              GameObject projectile = Instantiate(projectilePrefab, muzzlePosition.transform.position, muzzlePosition.transform.rotation);
              Physics.IgnoreCollision(projectile.GetComponent<Collider>(), instigator.GetComponent<Collider>(), true);
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

            // TODO cuidao
            //// --- Handle Audio ---
            //if (source != null)
            //{
            //    // --- Sometimes the source is not attached to the weapon for easy instantiation on quick firing weapons like machineguns, 
            //    // so that each shot gets its own audio source, but sometimes it's fine to use just 1 source. We don't want to instantiate 
            //    // the parent gameobject or the program will get stuck in a loop, so we check to see if the source is a child object ---
            //    if(source.transform.IsChildOf(transform))
            //    {
            //        source.Play();
            //    }
            //    else
            //    {
            //        // --- Instantiate prefab for audio, delete after a few seconds ---
            //        AudioSource newAS = Instantiate(source);
            //        if ((newAS = Instantiate(source)) != null && newAS.outputAudioMixerGroup != null && newAS.outputAudioMixerGroup.audioMixer != null)
            //        {
            //            // --- Change pitch to give variation to repeated shots ---
            //            newAS.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", Random.Range(audioPitch.x, audioPitch.y));
            //            newAS.pitch = Random.Range(audioPitch.x, audioPitch.y);

            //            // --- Play the gunshot sound ---
            //            newAS.PlayOneShot(GunShotClip);

            //            // --- Remove after a few seconds. Test script only. When using in project I recommend using an object pool ---
            //            Destroy(newAS.gameObject, 4);
            //        }
            //    }
            //}
        }

        private void ReEnableDisabledProjectile()
        {
            reloadSource.Play();
            projectileToDisableOnFire.SetActive(true);
            canShoot = true;
        }
    }