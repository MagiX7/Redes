using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnemyController : MonoBehaviour
{
    [HideInInspector] public PlayerData playerData;
    [HideInInspector] public string ip;
    [HideInInspector] public NewUDPManager udpManager;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = playerData.position;
        transform.rotation = playerData.rotation;
    }
}
