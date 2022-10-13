using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Client : MonoBehaviour
{
    Socket clientSocket;
    IPEndPoint clientIpep;

    int recv = 0; // Size of the data
    byte[] data;
    [SerializeField] string clientIp;
    [SerializeField] string serverIp;
    
    EndPoint remote = null;

    Thread netThread;
    bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(clientIp), 5497);
        clientSocket.Bind(clientIpep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5497);

        data = new byte[1024];

        netThread = new Thread(RecieveMessages);
        netThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            string text = "Un saludo desde" + clientIpep.Address.ToString();
            data = Encoding.ASCII.GetBytes(text);
            recv = data.Length;
            clientSocket.SendTo(data, recv, SocketFlags.None, remote);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            finished = true;
        }
    }

    void RecieveMessages()
    {
        while (!finished)
        {

            if (remote == null)
                return;
            recv = clientSocket.ReceiveFrom(data, SocketFlags.None, ref remote);
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));

        }
    }
}
