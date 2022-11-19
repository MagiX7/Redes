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
    byte[] dataToSend;
    [HideInInspector] public string clientIp;
    [HideInInspector] public string serverIp;
    [HideInInspector] public string userName;
    
    EndPoint remote = null;

    Thread receiveMsgsThread;

    bool finished = false;
    bool newMessage = false;
    bool sendMessage = false;
    bool sendData = false;

    string incomingText;

    [SerializeField] UnityEngine.UI.Text chat;
    [SerializeField] InputField input;

    [SerializeField] EnemyController enemy;
    [SerializeField] ClientSceneManagerUDP sceneManager;

    
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

        receiveMsgsThread = new Thread(RecieveAndSendMessages);
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
            sendMessage = true;
            OnMessageSent();
        }
    }

    void RecieveAndSendMessages()
    {
        while (!finished)
        {
            // Receive
            byte[] msg = new byte[1024];
            recv = clientSocket.ReceiveFrom(msg, SocketFlags.None, ref remote);

            MemoryStream stream = new MemoryStream(msg, 0, recv);
            BinaryReader reader = new BinaryReader(stream);

            stream.Seek(0, SeekOrigin.Begin);

            MessageType messageType = (MessageType)reader.ReadInt32();
            MsgType(msg, stream, reader, messageType);

            // Send
            if (sendMessage)
            {
                OnMessageSent();
                sendMessage = false;
            }

            if (sendData)
            {
                clientSocket.SendTo(dataToSend, dataToSend.Length, SocketFlags.None, remote);
                sendData = false;
                dataToSend = new byte[1024];
            }
        }

       
    }

    void MsgType(byte[] msg, MemoryStream stream, BinaryReader reader, MessageType messageType)
    {
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

            case MessageType.START_GAME:
                sceneManager.StartClient();
                break;

            //case MessageType.SHOOT:
            //{
            //    enemy.canShoot = Serializer.DeserializeBool(reader, stream);
            //    break;
            //}

            default:
                break;
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

    public void Send(byte[] bytes)
    {
        sendData = true;
        dataToSend = bytes;
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
