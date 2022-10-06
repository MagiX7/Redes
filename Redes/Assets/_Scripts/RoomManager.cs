using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    Socket udpSocket;
    IPEndPoint ipep; // My socket endpoint
    IPEndPoint clientIpep; // Client Endpoint
    [SerializeField]
    string clientIp;


    int recv = 0; // Size of the data
    byte[] data;
    EndPoint remote;


    Thread netThread;

    // Start is called before the first frame update
    void Start()
    {
        udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
        ipep = new IPEndPoint(IPAddress.Parse("10.0.103.46"), 5497);
        udpSocket.Bind(ipep);
        
        clientIpep = new IPEndPoint(IPAddress.Parse(clientIp), 5497);
        remote = clientIpep;

        netThread = new Thread(RecieveMessages);
        netThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.S))
        {
            string text = "Un saludo desde" + ipep.Address.ToString();
            data = Encoding.ASCII.GetBytes(text);
            recv = data.Length;
            udpSocket.SendTo(data, recv, SocketFlags.None, clientIpep);
        }
    }

    void RecieveMessages()
    {
        recv = udpSocket.ReceiveFrom(data, SocketFlags.None, ref remote);
        Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
    }

    private void OnDisable()
    {
        udpSocket.Close();
        if (netThread.IsAlive)
            netThread.Abort();
    }

}
