using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    Socket server;
    IPEndPoint ipep;
    int recv = 0; // Size of the data
    byte[] data;

    [SerializeField] string serverIp;

    EndPoint remote = null;
    IPEndPoint sender;


    Thread netThread;
    bool finished = false;
    bool newMessage = false;

    string text;
    List<EndPoint> remoters;

    [SerializeField] Text chat;
    [SerializeField] InputField input;

    // Start is called before the first frame update
    void Start()
    {
        //server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //ipep = new IPEndPoint(IPAddress.Any, 5497);
        //server.Bind(ipep);
        //
        //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        //remote = (EndPoint)sender;

        remoters = new List<EndPoint>();

        data = new byte[1024];

        ipep = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        server = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp);
        server.Bind(ipep);

        Debug.Log("Waiting for a client...");
        Debug.Log("Server IP: " + ipep.ToString());

        sender = new IPEndPoint(IPAddress.Parse(serverIp), 5345);
        remote = (EndPoint)(sender);

        netThread = new Thread(RecieveMessages);
        netThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            finished = true;
        }

        //if (Input.GetKeyUp(KeyCode.A))
        //{
        //    text = "Server message";
        //    data = Encoding.ASCII.GetBytes(text);
        //    recv = data.Length;
        //    remote = sender;
        //    server.SendTo(data, recv, SocketFlags.None, remote);
        //}

        if (newMessage)
        {
            Debug.Log(text + " Update");
            data = Encoding.ASCII.GetBytes(text);
            recv = data.Length;
            for (int i = 0; i < remoters.Count; i++)
            {
                server.SendTo(data, recv, SocketFlags.None, remoters[i]);
            }
            chat.text += (text + "\n");
            newMessage = false;
            data = new byte[1024];
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            data = Encoding.ASCII.GetBytes(input.text);
            recv = data.Length;
            for (int i = 0; i < remoters.Count; i++)
            {
                server.SendTo(data, recv, SocketFlags.None, remoters[i]);
            }
            chat.text += (input.text + "\n");
            Debug.Log(input.text + " Send");
            input.text = "";
            data = new byte[1024];
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

            if (!remoters.Contains(remote))
            {
                remoters.Add(remote);
            }
        }
    }

    private void OnDisable()
    {
        server.Close();
        if (netThread.IsAlive)
            netThread.Abort();
    }
}
