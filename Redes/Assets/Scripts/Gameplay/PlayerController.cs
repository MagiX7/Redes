using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;
    //[SerializeField] ClientUDP udp;
    //[SerializeField] ServerUDP udp;

    [SerializeField] UDPManager udpManager;
    public bool isClient = false;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        if(x != 0)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(x * 10,0,0);
        }
        float z = Input.GetAxis("Vertical");
        if (z != 0)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, z * 10);
        }

        playerData.position = transform.position;
        Debug.Log(playerData.position);

        udpManager.SendPlayerData(playerData, isClient);
    }

    public PlayerData GetPlayerData() { return playerData; }

}
