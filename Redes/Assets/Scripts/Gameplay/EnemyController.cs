using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;

    public ClientSceneManagerUDP sceneManager;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;
    [HideInInspector] public int life = 5;

    // UI Variables
    public HealthBar healthBar;

    void Start()
    {
        playerData = new PlayerData();
        healthBar.SetMaxHealth(5);
    }

    void Update()
    {
        if (playerData.shooted)
        {          
            rocketLauncherController.FireWeapon(playerData.rocketPosition, Quaternion.Euler(playerData.rocketDirection));
            playerData.shooted = false;
        }

        transform.position = playerData.position;
        transform.rotation = playerData.rotation;

        if (gotHit)
        {
            life -= 1;
            healthBar.SetHealth(life);
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
        Invoke("DisableChicken", 1.0f);
        sceneManager.EndGame();
    }

    public void ResetStats()
    {
        life = 5;
        healthBar.SetHealth(life);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Rocket")
        {
            gotHit = true;
        }
    }

    void DisableChicken()
    {
        gameObject.SetActive(false);
    }

    public PlayerData GetPlayerData() { return playerData; }
}
