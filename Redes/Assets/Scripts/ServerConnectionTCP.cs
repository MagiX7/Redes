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
    enum InfoType
    {
        MESSAGE = 0,
        CONNECTION
    };

    private Socket serverSocket;
    private List<Socket> clientSocket;
    private IPEndPoint ipep;
    private int port = 3438;
    private Thread threadTCPConnection;
    private Thread threadReceiveTCPMessages;
    private bool startListening = false;
    private bool newChatMessage = false;


    // Gameplay variables
    bool newPlayer = false;
    private Text playersConnectedText;
    List<string> playerConnectionList;
    public Text playerChatMessagesText;
    List<string> playerChatMessagesList;

    // Start is called before the first frame update
    void Start()
    {
        clientSocket = new List<Socket>();
        playerConnectionList = new List<string>();
        playerChatMessagesList = new List<string>();
        playersConnectedText = GameObject.Find("Players").GetComponent<Text>();

        serverSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("192.168.0.12"), port);

        serverSocket.Bind(ipep);

        // First Thread to begin connection
        threadTCPConnection = new Thread(ThreadTCPConnect);
        threadTCPConnection.Start();

        // Thread to keep listening for messages while at least 1 client is connected
        threadReceiveTCPMessages = new Thread(ThreadReceiveTCPMessage);
    }

    // Update is called once per frame
    void Update()
    {
        if (newPlayer)
        {
            playersConnectedText.text += playerConnectionList[playerConnectionList.Count-1] + "\n";
            newPlayer = false;
        }

        if (newChatMessage)
        {
            playerChatMessagesText.text += playerChatMessagesList[playerChatMessagesList.Count-1] + "\n";
            newChatMessage = false;

        }

        // While at least there's 1 client we can call this function
        if (clientSocket.Count > 0 && !startListening)
        {
            if (!threadReceiveTCPMessages.IsAlive)
            {
                Debug.Log("Server started listening for messages...");
                threadReceiveTCPMessages.Start();
                startListening = true;
            }
        }
    }


    private void ThreadTCPConnect()
    {
        Debug.Log("Listening for other players...");
        // Keep listening until someone connects, then accept it
        serverSocket.Listen(1);
        clientSocket.Add(serverSocket.Accept());
        
        // Receive message
        byte[] info = new byte[1024];
        string clientName = "";
        for (int i = 0; i < clientSocket.Count; ++i)
        {
            int siz = clientSocket[i].Receive(info);
            clientName = Encoding.ASCII.GetString(info, 0, siz);
            Debug.Log("Client connected " + siz + " Message: " + clientName);
            playerConnectionList.Add(clientName);
            newPlayer = true;
        }
       

        // Send message to client that he connected successfully
        string messageToClient = "Player " + clientName + " connected to server: Middle Ambient" + "\n";
        byte[] buffer = new byte[messageToClient.Length];
        buffer = Encoding.ASCII.GetBytes(messageToClient);
        for (int i = 0; i < clientSocket.Count; ++i)
        {
            clientSocket[i].Send(buffer);
        }
        
    }

    private void ThreadReceiveTCPMessage()
    {
        // While the client is connected, we receive messages,
        // this way we avoid creating new threads
        while (clientSocket.Count > 0)
        {
            for (int i = 0; i < clientSocket.Count; ++i)
            {
                // Receive message and convert it to string for debug
                byte[] info = new byte[1024];
                int siz = clientSocket[i].Receive(info);
                string clientMessage = Encoding.ASCII.GetString(info, 0, siz);

                // Add it to player messages list
                if (playerChatMessagesList.Count > 3) playerChatMessagesList.RemoveAt(0);
          
                playerChatMessagesList.Add(clientMessage);
                newChatMessage = true;
                Debug.Log("Client said: " + clientMessage);


                // Send message to client that he connected successfully
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
        // Correctly close and abort all sockets and threads
        // Close sockets

        for (int i = 0; i < clientSocket.Count; ++i)
        {
            clientSocket[i].Close();
            Debug.Log("Closed client socket" + i);
        }


        if (serverSocket != null)
        {
            serverSocket.Close();
            Debug.Log("Closed server socket");
        }
       

        // Abort Threads
        if (threadTCPConnection.IsAlive)
        {
            threadTCPConnection.Abort();
            Debug.Log("Aborted connections thread");
        }

        if (threadReceiveTCPMessages.IsAlive)
        {
            threadReceiveTCPMessages.Abort();
            Debug.Log("Aborted receive messages thread");
        }
    }
}