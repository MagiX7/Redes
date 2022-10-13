using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket server;
    IPEndPoint ipep;
    int recv = 0; // Size of the data
    byte[] data;

    EndPoint remote = null;
    
    Thread netThread;
    bool finished = false;

    // Start is called before the first frame update
    void Start()
    {
        //server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //ipep = new IPEndPoint(IPAddress.Any, 5497);
        //server.Bind(ipep);
        //
        //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        //remote = (EndPoint)sender;

        data = new byte[1024];

        ipep = new IPEndPoint(IPAddress.Any, 5497);

        server = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp);
        server.Bind(ipep);

        Debug.Log("Waiting for a client...");
        Debug.Log("Server IP: " + ipep.ToString());

        IPEndPoint sender = new IPEndPoint(IPAddress.Parse("10.0.103.35"), 0);
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

        if (Input.GetKeyUp(KeyCode.A))
        {
            //string text = ;
            data = Encoding.ASCII.GetBytes("Un saludo desde" + ipep.Address.ToString());
            recv = data.Length;
            server.SendTo(data, recv, SocketFlags.None, remote);
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
            Debug.Log(Encoding.ASCII.GetString(data, 0, recv));
            data = new byte[1024];
        }
    }

    private void OnDisable()
    {
        server.Close();
        if (netThread.IsAlive)
            netThread.Abort();
    }
}
