using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Text;
using UnityEngine.UI;

public class ClientTCP : MonoBehaviour
{
    public Socket socket;
    public IPEndPoint ipep;
    bool connected = false;
    bool startListening = false;
    int port = 3442;
    private string playerName;
    Thread connectionThread;
    Thread receiveMessagesThread;

    // Connect player
    public InputField playerNameInput;
    public InputField serverIpAddress;
    public InputField chatMessages;
    bool firstConnection = false;
    public GameObject connectionCanvas;
    public GameObject waitingRoomCanvas;

    // Start is called before the first frame update
    void Start()
    {
        playerName = "Lucas Perez";
        // Start a new socket TCP type
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Create an endpoint with our ip adress (Client)
        ipep = new IPEndPoint(IPAddress.Parse("192.168.0.31"), port);
        Debug.Log("Connecting");
        // Bind socket with our ip
        socket.Bind(ipep);

      
    }

    // Update is called once per frame
    void Update()
    {
        if (firstConnection)
        {
            // TODO: This is for chat messages
            //Thread thread = new Thread(SendMessage);
            //thread.Start();

            // Connect to server
            connectionThread = new Thread(StartClient);
            connectionThread.Start();

            connectionCanvas.SetActive(false);
            waitingRoomCanvas.SetActive(true);


            firstConnection = false;
        }

        if (connected && !startListening)
        {
            // Receive messages from the server
            receiveMessagesThread = new Thread(ReceiveMessageThread);
            receiveMessagesThread.Start();

            startListening = true;
        }

        if (waitingRoomCanvas.activeSelf)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                socket.Send(Encoding.ASCII.GetBytes(chatMessages.text));
            }
        }
    }

    void StartClient()
    {
        // Get the endpoint of the server
        IPEndPoint ipepServer = new IPEndPoint(IPAddress.Parse(serverIpAddress.text), port);
        Debug.Log("Connecting to server");
        // Connect our socket to the server end point
        socket.Connect(ipepServer);
        Debug.Log("Connected!");

        // Send message to the server
        socket.Send(Encoding.ASCII.GetBytes("Player " + playerNameInput.text + " connected"));
        socket.Send(Encoding.ASCII.GetBytes("Hola sucio cerdo"));

        connected = true;
    }

    void SendMessage()
    {
        Debug.Log(playerNameInput.text);
        socket.Send(Encoding.ASCII.GetBytes("Player " + playerNameInput.text + " connected"));
    }

    void ReceiveMessageThread()
    {
        while (connected)
        {
            byte[] info = new byte[1024];
            int size = socket.Receive(info);

            Debug.Log(Encoding.ASCII.GetString(info, 0, size));
        }
    }

    public void Connect()
    {
        firstConnection = true;
    }

    private void OnDisable()
    {
        if (socket != null) socket.Close();

        if (connectionThread.IsAlive) connectionThread.Abort();
    }
}
