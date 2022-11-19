using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerConnectionTCP : MonoBehaviour
{
    // Connection Variables
    private Socket serverSocket;
    private List<Socket> clientSocket;
    private IPEndPoint ipep;
    private int port = 5345;
    private Thread threadTCPConnection;
    private Thread threadReceiveTCPMessages;
    private bool startListening = false;


    // User-Related variables
    public InputField chatServerInput;
    private bool newChatMessage = false;
    bool newPlayer = false;
    public Text playersConnectedText;
    List<string> playerConnectionList;
    public Text playerChatMessagesText;
    List<string> playerChatMessagesList;

    void Start()
    {
        clientSocket = new List<Socket>();
        playerConnectionList = new List<string>();
        playerChatMessagesList = new List<string>();

        serverSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), port);
        serverSocket.Bind(ipep);

        threadTCPConnection = new Thread(ThreadTCPConnect);
        threadTCPConnection.Start();

        threadReceiveTCPMessages = new Thread(ThreadReceiveTCPMessage);
    }

    void Update()
    {
        if (clientSocket.Count > 0 && !startListening)
        {
            if (!threadReceiveTCPMessages.IsAlive)
            {
                threadReceiveTCPMessages.Start();
                startListening = true;
            }
        }
    }


    private void ThreadTCPConnect()
    {
        while (clientSocket.Count < 2)
        {
            serverSocket.Listen(2);
            clientSocket.Add(serverSocket.Accept());

            byte[] info = new byte[1024];
            string clientName = "";
            int siz = clientSocket[clientSocket.Count - 1].Receive(info);
            clientName = Encoding.ASCII.GetString(info, 0, siz);

            playerConnectionList.Add(clientName);
            newPlayer = true;

            string messageToClient = "Player " + clientName + " connected to server: Middle Ambient" + "\n";
            byte[] buffer = new byte[messageToClient.Length];
            buffer = Encoding.ASCII.GetBytes(messageToClient);

            clientSocket[clientSocket.Count - 1].Send(buffer);

        }    
    }

    private void ThreadReceiveTCPMessage()
    {
        while (clientSocket.Count > 0)
        {
            List<Socket> receiveSockets = new List<Socket>(clientSocket);
            Socket.Select(receiveSockets, null, null, 50000);
            for (int i = 0; i < receiveSockets.Count; ++i)
            {
                byte[] info = new byte[1024];
                int siz = receiveSockets[i].Receive(info);
                string clientMessage = Encoding.ASCII.GetString(info, 0, siz);
          
                playerChatMessagesList.Add(clientMessage);
                newChatMessage = true;

                byte[] buffer = new byte[clientMessage.Length];
                buffer = Encoding.ASCII.GetBytes(clientMessage);
                for (int j = 0; j < clientSocket.Count; ++j)
                {
                    clientSocket[j].Send(buffer);
                }
            }            
        }

        startListening = false;
    }

    private void OnDisable()
    {
        if (threadTCPConnection.IsAlive)
        {
            threadTCPConnection.Abort();
        }

        if (threadReceiveTCPMessages.IsAlive)
        {
            threadReceiveTCPMessages.Abort();
        }

        for (int i = 0; i < clientSocket.Count; ++i)
        {
            clientSocket[i].Close();
        }
        if (serverSocket != null)
        {
            serverSocket.Close();
        }
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

    public List<Socket> GetClientSockets()
    {
        return clientSocket;
    }
}