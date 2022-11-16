using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // UI Variables
    [SerializeField] Text playerHPText;

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
            life -= 1;
            playerHPText.text = life.ToString();
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
        //GetComponent<Renderer>().enabled = false;
        Invoke("DisableChicken", 1.0f);
        //Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
        sceneManager.EndGame();
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
