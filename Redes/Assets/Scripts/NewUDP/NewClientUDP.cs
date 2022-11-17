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

public class NewClientUDP : MonoBehaviour
{
    Socket clientSocket;
    IPEndPoint clientIpep;

    int recv = 0;
    byte[] data;
    [HideInInspector] public string clientIp;
    [HideInInspector] public string serverIp;
    [HideInInspector] public string userName;

    EndPoint remote = null;

    Thread receiveMsgsThread;

    bool finished = false;
    bool newMessage = false;

    string incomingText;

    [SerializeField] UnityEngine.UI.Text chat;
    [SerializeField] InputField input;

    List<string> listOfPlayers;

    [SerializeField] NewPlayerController player;
    [SerializeField] NewUDPManager udpManager;
    string enemyIp;

    // Start is called before the first frame update
    void Start()
    {
        listOfPlayers = new List<string>();

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        clientSocket.Bind(clientIpep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        data = new byte[1024];
        data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, userName);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, remote);
        data = new byte[1024];

        udpManager.NewEnemy(remote);

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

    // Update is called once per frame
    void Update()
    {
        MessagesUpdate();
    }

    void MessagesUpdate()
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
            TypeMsg(msg, stream, reader, messageType);
        }
    }

    private void TypeMsg(byte[] msg, MemoryStream stream, BinaryReader reader, MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.NEW_USER:
                // New Player Connected
                incomingText = Serializer.DeserializeString(reader, stream);
                break;
            case MessageType.CHAT:
                //incomingText = Encoding.ASCII.GetString(msg, 0, recv);
                incomingText = Serializer.DeserializeString(reader, stream);
                newMessage = true;
                data = msg;
                break;
            case MessageType.PLAYER_DATA:
                PlayerData dataAux = Serializer.NewDeserializePlayerData(reader, stream, ref enemyIp);
                if (enemyIp == GetLocalIPAddress())
                {
                    NewPlayerController aux = player.GetComponent<NewPlayerController>();
                    aux.playerData = dataAux;
                }
                else
                    udpManager.UpdateEnemy(dataAux, enemyIp);
                break;

            //case MessageType.SHOOT:
            //    enemy.canShoot = Serializer.DeserializeBool(reader, stream);
            //    break;
            case MessageType.NEW_PLAYER:
                listOfPlayers = Serializer.DeserializePlayerList(reader, stream);
                listOfPlayers.Remove(GetLocalIPAddress());
                for (int i = 0; i < listOfPlayers.Count; i++)
                {
                    string auxIp = listOfPlayers[i];
                    bool found = false;

                    foreach (var enemy in udpManager.enemies)
                    {
                        if (enemy.GetComponent<NewEnemyController>().ip == auxIp)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        udpManager.NewEnemy(auxIp);
                }
                break;

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

    public void SendPlayerData(PlayerData data)
    {

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
