using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.XR;

public class ClientUDP : MonoBehaviour
{
    Socket clientSocket;
    IPEndPoint clientIpep;

    int recv = 0;
    byte[] data;
    public string clientIp;
    public string serverIp;
    [HideInInspector] public string userName;
    
    EndPoint remote = null;

    Thread receiveMsgsThread;

    bool finished = false;
    bool newMessage = false;

    string incomingText;

    [SerializeField] UnityEngine.UI.Text chat;
    [SerializeField] InputField input;

    [SerializeField] EnemyController enemy;

    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        clientSocket.Bind(clientIpep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        data = new byte[1024];
        data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, userName);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, remote);
        data = new byte[1024];

        receiveMsgsThread = new Thread(RecieveMessages);
        receiveMsgsThread.Start();

    }

    private void OnDisable()
    {
        finished = true;

        clientSocket.Close();
        if (receiveMsgsThread.IsAlive)
            receiveMsgsThread.Abort();
    }

    void Update()
    {
        if (newMessage)
        {
            chat.text += (incomingText + "\n");
            newMessage = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnMessageSent();
        }
    }

    void RecieveMessages()
    {
        while (!finished)
        {
            byte[] msg = new byte[1024];
            recv = clientSocket.ReceiveFrom(msg, SocketFlags.None, ref remote);

            MemoryStream stream = new MemoryStream(msg, 0, recv);
            BinaryReader reader = new BinaryReader(stream);

            stream.Seek(0, SeekOrigin.Begin);

            MessageType messageType = (MessageType)reader.ReadInt32();
            switch (messageType)
            {
                case MessageType.NEW_USER:
                {
                    // New Player Connected
                    incomingText = Serializer.DeserializeString(reader, stream);
                    break;
                }
                case MessageType.CHAT:
                    //incomingText = Encoding.ASCII.GetString(msg, 0, recv);
                    incomingText = Serializer.DeserializeString(reader, stream);
                    newMessage = true;
                    data = msg;
                    break;
                case MessageType.PLAYER_DATA:
                     enemy.playerData = Serializer.DeserializePlayerData(reader, stream);
                    break;

                default:
                    break;
            }
        }
    }

    void OnMessageSent()
    {
        string msg = "[" + userName + "]" + ": " + input.text;
        //data = Encoding.ASCII.GetBytes(msg);
        data = Serializer.SerializeStringWithHeader(MessageType.CHAT, msg);
        recv = data.Length;
        clientSocket.SendTo(data, recv, SocketFlags.None, remote);
        input.text = "";
    }

    public void SendPlayerData(PlayerData playerData)
    {
        byte[] bytes = Serializer.SerializePlayerData(playerData);
        clientSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
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
