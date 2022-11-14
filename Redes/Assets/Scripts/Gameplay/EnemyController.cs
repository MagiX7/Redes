using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public PlayerData playerData;

    //public GameObject rocketLauncher;
    [SerializeField] RocketLauncherController rocketLauncherController;
    [HideInInspector] public bool canShoot = false;


    void Start()
    {
        playerData = new PlayerData();
        //rocketLauncherController = rocketLauncher.GetComponent<RocketLauncherController>();
    }

    void Update()
    {
        if (canShoot)
        {
            rocketLauncherController.FireWeapon();
            canShoot = false;
        }

        transform.position = playerData.position;
        transform.rotation = playerData.rotation;

        if (playerData.life <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        playerData.life = 0;
        //audioSource.Play();
        //Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
        //Destroy(this.gameObject, 1.0f);
    }

    public PlayerData GetPlayerData() { return playerData; }
}
