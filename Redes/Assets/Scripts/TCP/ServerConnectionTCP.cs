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
    private int port = 3438;
    private Thread threadTCPConnection;
    private Thread threadReceiveTCPMessages;
    private bool startListening = false;


    // User-Related variables
    public InputField chatServerInput;
    private bool newChatMessage = false;
    bool newPlayer = false;
    private Text playersConnectedText;
    List<string> playerConnectionList;
    public Text playerChatMessagesText;
    List<string> playerChatMessagesList;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize lists and get components
        clientSocket = new List<Socket>();
        playerConnectionList = new List<string>();
        playerChatMessagesList = new List<string>();
        playersConnectedText = GameObject.Find("Players").GetComponent<Text>();

        // Input the options for our server, TCP in this case
        serverSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), port);
        serverSocket.Bind(ipep);

        // Begin connection in a new thread
        threadTCPConnection = new Thread(ThreadTCPConnect);
        threadTCPConnection.Start();

        // Thread to keep listening for messages while at least 1 client is connected
        threadReceiveTCPMessages = new Thread(ThreadReceiveTCPMessage);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            // Prepare the messages to send to the clients
            string messageToClient = "[Server]: " + chatServerInput.text + "\n";
            playerChatMessagesList.Add(messageToClient);
            playerChatMessagesText.text += messageToClient;
            chatServerInput.text = "";

            // Message limit behaviour
            if (playerChatMessagesList.Count > 5)
            {
                playerChatMessagesList.RemoveAt(0);
                playerChatMessagesText.text = "";
                for (int i = 0; i < playerChatMessagesList.Count; ++i)
                {
                    playerChatMessagesText.text += playerChatMessagesList[i];
                }
            }

            // Send message to the clients
            byte[] buffer = new byte[messageToClient.Length];
            buffer = Encoding.ASCII.GetBytes(messageToClient);
            for (int i = 0; i < clientSocket.Count; ++i)
            {
                clientSocket[i].Send(buffer);
            }
        }

        if (newPlayer)
        {
            // Message limit behaviour
            if (playerChatMessagesList.Count > 5)
            {
                playerChatMessagesList.RemoveAt(0);
                playerChatMessagesText.text = "";
                for (int j = 0; j < playerChatMessagesList.Count; ++j)
                {
                    playerChatMessagesText.text += playerChatMessagesList[j];
                }
            }
       
            playerChatMessagesText.text += playerConnectionList[playerConnectionList.Count - 1] + " connected!\n";

            // Add it to list of players
            playersConnectedText.text += playerConnectionList[playerConnectionList.Count-1] + "\n";
            newPlayer = false;
        }

        if (newChatMessage)
        {
            // Message limit behaviour
            if (playerChatMessagesList.Count > 5)
            {
                playerChatMessagesList.RemoveAt(0);
                playerChatMessagesText.text = "";
                for (int j = 0; j < playerChatMessagesList.Count; ++j)
                {
                    playerChatMessagesText.text += playerChatMessagesList[j];
                }
            }
            else
            {
                playerChatMessagesText.text += playerChatMessagesList[playerChatMessagesList.Count-1] + "\n";
            }
            newChatMessage = false;
        }

        // While at least there's 1 client we can call this function
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
            // Keep listening until someone connects, then accept it
            serverSocket.Listen(2);
            clientSocket.Add(serverSocket.Accept());

            // Receive message
            byte[] info = new byte[1024];
            string clientName = "";
            int siz = clientSocket[clientSocket.Count - 1].Receive(info);
            clientName = Encoding.ASCII.GetString(info, 0, siz);

            // Add the new client to the connection list
            playerConnectionList.Add(clientName);
            newPlayer = true;

            // Send message to client that he connected successfully
            string messageToClient = "Player " + clientName + " connected to server: Middle Ambient" + "\n";
            byte[] buffer = new byte[messageToClient.Length];
            buffer = Encoding.ASCII.GetBytes(messageToClient);

            clientSocket[clientSocket.Count - 1].Send(buffer);

        }    
    }

    private void ThreadReceiveTCPMessage()
    {
        // While the client is connected, we receive messages,
        // this way we avoid creating new threads
        while (clientSocket.Count > 0)
        {   
            // This select is used for checking if there's pending messages, if there's any
            // The select will reduce the list to the many players that have pending messages
            List<Socket> receiveSockets = new List<Socket>(clientSocket);
            Socket.Select(receiveSockets, null, null, 50000);
            for (int i = 0; i < receiveSockets.Count; ++i)
            {
                // Receive message and convert it to string for debug
                byte[] info = new byte[1024];
                int siz = receiveSockets[i].Receive(info);
                string clientMessage = Encoding.ASCII.GetString(info, 0, siz);
          
                playerChatMessagesList.Add(clientMessage);
                newChatMessage = true;


                // Send message to all clients
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
        }


        if (serverSocket != null)
        {
            serverSocket.Close();
        }
       

        // Abort Threads
        if (threadTCPConnection.IsAlive)
        {
            threadTCPConnection.Abort();
        }

        if (threadReceiveTCPMessages.IsAlive)
        {
            threadReceiveTCPMessages.Abort();
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
}