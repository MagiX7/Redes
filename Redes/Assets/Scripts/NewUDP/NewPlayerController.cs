using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    // Public variables
    public float speed = 1.0f;
    public Camera cam;

    // Animations
    public Animator animator;

    // Online variables
    public PlayerData playerData;
    [SerializeField] NewUDPManager udpManager;
    public bool isClient = false;
    float sendDataCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();
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
        playerData.rotation = transform.rotation;

        sendDataCounter += Time.deltaTime;
        if (sendDataCounter >= 0.05f)
        {
            sendDataCounter = 0.0f;
            udpManager.SendPlayerData(playerData, isClient);
        }
    }
}
