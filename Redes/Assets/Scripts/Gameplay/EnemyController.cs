using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;
    [HideInInspector] public bool canShoot = false;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;

    void Start()
    {
        playerData = new PlayerData();
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

        if (gotHit)
        {
            playerData.life -= 1;
            gotHit = false;
        }
        if (!died && playerData.life <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        died = true;
        playerData.life = 0;
        //audioSource.Play();
        Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject, 1.0f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Rocket")
        {
            gotHit = true;
        }
    }


    public PlayerData GetPlayerData() { return playerData; }
}
