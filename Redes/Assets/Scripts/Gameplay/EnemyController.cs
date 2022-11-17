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
    [SerializeField] Text playerHPText;

    Vector3 prevPos;

    void Start()
    {
        playerData = new PlayerData();
        prevPos = playerData.position;
    }

    void Update()
    {
        if (playerData.shooted)
        {
            rocketLauncherController.FireWeapon();
            playerData.shooted = false;
        }

        //if (transform.position != playerData.position)
        //{
        //    transform.position = Vector3.Lerp(prevPos, playerData.position,  Time.deltaTime / 0.2f);
        //}

        transform.Translate(playerData.movementDirection * speed * Time.deltaTime);

        //transform.position = playerData.position;
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

    public void ResetStats()
    {
        life = 5;
        playerHPText.text = life.ToString();
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
