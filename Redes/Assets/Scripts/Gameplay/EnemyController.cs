using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;

    [SerializeField] RocketLauncherController rocketLauncherController;

    public ClientSceneManagerUDP sceneManager;

    public GameObject deathPrefab;
    bool died = false;
    bool gotHit = false;
    [HideInInspector] public int life = 5;

    private float speed = 5.0f;

    // For interpolation
    private int interpolationFramesCount = 5;
    private int elapsedFrames = 0;
    private Vector3 interpolatePosition;
    private Quaternion interpolateRotation;

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

        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        transform.position = Vector3.Lerp(transform.position, interpolatePosition, interpolationRatio);
        transform.rotation = Quaternion.Lerp(transform.rotation, interpolateRotation, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1);
        if (interpolationRatio >= 1.0f)
        {
            interpolatePosition = playerData.position;
            interpolateRotation = playerData.rotation;
        }

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
