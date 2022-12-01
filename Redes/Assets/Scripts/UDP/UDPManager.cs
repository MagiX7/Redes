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

    public void SendPlayerData(PlayerData playerData, int netId, bool isClient)
    {
        byte[] bytes = Serializer.SerializePlayerData(playerData, netId);

        if (isClient) client.Send(bytes);
        else server.Send(bytes);
    }

    public void SendNewRocketRequest(bool value, bool isClient)
    {
        byte[] bytes = Serializer.SerializeBoolWithHeader(MessageType.SHOOT, value);

        if (isClient) client.Send(bytes);
        else server.Send(bytes);
    }
}
