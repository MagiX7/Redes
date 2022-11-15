using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;
    [HideInInspector] public bool canShoot = false;

    public ClientSceneManagerUDP sceneManager;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;
    int life = 5;

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
            life -= 5;
            gotHit = false;
        }
        if (!died && life <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        died = true;
        life = 0;
        //audioSource.Play();
        Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject, 1.0f);
        sceneManager.EndGame();
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
