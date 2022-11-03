using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPManager : MonoBehaviour
{
    [SerializeField] ServerUDP server;
    [SerializeField] ClientUDP client;

    public void SendPlayerData(PlayerData playerData, bool isClient)
    {
        if (isClient)
        {
            client.SendPlayerData(playerData);
        }
        else
        {
            server.SendPlayerData(playerData);
        }
    }
}
