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

    // Start is called before the first frame update
    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(clientIp), 5497);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5497);
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
    }
}
