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
    int port = 5345;
    Thread connectionThread;
    Thread receiveMessagesThread;

    // Connect player
    public InputField playerNameInput;
    private string playerName;
    public InputField serverIpAddress;
    public InputField chatMessages;
    bool firstConnection = false;
    public GameObject connectionCanvas;
    public GameObject waitingRoomCanvas;
    private bool resetText = false;

    // Waiting room
    private bool newChatMessage = false;
    private List<string> chatMessagesList;
    public Text chatMessagesText;

    private bool closeSocket = false;

    void Start()
    {
        chatMessagesList = new List<string>();

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), port);
    }

    // Update is called once per frame
    void Update()
    {
        if (firstConnection)
        {
            connectionThread = new Thread(StartClient);
            connectionThread.Start();

            connectionCanvas.SetActive(false);
            waitingRoomCanvas.SetActive(true);

            firstConnection = false;
        }

        if (connected && !startListening)
        {
            receiveMessagesThread = new Thread(ReceiveMessageThread);
            receiveMessagesThread.Start();

            startListening = true;
        }

        if (waitingRoomCanvas.activeSelf)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                socket.Send(Encoding.ASCII.GetBytes("[" + playerName + "]: " + chatMessages.text + "\n"));
                
                chatMessages.text = "";
            }
        }

        if (newChatMessage)
        {
            chatMessagesText.text += chatMessagesList[chatMessagesList.Count - 1];

            newChatMessage = false;
        }

        if (resetText)
        {
            chatMessagesList.RemoveAt(0);
            chatMessagesText.text = "";
            for (int i = 0; i < chatMessagesList.Count; ++i)
            {
                chatMessagesText.text += chatMessagesList[i];
            }
            resetText = false;
        }
    }

    void StartClient()
    {
        IPEndPoint ipepServer = new IPEndPoint(IPAddress.Parse(serverIpAddress.text), port);

        socket.Connect(ipepServer);
        socket.Send(Encoding.ASCII.GetBytes(playerNameInput.text));

        playerName = playerNameInput.text;

        connected = true;
    }

    void ReceiveMessageThread()
    {
        while (connected)
        {
            if (closeSocket)
            {
                socket.Close();
                return;
            }

            byte[] info = new byte[1024];
            int size = socket.Receive(info);

            string chatMessage = Encoding.ASCII.GetString(info, 0, size);

            newChatMessage = true;
            if (chatMessagesList.Count > 3)
            {
                resetText = true;
            }

            chatMessagesList.Add(chatMessage);
        }
    }

    public void Connect()
    {
        firstConnection = true;
    }

    private void OnDisable()
    {
        closeSocket = true;

        connected = false;
        if (connectionThread != null && connectionThread.IsAlive)
        {
            connectionThread.Abort();
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
