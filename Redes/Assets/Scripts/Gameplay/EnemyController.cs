using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;
    //[HideInInspector] public bool canShoot = false;

    public ClientSceneManagerUDP sceneManager;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;
    [HideInInspector] public int life = 5;

    private float speed = 5.0f;

    // UI Variables
    public HealthBar healthBar;

    Vector3 prevPos;

    float correctionTimer = 0.0f;

    void Start()
    {
        playerData = new PlayerData();
        prevPos = playerData.position;
        healthBar.SetMaxHealth(5);
    }

    void Update()
    {
        if (playerData.shooted)
        {          
            rocketLauncherController.FireWeapon(playerData.position, Quaternion.Euler(playerData.rocketDirection));
            playerData.shooted = false;
        }

        correctionTimer += Time.deltaTime;
        //if (correctionTimer >= 0.2f)
        {
            transform.position = playerData.position;
            prevPos = transform.position;
            //Debug.Log("TRANSFORM " + transform.position.ToString());
            //Debug.Log("TRANSFORM DATA " + playerData.position.ToString());
        }
        //else
        //{
        //    transform.position = Vector3.Lerp(prevPos, playerData.position, Time.deltaTime / 0.2f);
        //}

        //if (transform.position != playerData.position)
        //{
        //    transform.position = Vector3.Lerp(prevPos, playerData.position, Time.deltaTime / 0.2f);
        //}

        //transform.Translate(playerData.movementDirection * speed * Time.deltaTime);

        //transform.position = playerData.position;
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
        //audioSource.Play();
        //GetComponent<Renderer>().enabled = false;
        Invoke("DisableChicken", 1.0f);
        //Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
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
