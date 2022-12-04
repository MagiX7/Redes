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

    bool clientConnected = false;
    string lastUserName = string.Empty;

    string text;
    List<EndPoint> remoters;
    int clientsNetId = 1;
    int netId = 0;

    //[SerializeField] Text chat;
    [SerializeField] InputField input;
    //[SerializeField] Text connectedPeople;
    [SerializeField] Text ipText;

    [SerializeField] EnemyController enemy;

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

        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    OnMessageSent();
        //}
        if (messageSent)
        {
            sceneManager.OnNewChatMessage("[Server]: " + input.text);
            //chat.text += ("[Server]: " + input.text + "\n");
            input.text = "";
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
                connectionsManager.OnMessageReceived(bytes, out text);

                if (!remoters.Contains(remote))
                {
                    clientConnected = true;
                    lastUserName = text;
                    remoters.Add(remote);

                    for (int i = 0; i < remoters.Count; i++)
                    {
                        if (remote == remoters[i])
                        {
                            text = "Welcome to the UDP server";
                            sceneManager.OnNewChatMessage(text);
                        }
                        else
                        {
                            text = lastUserName + " Connected!";
                            sceneManager.OnNewChatMessage(text);
                            connectionsManager.OnNewClient(clientsNetId++);
                        }

                        bytes = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, text);
                        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    }
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

        //chat.text += (text + "\n");

        newMessage = false;
        data = new byte[1024];
    }

    //void OnMessageSent()
    //{
    //    string msg = "[Server]: " + input.text;
    //    data = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, msg);
    //    recv = data.Length;
    //    for (int i = 0; i < remoters.Count; i++)
    //    {
    //        serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);
    //    }
    //
    //    messageSent = true;
    //    data = new byte[1024];
    //}

    void OnClientConnected()
    {
        sceneManager.OnNewChatMessage(lastUserName + " Connected!");
        sceneManager.UpdateUsersList(lastUserName);
        //chat.text += (lastUserName + " Connected!\n");
        //connectedPeople.text += (lastUserName + "\n");

        clientConnected = false;
        lastUserName = string.Empty;

        newMessage = false;
        data = new byte[1024];
    }

    //public void SendClientGameStart()
    //{
    //    byte[] bytes = Serializer.SerializeString("Game is about to start!");
    //    serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
    //}

    public void Send(byte[] bytes)
    {
        if (remoters.Count <= 0)
            return;

        //serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
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
