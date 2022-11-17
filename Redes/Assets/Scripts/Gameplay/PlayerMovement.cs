using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Public variables
    public float speed = 1.0f;
    public Camera cam;
    public GameObject weaponPosition;
    public GameObject deathPrefab;
    public ClientSceneManagerUDP sceneManager;

    // Private variables
    AudioSource audioSource;
    bool died = false;
    bool gotHit = false;
    [HideInInspector] public int life = 5;

    // Weapons
    public GameObject rocketLauncher;
    [SerializeField] RocketLauncherController rocketLauncherController;
    bool canShoot = true;

    // Animations
    public Animator animator;

    // Online variables
    public PlayerData playerData;
    [SerializeField] UDPManager udpManager;
    public bool isClient = false;
    float sendDataCounter = 0;

    // UI Variables
    [SerializeField] Text playerHPText;

    void Start()
    {
        playerData = new PlayerData();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");
        if (verticalAxis != 0.0f || horizontalAxis != 0.0f)
        {
            animator.SetBool("Run", true);
            transform.Translate(Vector3.forward * verticalAxis * speed * Time.deltaTime, Space.World);
            transform.Translate(Vector3.right * horizontalAxis * speed * Time.deltaTime, Space.World);
        }
        else
        {
            animator.SetBool("Run", false);
        }


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            playerData.movementDirection = direction;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            canShoot = false;
            playerData.shooted = true;
            Invoke("ReEnableDisabledProjectile", 3.0f);
            rocketLauncherController.FireWeapon();
            //udpManager.SendNewRocketRequest(true, isClient);
        }


        playerData.position = transform.position;
        playerData.rotation = transform.rotation;

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

        sendDataCounter += Time.deltaTime;
        if (sendDataCounter >= 0.2f)
        {
            sendDataCounter = 0.0f;
            udpManager.SendPlayerData(playerData, isClient);
            playerData.shooted = false;
        }
    }

    public PlayerData GetPlayerData() { return playerData; }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.GetComponent<GroundWeapon>().type)
        {
            case GroundWeapon.weaponType.ROCKETLAUNCHER:
                Destroy(other.gameObject);
                GameObject weapon = Instantiate(rocketLauncher, weaponPosition.transform.position, weaponPosition.transform.rotation);
                weapon.transform.parent = weaponPosition.transform;
                weapon.GetComponentInChildren<RocketLauncherController>().instigator = this.gameObject;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Rocket")
        {
            gotHit = true;
        }
    }

    public void SetScore()
    {
        Time.timeScale = 0.2f;
        Invoke("ResetTimeScale", 0.3f);
    }

    void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
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

    void DisableChicken()
    {
        gameObject.SetActive(false);
    }

    private void ReEnableDisabledProjectile()
    {
        canShoot = true;
    }
}