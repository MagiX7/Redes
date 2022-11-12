using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;
    Rigidbody rb;
    
    [SerializeField] UDPManager udpManager;
    public bool isClient = false;

    float sendDataCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        if(x != 0)
        {
            rb.velocity = new Vector3(x * 10, rb.velocity.y, rb.velocity.z);
        }
        float z = Input.GetAxis("Vertical");
        if (z != 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, z * 10);
        }

        playerData.position = transform.position;
        //Debug.Log(playerData.position);

        sendDataCounter += Time.deltaTime;
        if (sendDataCounter >= 0.2f)
        {
            sendDataCounter = 0.0f;
            udpManager.SendPlayerData(playerData, isClient);
        }
    }

    public PlayerData GetPlayerData() { return playerData; }

}
