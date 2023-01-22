using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Public variables
    public float speed = 1.0f;
    public Camera cam;
    public GameObject weaponPosition;
    public GameObject deathPrefab;
    public ClientSceneManagerUDP sceneManager;

    // Private variables
    bool isMoving = false;
    bool died = false;
    [HideInInspector] public int life = 5;

    // Weapons
    public GameObject rocketLauncher;
    [SerializeField] RocketLauncherController rocketLauncherController;
    bool canShoot = true;

    // Animations
    public Animator animator;

    // Online variables
    public PlayerData playerData = new PlayerData();
    [SerializeField] UDPManager udpManager;
    public bool isClient = false;
    float sendDataCounter = 0;

    // UI Variables
    public HealthBar healthBar;

    void Start()
    {
        healthBar.SetMaxHealth(5);
    }

    void Update()
    {
        if (!sceneManager.gameStarted)
        {
            playerData.position = gameObject.transform.position;
            return;
        }

        float verticalAxis = Input.GetAxis("Vertical");
      
        float horizontalAxis = Input.GetAxis("Horizontal");
        if (verticalAxis != 0.0f || horizontalAxis != 0.0f)
        {
            isMoving = true;
            playerData.isMoving = isMoving;
            animator.SetBool("Run", true);
            transform.Translate(Vector3.forward * verticalAxis * speed * Time.deltaTime, Space.World);
            transform.Translate(Vector3.right * horizontalAxis * speed * Time.deltaTime, Space.World);
        }
        else
        {
            isMoving = false;
            playerData.isMoving = false;
            animator.SetBool("Run", false);
        }


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, 3))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            playerData.movementDirection = direction;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            canShoot = false;
            Invoke("ReEnableDisabledProjectile", 3.0f);
            Transform rocketTrans = rocketLauncherController.FireWeapon();

            playerData.shooted = true;
            playerData.rocketPosition = rocketTrans.position;
            playerData.rocketDirection = rocketTrans.rotation.eulerAngles;
        }

        playerData.position = transform.position;
        playerData.rotation = transform.rotation;
       

        if (!died && life <= 0)
        {
            Die();
        }

        sendDataCounter += Time.deltaTime;
        if (sendDataCounter >= 0.05f) // 4 frames
        {
            playerData.packetID++;
            sendDataCounter = 0.0f;
            udpManager.SendPlayerData(playerData, int.Parse(name), isClient);
            udpManager.SendPlayerData(playerData, int.Parse(name), isClient);
            udpManager.SendPlayerData(playerData, int.Parse(name), isClient);

            playerData.shooted = false;
            playerData.chickenGotHit = false;
            playerData.chickenHitId = -1;
        }
    }

    public void SetLife(int lifePoints)
    {
        life = lifePoints;
        healthBar.SetHealth(life);
    }


    private void OnTriggerEnter(Collider other)
    {
        switch (other.GetComponent<GroundWeapon>().type)
        {
            case GroundWeapon.weaponType.ROCKETLAUNCHER:
            { 
                Destroy(other.gameObject);
                GameObject weapon = Instantiate(rocketLauncher, weaponPosition.transform.position, weaponPosition.transform.rotation);
                weapon.transform.parent = weaponPosition.transform;
                weapon.GetComponentInChildren<RocketLauncherController>().instigator = this.gameObject;
                break;
            }
        }
    }

    public void Die()
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.enabled = false;
        }
        died = true;
        life = 0;
        Invoke("RestartChicken", 5.0f);
        sceneManager.EndGame();
    }

    //public void ResetStats()
    //{
    //    life = 5;
    //    healthBar.SetHealth(life);
    //}

    //void DisableChicken()
    //{
    //    //gameObject.SetActive(false);
    //    Invoke("RestartChicken", 5.0f);
    //}

    void RestartChicken()
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.enabled = true;
        }
        
        ConnectionsManager connectionsManager = GameObject.Find("Connections Manager").GetComponent<ConnectionsManager>();

        if (isClient)
        {
            transform.position = connectionsManager.positions[GetComponentInChildren<ClientUDP>().GetNetId()];
        }
        else
        {
            transform.position = connectionsManager.positions[GetComponentInChildren<ServerUDP>().GetNetId()];
        }
        SetLife(5);
        died = false;
    }

    private void ReEnableDisabledProjectile()
    {
        canShoot = true;
    }
}