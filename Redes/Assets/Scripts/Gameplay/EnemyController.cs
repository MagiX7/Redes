using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public PlayerData playerData;
    //[SerializeField] ClientUDP udp;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();
    }

    // Update is called once per frame
    void Update()
    {
        //float x = Input.GetAxis("Horizontal");
        //if(x != 0)
        //{
        //    GetComponent<Rigidbody>().velocity = new Vector3(x * 10,0,0);
        //}
        //float z = Input.GetAxis("Vertical");
        //if (z != 0)
        //{
        //    GetComponent<Rigidbody>().velocity = new Vector3(0, 0, z * 10);
        //}

        transform.position = playerData.position;
        transform.rotation = playerData.rotation;

        if (playerData.life <= 0)
        {
            Die();
        }
        //Debug.Log(playerData.position);

        //udp.SendPlayerData(playerData);
    }

    public void Die()
    {
        playerData.life = 0;
        //audioSource.Play();
        //Instantiate(deathPrefab, this.transform.position, Quaternion.identity);
        //Destroy(this.gameObject, 1.0f);
    }

    public PlayerData GetPlayerData() { return playerData; }
}
