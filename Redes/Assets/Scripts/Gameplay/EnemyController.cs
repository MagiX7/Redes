using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;

    ClientSceneManagerUDP sceneManager;
    ConnectionsManager connectionManager;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;
    [HideInInspector] public int life = 5;
    float invulnerabilityTime = 1.0f;
    //bool isInvulnerable = false;
    Material material;


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
        material = transform.GetChild(4).gameObject.GetComponent<SkinnedMeshRenderer>().material;
    }

    void Update()
    {
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
            interpolateStartingPosition = transform.position;
            positionToInterpolate = playerData.position;
            rotationToInterpolate = playerData.rotation;

            durationInterpolation = (positionToInterpolate - interpolateStartingPosition).magnitude / 5.0f;
            currentInterpolationTime = 0.0f;
        }

        if (playerData.isMoving)
            anim.SetBool("Run", true);
        else
            anim.SetBool("Run", false);

        //if (gotHit)
        //{
        //    Debug.Log("Entered Enemy update");
        //    if (!connectionManager.isClient)
        //    {
        //        life -= 1;
        //        healthBar.SetHealth(life);
        //        Debug.Log("Enemy update");
        //    }
        //}

        if (!died && life <= 0)
        {
            Die();
        }
    }
    public void SetLife(int lifePoints)
    {
        Debug.Log("SET LIFE IN ENEMY " + lifePoints);
        life = lifePoints;
        healthBar.SetHealth(life);
        gotHit = true;
    }

    public void DecrementLife()
    {
        Debug.Log("Entered Decrement enemy");
        if (!connectionManager.isClient)
            return;

        healthBar.SetHealth(--life);
        playerData.chickenGotHit = false;
        playerData.chickenHitId = -1;
        gotHit = true;
        Debug.Log("Decrement Enemy");
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

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.tag == "Rocket")
        //{
        //    gotHit = true;
        //}

        // We are in the server
        //if (!connectionManager.isClient)
        //{
        //    if (collision.gameObject.tag == "Rocket")
        //    {
        //        gotHit = true;
        //    }
        //}
    }

    void DisableChicken()
    {
        gameObject.SetActive(false);
        Invoke("RestartChicken", 5.0f);
    }

    void RestartChicken()
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.enabled = true;
        }
        SetLife(5);
        died = false;
    }
    public PlayerData GetPlayerData() { return playerData; }
}
