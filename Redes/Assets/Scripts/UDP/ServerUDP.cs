using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerUDP : MonoBehaviour
{
    Socket serverSocket;
    IPEndPoint ipep;
    int recv = 0;
    byte[] data;
    byte[] dataToSend;
    bool needToSendMessage = false;

    EndPoint remote = null;

    Thread receiveMsgsThread;
    Thread broadcastMsgsThread;
    bool finished = false;

    bool newMessage = false;
    bool messageSent = false;
    bool notifyExistingUsers = false;

    bool clientConnected = false;
    string lastUserName = string.Empty;

    string text;
    List<EndPoint> remoters;
    int clientsNetId = 1;
    int netId = 0;

    [SerializeField] Text ipText;

    ConnectionsManager connectionsManager;
    ClientSceneManagerUDP sceneManager;

    void Start()
    {
        remoters = new List<EndPoint>();

        connectionsManager = GameObject.Find("Connections Manager").GetComponent<ConnectionsManager>();
        sceneManager = GameObject.Find("SceneManager").GetComponent<ClientSceneManagerUDP>();

        data = new byte[1024];
        dataToSend = new byte[1024];

        string serverIp = GetLocalIPAddress();
        ipep = new IPEndPoint(IPAddress.Parse(serverIp), 5345);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.Bind(ipep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        receiveMsgsThread = new Thread(ReceiveMessages);
        receiveMsgsThread.Start();
        broadcastMsgsThread = new Thread(BroadcastMessages);
        broadcastMsgsThread.Start();

        sceneManager.UpdateUsersList("You (Server)");
        sceneManager.OnNewChatMessage("Server created successfully!");
        ipText.text += serverIp;

        transform.parent.name = netId.ToString();
        transform.parent.position = connectionsManager.positions[netId];
    }
    
    private void OnDisable()
    {
        finished = true;

        remoters.Clear();

        serverSocket.Close();
        if (receiveMsgsThread.IsAlive)
            receiveMsgsThread.Abort();
        if (broadcastMsgsThread.IsAlive)
            broadcastMsgsThread.Abort();
    }

    void Update()
    {
        if (!clientConnected && newMessage)
        {
            OnChatMessageReceived();
        }
        else if (clientConnected)
        {
            OnClientConnected();
        }

        if (notifyExistingUsers && !connectionsManager.newUser)
        {
            foreach (var remote in remoters)
            {
                for (int i = 0; i < connectionsManager.players.Count; i++)
                {
                    GameObject client = connectionsManager.players[i];
                    int clientNetId = int.Parse(client.name);
                    string text = "Welcome, " + lastUserName;
                    data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, clientNetId, text);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);

                    PlayerData playerData = client.GetComponent<EnemyController>().playerData;
                    data = Serializer.SerializePlayerData(playerData, netId, clientNetId);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);
                }
            }
            notifyExistingUsers = false;
        }

        if (messageSent)
        {
            data = new byte[1024];
            messageSent = false;
        }
    }

    void ReceiveMessages()
    {
        while (!finished)
        {
            byte[] bytes = new byte[1024];
            recv = serverSocket.ReceiveFrom(bytes, SocketFlags.None, ref remote);
            if (recv > 0)
            {
                int clientNetId = -1;
                int senderNetId;
                MessageType msgType = connectionsManager.OnMessageReceived(bytes, out text, out clientNetId, out senderNetId, out _);

                if (msgType == MessageType.DISCONNECT)
                {
                    remoters.Remove(remote);
                    for (int i = 0; i < remoters.Count; ++i)
                    {
                        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    }
                    continue;
                }

                if (msgType == MessageType.NEW_USER)
                {
                    clientConnected = true;
                    lastUserName = text;
                }
                else if (msgType == MessageType.PLAYER_DATA && connectionsManager.needToUpdateEnemy)
                {
                    dataToSend = bytes;
                    needToSendMessage = true;
                }
                else if(msgType == MessageType.RTT)
                {
                    byte[] rttData = Serializer.SerializeDateWithHeader(MessageType.RTT, DateTime.Now);
                    // We use Send here and not socket.Send() to simulate the real process of sending the packets the app follows
                    serverSocket.SendTo(rttData, remote);
                    //Send(rttData);
                    continue;
                }



                if (!remoters.Contains(remote))
                {
                    clientConnected = true;
                    lastUserName = text;
                    remoters.Add(remote);

                    byte[] netIdBytes = Serializer.SerializeIntWithHeader(MessageType.NET_ID, netId, clientsNetId);
                    serverSocket.SendTo(netIdBytes, remote);
                    connectionsManager.OnNewClient(clientsNetId);
                    clientsNetId++;

                    notifyExistingUsers = true;

                    for (int i = 0; i < remoters.Count - 1; ++i)
                    {
                        string msg = "Welcome, " + lastUserName;
                        sceneManager.OnNewChatMessage(msg);
                        bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, msg);
                        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    }
                }
            }
        }
    }

    void BroadcastMessages()
    {
        while (!finished)
        {
            if (needToSendMessage)
            {
                // World State Replication: Passive
                for (int i = 0; i < remoters.Count; i++)
                    serverSocket.SendTo(dataToSend, dataToSend.Length, SocketFlags.None, remoters[i]);
                needToSendMessage = false;
                dataToSend = new byte[1024];
            }
        }
    }

    void OnChatMessageReceived()
    {
        data = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, text);
        recv = data.Length;

        for (int i = 0; i < remoters.Count; i++)
            serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);

        newMessage = false;
        data = new byte[1024];
    }

    void OnClientConnected()
    {
        sceneManager.OnNewChatMessage("Welcome, " + lastUserName);
        sceneManager.UpdateUsersList(lastUserName);

        clientConnected = false;
        //lastUserName = string.Empty;

        newMessage = false;
        data = new byte[1024];
    }

    public void Send(byte[] bytes)
    {
        if (remoters.Count <= 0)
            return;

        dataToSend = bytes;
        needToSendMessage = true;
    }

    public int GetNetId()
    {
        return netId;
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "Null";
    }

}
