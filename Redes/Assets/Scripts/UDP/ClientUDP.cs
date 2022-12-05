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
using UnityEngine.SceneManagement;

public class ClientUDP : MonoBehaviour
{
    Socket clientSocket;
    IPEndPoint clientIpep;

    int recv = 0;
    byte[] data;
    [HideInInspector] public string clientIp;
    [HideInInspector] public string serverIp;
    [HideInInspector] public string userName;
    
    EndPoint remote = null;
    int netId = -1;

    Thread receiveMsgsThread;

    bool finished = false;
    //bool newMessage = false;

    //string incomingText;

    //[SerializeField] InputField input;

    [SerializeField] EnemyController enemy;
    [SerializeField] ClientSceneManagerUDP sceneManager;
    ConnectionsManager connectionsManager;
    
    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        clientSocket.Bind(clientIpep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        data = new byte[1024];
        data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, netId, userName);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, remote);
        data = new byte[1024];

        receiveMsgsThread = new Thread(RecieveMessages);
        receiveMsgsThread.Start();

        connectionsManager = GameObject.Find("Connections Manager").GetComponent<ConnectionsManager>();
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
        //if (newMessage)
        //{
        //    //chat.text += (incomingText + "\n");
        //    sceneManager.OnNewChatMessage(incomingText);
        //    newMessage = false;
        //}

        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    OnMessageSent();
        //}
    }

    void RecieveMessages()
    {
        while (!finished)
        {
            byte[] bytes = new byte[1024];
            recv = clientSocket.ReceiveFrom(bytes, SocketFlags.None, ref remote);

            if (recv > 0)
            {
                int incomingNetId;
                connectionsManager.OnMessageReceived(bytes, out _, out incomingNetId);
                if(incomingNetId > 0) netId = incomingNetId;
            }

            //MemoryStream stream = new MemoryStream(bytes, 0, recv);
            //BinaryReader reader = new BinaryReader(stream);
            //
            //stream.Seek(0, SeekOrigin.Begin);
            //
            //MessageType messageType = (MessageType)reader.ReadInt32();
            //switch (messageType)
            //{
            //    case MessageType.NEW_USER:
            //    {
            //        // New Player Connected
            //        incomingText = Serializer.DeserializeString(reader, stream);
            //        break;
            //    }
            //    case MessageType.CHAT:
            //        incomingText = Serializer.DeserializeString(reader, stream);
            //        newMessage = true;
            //        data = bytes;
            //        break;
            //    case MessageType.PLAYER_DATA:
            //         enemy.playerData = Serializer.DeserializePlayerData(reader, stream);
            //        break;
            //
            //    case MessageType.START_GAME:
            //        sceneManager.StartClient();
            //        break;
            //
            //    default:
            //        break;
            //}
        }
    }

    void OnMessageSent()
    {
        //string msg = "[" + userName + "]" + ": " + input.text;
        //data = Serializer.SerializeStringWithHeader(MessageType.CHAT, netId, msg);
        //recv = data.Length;
        //clientSocket.SendTo(data, recv, SocketFlags.None, remote);
        //input.text = "";
    }

    public void Send(byte[] bytes)
    {
        clientSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
    }

    public void SetNetId(int value)
    {
        netId = value;
    }

    public int GetNetId() { return netId; }

    public string GetUserName() { return userName; }

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
