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
    Thread thread;

    // Connect player
    public InputField playerNameInput;
    bool connect = false;

    // Start is called before the first frame update
    void Start()
    {
        playerName = "Lucas Perez";
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("192.168.0.31"), port);
        Debug.Log("Connecting");
        socket.Bind(ipep);

        thread = new Thread(StartClient);
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (connect)
        {
            Thread thread = new Thread(SendMessage);
            thread.Start();
            connect = false;
        }

        if (connected && !startListening)
        {
            Thread thread = new Thread(ReceiveMessageThread);
            thread.Start();
        }
    }

    void StartClient()
    {
        IPEndPoint ipepServer = new IPEndPoint(IPAddress.Parse("192.168.0.12"), port);
        Debug.Log("Connecting to server");
        socket.Connect(ipepServer);
        Debug.Log("Connected!");

        socket.Send(Encoding.ASCII.GetBytes("Hola sucio cerdo"));

        connected = true;
    }

    void SendMessage()
    {
        //IPEndPoint ipepServer = new IPEndPoint(IPAddress.Parse("10.0.103.50"), 5496);
        //socket.Connect(ipepServer);
        Debug.Log(playerNameInput.text);
        socket.Send(Encoding.ASCII.GetBytes("Player" + playerNameInput.text + " connected"));
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
        connect = true;
    }

    private void OnDisable()
    {
        if (socket != null) socket.Close();

        if (thread.IsAlive) thread.Abort();
    }
}
