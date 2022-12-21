using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using static UnityEngine.EventSystems.EventTrigger;

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

    //[SerializeField] Text chat;
    //[SerializeField] InputField input;
    //[SerializeField] Text connectedPeople;
    [SerializeField] Text ipText;

    PlayerData serverPlayerData;

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

        //connectedPeople.text += ("You (Server)\n");
        sceneManager.UpdateUsersList("You (Server)");
        sceneManager.OnNewChatMessage("Server created successfully!");
        //chat.text += "Server created successfully!\n";
        ipText.text += serverIp;

        serverPlayerData = transform.parent.GetComponent<PlayerMovement>().playerData;
        transform.parent.name = netId.ToString();

    }
    
    private void OnDisable()
    {
        finished = true;

        remoters.Clear();

        serverSocket.Close();
        if (receiveMsgsThread.IsAlive)
            receiveMsgsThread.Abort();
    }

    void Update()
    {
        if (needToSendMessage)
        {
            // World State Replication: Passive
            for (int i = 0; i < remoters.Count; i++)
                serverSocket.SendTo(dataToSend, dataToSend.Length, SocketFlags.None, remoters[i]);
            needToSendMessage = false;
            dataToSend = new byte[1024];
        }

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
            text = "Welcome to the UDP server";
            sceneManager.OnNewChatMessage(text);

            // TODO: The problem here is that we send to remote, which is the latest connected client
            // We need a way to ensure both lists, remoters and connectionsManager.players are the same (the first one is greater by 1)
            // So we can send the required information to each client

            foreach (var remote in remoters)
            {
                foreach (var client in connectionsManager.players)
                {
                    // Send the server playerdata
                    //byte[] data = Serializer.SerializePlayerData(serverPlayerData, netId, netId);
                    //serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);
                    // Send the server playerdata

                    int clientNetId = int.Parse(client.name);
                    string message = string.Empty;
                    //if (clientNetId == clientsNetId - 1)
                    //{
                        message = text;
                        data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, clientNetId, message);
                        serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);
                    //}
                    //else
                    //{
                    //    message = ""
                    //}

                    PlayerData playerData = client.GetComponent<EnemyController>().playerData;
                    data = Serializer.SerializePlayerData(playerData, netId, clientNetId);
                    serverSocket.SendTo(data, data.Length, SocketFlags.None, remote);
                }
            }
            notifyExistingUsers = false;
        }

        if (messageSent)
        {
            //sceneManager.OnNewChatMessage("[Server]: " + input.text);
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
                        //byte[] data = Serializer.SerializeIntWithHeader(MessageType.DISCONNECT, clientNetId, -1);
                        //serverSocket.SendTo(data, data.Length, SocketFlags.None, remoters[i]);
                        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    }
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


                    //if (msgType == MessageType.NEW_USER)
                    //{
                    //    for (int i = 0; i < remoters.Count; ++i)
                    //    {
                    //        byte[] data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, netId, "Welcome!");
                    //        serverSocket.SendTo(data, data.Length, SocketFlags.None, remoters[i]);
                    //    }
                    //}


                    // ESTO ES UNA PRUEBA
                    //{
                        notifyExistingUsers = true;
                    //}
                    for (int i = 0; i < remoters.Count - 1; ++i)
                    {
                        text = lastUserName + " Connected!";
                        sceneManager.OnNewChatMessage(text);
                        bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, text);
                        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    }

                    // Lo que hay arriba es más óptimo, crack, titán, portaaviones
                    //for (int i = 0; i < remoters.Count; i++)
                    //{
                    //    if (remote == remoters[i])
                    //    {
                    //        text = "Welcome to the UDP server";
                    //        sceneManager.OnNewChatMessage(text);

                    //        byte[] data = Serializer.SerializePlayerData(serverPlayerData, netId, netId);
                    //        serverSocket.SendTo(data, data.Length, SocketFlags.None, remoters[i]);

                    //        foreach (var client in connectionsManager.players)
                    //        {
                    //            int affectedNetId = client.GetComponent<ClientUDP>().GetNetId();
                    //            //if (affectedNetId == clientsNetId - 1)
                    //            //    continue;
                    //            PlayerData playerData = client.GetComponent<EnemyController>().playerData;
                    //            data = Serializer.SerializePlayerData(playerData, netId, affectedNetId);
                    //            serverSocket.SendTo(data, data.Length, SocketFlags.None, remoters[i]);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        text = lastUserName + " Connected!";
                    //        sceneManager.OnNewChatMessage(text);
                    //        //connectionsManager.OnNewClient(clientsNetId++);
                    //    }

                    //    bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, text);
                    //    serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    //}
                }
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
        sceneManager.OnNewChatMessage(lastUserName + " Connected!");
        sceneManager.UpdateUsersList(lastUserName);

        clientConnected = false;
        lastUserName = string.Empty;

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
