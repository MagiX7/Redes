using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Public variables
    public float speed = 1.0f;
    public Camera cam;
    public GameObject weaponPosition;
    public GameObject deathPrefab;

    // Private variables
    AudioSource audioSource;

    // Weapons
    public GameObject rocketLauncher;

    // Animations
    public Animator animator;

    // Online variables
    public PlayerData playerData;
    [SerializeField] UDPManager udpManager;
    public bool isClient = false;
    float sendDataCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
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
            Vector3 direction = (hit.point - transform.position);
            transform.rotation = Quaternion.LookRotation(direction);
        }


        playerData.position = transform.position;

        sendDataCounter += Time.deltaTime;
        if (sendDataCounter >= 0.2f)
        {
            sendDataCounter = 0.0f;
            udpManager.SendPlayerData(playerData, isClient);
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
        audioSource.Play();
        Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject, 1.0f);
    }
}