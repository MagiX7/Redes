using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NewUDPManager : MonoBehaviour
{
    [SerializeField] NewServerUDP server;
    [SerializeField] NewClientUDP client;

    [SerializeField] GameObject enemyPrefab;

    [HideInInspector] public List<GameObject> enemies;

    public void SendPlayerData(PlayerData playerData, bool isClient)
    {
        if (isClient) client.SendPlayerData(playerData);
        else server.SendPlayerData(playerData);
    }

    public void SendEnemyData(PlayerData playerData, bool isClient, string ip)
    {
        //if (isClient) 
        //else 
    }

    public void NewEnemy(EndPoint enemyIp)
    {
        string player = enemyIp.ToString();

        GameObject enemy = Instantiate(enemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player = player.Substring(0, player.LastIndexOf(":") - 1);
        enemy.GetComponent<NewEnemyController>().ip = player;
        enemy.GetComponent<NewEnemyController>().udpManager = this;
        enemies.Add(enemy);
    }
    public void NewEnemy(string enemyIp)
    {
        GameObject enemy = Instantiate(enemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        enemyIp = enemyIp.Substring(0, enemyIp.LastIndexOf(":") - 1);
        enemy.GetComponent<NewEnemyController>().ip = enemyIp;
        enemy.GetComponent<NewEnemyController>().udpManager = this;
        enemies.Add(enemy);
    }

    public void UpdateEnemy(PlayerData data, string enemyIp)
    {
        foreach (GameObject enemy in enemies)
        {
            NewEnemyController aux = enemy.GetComponent<NewEnemyController>();
            if (aux.ip == enemyIp)
            {
                aux.playerData = data;
                break;
            }
        }
    }
}
