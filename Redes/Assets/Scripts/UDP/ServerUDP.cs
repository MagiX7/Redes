using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ServerUDP : MonoBehaviour
{
    Socket serverSocket;
    IPEndPoint ipep;
    int recv = 0;
    byte[] data;

    EndPoint remote = null;

    Thread receiveMsgsThread;
    bool finished = false;

    bool newMessage = false;
    bool messageSent = false;

    bool clientConnected = false;
    string lastUserName = string.Empty;

    string text;
    List<EndPoint> remoters;

    [SerializeField] Text chat;
    [SerializeField] InputField input;
    [SerializeField] Text connectedPeople;
    [SerializeField] Text ipText;

    [SerializeField] EnemyController player;
    void Start()
    {
        remoters = new List<EndPoint>();

        data = new byte[1024];

        string serverIp = GetLocalIPAddress();
        ipep = new IPEndPoint(IPAddress.Parse(serverIp), 5345);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.Bind(ipep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        receiveMsgsThread = new Thread(RecieveMessages);
        receiveMsgsThread.Start();

        connectedPeople.text += ("You (Server)\n");
        chat.text += "Server created successfully!\n";
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
        if (!clientConnected && newMessage)
        {
            OnMessageReceived();
        }
        else if (clientConnected)
        {
            OnClientConnected();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnMessageSent();
        }
        if (messageSent)
        {
            chat.text += ("[Server]: " + input.text + "\n");
            input.text = "";
            data = new byte[1024];
            messageSent = false;
        }
    }

    void RecieveMessages()
    {
        while (!finished)
        {
            byte[] bytes = new byte[1024];
            recv = serverSocket.ReceiveFrom(bytes, SocketFlags.None, ref remote);
            if (recv > 0)
            {
                MemoryStream stream = new MemoryStream(bytes, 0, recv);
                BinaryReader reader = new BinaryReader(stream);

                stream.Seek(0, SeekOrigin.Begin);

                int messageType = reader.ReadInt32();
                switch (messageType)
                {
                    case 0:
                    {
                        // New Player Connected
                        text = Encoding.ASCII.GetString(bytes, 0, recv);
                        text = text[2..];
                        break;
                    }

                    case 1:
                    {
                        text = Encoding.ASCII.GetString(bytes, 0, recv);
                        newMessage = true;
                        data = bytes;
                        break;
                    }

                    case 2:
                    {
                        player.playerData = Serializer.DeserializePlayerData(reader, stream);
                        break;
                    }
                     
                    default:
                    {
                        //text = Encoding.ASCII.GetString(bytes, 0, recv);
                        //newMessage = true;
                        //data = bytes;

                        text = Encoding.ASCII.GetString(bytes, 0, recv);
                        text = text[1..];
                        break;
                    }
                }

                if (!remoters.Contains(remote))
                {
                    clientConnected = true;
                    lastUserName = text;
                    remoters.Add(remote);

                    for (int i = 0; i < remoters.Count; i++)
                    {
                        if (remote == remoters[i])
                            text = "Welcome to the UDP server";
                        else
                            text = lastUserName + " Connected!\n";

                        bytes = Encoding.ASCII.GetBytes(text);
                        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoters[i]);
                    }
                }
            }
        }
    }

    void OnMessageReceived()
    {
        data = Encoding.ASCII.GetBytes(text);
        recv = data.Length;

        for (int i = 0; i < remoters.Count; i++)
            serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);

        chat.text += (text + "\n");

        newMessage = false;
        data = new byte[1024];
    }

    void OnMessageSent()
    {
        data = Encoding.ASCII.GetBytes("[Server]: " + input.text);
        recv = data.Length;
        for (int i = 0; i < remoters.Count; i++)
        {
            serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);
        }

        messageSent = true;
        data = new byte[1024];
    }

    void OnClientConnected()
    {
        chat.text += (lastUserName + " Connected!\n");
        connectedPeople.text += (lastUserName + "\n");

        clientConnected = false;
        lastUserName = string.Empty;

        newMessage = false;
        data = new byte[1024];
    }

    public void SendPlayerData(PlayerData playerData)
    {
        if (remoters.Count <= 0)
            return;
        
        byte[] bytes = Serializer.SerializePlayerData(playerData);
        serverSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
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
