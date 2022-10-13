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
    private Socket serverSocket;
    private Socket clientSocket;
    private IPEndPoint ipep;
    private int port = 3436;
    private Thread threadTCPConnection;
    private Thread threadReceiveTCPMessages;


    // Gameplay variables
    bool newPlayer = false;
    private Text players;
    List<string> playerList;

    // Start is called before the first frame update
    void Start()
    {
        playerList = new List<string>();
        players = GameObject.Find("Players").GetComponent<Text>();

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
            players.text += playerList[playerList.Count-1] + "\n";
            newPlayer = false;
        }
        
        if (Input.GetKeyUp(KeyCode.M))
        {
            try
            {
                string test = "Hola";
                clientSocket.Send(Encoding.ASCII.GetBytes(test), test.Length, SocketFlags.None);
            }
            catch (Exception error)
            {
                Debug.Log("Couldn't send a message to the client: " + error);
            }
        }

        // While at least there's 1 client we can call this function
        if (clientSocket != null)
        {
            if (!threadReceiveTCPMessages.IsAlive)
            {
                Debug.Log("Server started listening for messages...");
                threadReceiveTCPMessages.Start();
            }
        }
    }


    private void ThreadTCPConnect()
    {
        // Keep listening until someone connects, then accept it
        serverSocket.Listen(1);
        clientSocket = serverSocket.Accept();

        // Receive message
        byte[] info = new byte[1024];
        int siz = clientSocket.Receive(info);
        Debug.Log("Client connected " + siz + " Message: " + Encoding.ASCII.GetString(info, 0, siz));

        // Send message to client that he connected successfully
        string messageToClient = "Server: Middle Ambient";
        byte[] buffer = new byte[messageToClient.Length];
        buffer = Encoding.ASCII.GetBytes(messageToClient);
        clientSocket.Send(buffer);
    }

    private void ThreadReceiveTCPMessage()
    {
        // While the client is connected, we receive messages,
        // this way we avoid creating new threads
        while (clientSocket != null)
        {
            byte[] info = new byte[1024];
            int siz = clientSocket.Receive(info);
            string clientMessage = Encoding.ASCII.GetString(info, 0, siz);

            playerList.Add(clientMessage);
            newPlayer = true;

            Debug.Log("Client said: " + clientMessage);

        }
    }

    private void OnDisable()
    {
        // Correctly close and abort all sockets and threads
        // Close sockets
        clientSocket.Close();
        Debug.Log("Closed client socket");
        serverSocket.Close();
        Debug.Log("Closed server socket");

        // Abort Threads
        threadTCPConnection.Abort();
        Debug.Log("Aborted connections thread");
        threadReceiveTCPMessages.Abort();
        Debug.Log("Aborted receive messages thread");
    }
}