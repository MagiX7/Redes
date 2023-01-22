using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;

    ClientSceneManagerUDP sceneManager;
    ConnectionsManager connectionManager;

    public GameObject deathPrefab;
    bool died = false;
    [HideInInspector] public int life = 5;


    // For interpolation
    public float durationInterpolation = 0.05f;
    public float currentInterpolationTime = 0.0f;
    private Vector3 positionToInterpolate;
    private Vector3 interpolateStartingPosition;
    private Quaternion rotationToInterpolate;
 
    // UI Variables
    public HealthBar healthBar;

    // Animations
    private Animator anim;

    void Start()
    {
        connectionManager = GameObject.Find("Connections Manager").GetComponent<ConnectionsManager>();
        sceneManager = GameObject.Find("SceneManager").GetComponent<ClientSceneManagerUDP>();
        playerData = new PlayerData();
        healthBar.SetMaxHealth(5);
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!sceneManager.gameStarted)
        {
            if (transform.position.y <= -3.0f)
            {
                transform.position.Set(transform.position.x, 2.0f, transform.position.z);
            }

            interpolateStartingPosition = positionToInterpolate = playerData.position = gameObject.transform.position;
            return;
        }

        if (playerData.shooted)
        {          
            rocketLauncherController.FireWeapon(playerData.rocketPosition, Quaternion.Euler(playerData.rocketDirection));
            playerData.shooted = false;
        }

        currentInterpolationTime += Time.deltaTime;    
        transform.position = Vector3.Lerp(interpolateStartingPosition, positionToInterpolate, currentInterpolationTime / durationInterpolation);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationToInterpolate, 0.01f * 10.0f);

        if (currentInterpolationTime >= durationInterpolation)
        {
            interpolateStartingPosition = gameObject.transform.position;
            positionToInterpolate = playerData.position;
            rotationToInterpolate = playerData.rotation;

            durationInterpolation = (positionToInterpolate - interpolateStartingPosition).magnitude / 5.0f;
            currentInterpolationTime = 0.0f;
        }

        if (playerData.isMoving)
            anim.SetBool("Run", true);
        else
            anim.SetBool("Run", false);

        if (!died && life <= 0)
        {
            Die();
        }
    }
    public void SetLife(int lifePoints)
    {
        life = lifePoints;
        healthBar.SetHealth(life);
    }

    public void DecrementLife()
    {
        if (!connectionManager.isClient)
            return;

        healthBar.SetHealth(--life);
        playerData.chickenGotHit = false;
        playerData.chickenHitId = -1;
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

    public void ResetStats()
    {
        life = 5;
        healthBar.SetHealth(life);
        died = false;
    }

    //void DisableChicken()
    //{
    //    gameObject.SetActive(false);
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

        transform.position = connectionsManager.positions[int.Parse(name)];

        SetLife(5);
        died = false;
    }
    public PlayerData GetPlayerData() { return playerData; }
}
