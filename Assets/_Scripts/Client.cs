using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    Socket server;
    IPEndPoint clientIpep;

    int recv = 0; // Size of the data
    byte[] data;
    [SerializeField] string clientIp;
    [SerializeField] string serverIp;
    
    EndPoint remote = null;
    IPEndPoint sender;

    Thread netThread;
    bool finished = false;
    bool newMessage = false;

    string text;

    [SerializeField] Text chat;
    [SerializeField] InputField input;

    // Start is called before the first frame update
    void Start()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(clientIp), 5345);
        server.Bind(clientIpep);

        sender = new IPEndPoint(IPAddress.Parse(serverIp), 5345);
        remote = (EndPoint)(sender);

        data = new byte[1024];

        netThread = new Thread(RecieveMessages);
        netThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.S))
        //{
        //    string text = "Un saludo desde" + clientIpep.Address.ToString();
        //    data = Encoding.ASCII.GetBytes(text);
        //    recv = data.Length;
        //    server.SendTo(data, recv, SocketFlags.None, remote);
        //}

        if (Input.GetKeyDown(KeyCode.F))
        {
            finished = true;
        }

        if (newMessage)
        {
            Debug.Log(text + " Update");
            chat.text += (text + "\n");
            newMessage = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            data = Encoding.ASCII.GetBytes(input.text);
            recv = data.Length;
            server.SendTo(data, recv, SocketFlags.None, remote);
            Debug.Log(input.text + " Send");
            input.text = "";
        }
    }

    void RecieveMessages()
    {
        while (!finished)
        {
            if (remote == null)
                return;
            recv = server.ReceiveFrom(data, SocketFlags.None, ref remote);
            text = Encoding.ASCII.GetString(data, 0, recv);
            Debug.Log(text + " Received");
            newMessage = true;
            data = new byte[1024];

        }
    }
}
