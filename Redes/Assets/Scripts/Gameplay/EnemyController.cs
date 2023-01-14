using UnityEditor.PackageManager;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;

    public ClientSceneManagerUDP sceneManager;
    public ConnectionsManager connectionManager;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;
    [HideInInspector] public int life = 5;
    float invulnerabilityTime = 1.0f;
    //bool isInvulnerable = false;


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
        playerData = new PlayerData();
        healthBar.SetMaxHealth(5);
        anim = GetComponent<Animator>();
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

        
        if (gotHit && !connectionManager.isClient)
        {
            playerData.isInvulnerable = true;
            life -= 1;
            healthBar.SetHealth(life);
            //gotHit = false;
        }

        if (!died && life <= 0)
        {
            Die();
        }

        if (playerData.isInvulnerable)
        {
            GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0, 184 / 255, 1, 1));
            Invoke("RestoreInvulnerability", invulnerabilityTime);
        }

    }

    public void DecrementLife()
    {
        life -= 1;
        healthBar.SetHealth(life);
        playerData.chickenGotHit = false;
        playerData.chickenHitId = -1;
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

    void RestoreInvulnerability()
    {
        playerData.isInvulnerable = false;
        gotHit = false;
    }

    public PlayerData GetPlayerData() { return playerData; }
}
