using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class UDPManager : MonoBehaviour
{
    [SerializeField] ServerUDP server;
    [SerializeField] ClientUDP client;

    public void SendPlayerData(PlayerData playerData, int senderNetId, bool isClient)
    {
        byte[] bytes = Serializer.SerializePlayerData(playerData, senderNetId, senderNetId);

        if (isClient) client.Send(bytes);
        else server.Send(bytes);
    }

}
