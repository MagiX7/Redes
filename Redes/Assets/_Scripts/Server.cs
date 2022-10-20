using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    Socket server;
    IPEndPoint ipep;
    int recv = 0; // Size of the data
    byte[] data;

    EndPoint remote = null;
    IPEndPoint sender;


    Thread netThread;
    bool finished = false;
    bool newMessage = false;

    bool clientConnected = false;
    string lastUserName = string.Empty;


    string text;
    List<EndPoint> remoters;

    [SerializeField] Text chat;
    [SerializeField] InputField input;
    [SerializeField] Text connectedPeople;

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

        ipep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);

        server = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp);
        server.Bind(ipep);

        //Debug.Log("Waiting for a client...");
        Debug.Log("Server IP: " + ipep.ToString());

        sender = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        remote = (EndPoint)(sender);

        netThread = new Thread(RecieveMessages);
        netThread.Start();

        connectedPeople.text += ("You(Server)\n");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            finished = true;
        }

        if (!clientConnected && newMessage)
        {
            try
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
            catch (Exception e)
            {
                Debug.Log("Error when sending message: " + e);
            }
        }
        else if (clientConnected)
        {
            byte[] msg = new byte[1024];
            string text = "Welcome to the UDP server";
            //msg = Encoding.ASCII.GetBytes(text);
            //server.SendTo(msg, msg.Length, SocketFlags.None, remote);

            //text = lastUserName + " Connected!\n";
            //msg = Encoding.ASCII.GetBytes(text);
            for (int i = 0; i < remoters.Count; i++)
            {
                if (remote == remoters[i])
                {
                    text = "Welcome to the UDP server";
                    msg = Encoding.ASCII.GetBytes(text);
                    server.SendTo(msg, msg.Length, SocketFlags.None, remoters[i]);
                }
                else
                {
                    text = lastUserName + " Connected!\n";
                    msg = Encoding.ASCII.GetBytes(text);
                    server.SendTo(msg, msg.Length, SocketFlags.None, remoters[i]);
                }
            }
            chat.text += (lastUserName + " Connected!\n");
            connectedPeople.text += (lastUserName + "\n");

            clientConnected = false;
            newMessage = false;

            data = new byte[1024];
            lastUserName = string.Empty;

            Debug.Log(text + " Send");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            try
            {
                data = Encoding.ASCII.GetBytes("Server: " + input.text);
                recv = data.Length;
                for (int i = 0; i < remoters.Count; i++)
                {
                    server.SendTo(data, recv, SocketFlags.None, remoters[i]);
                }
                chat.text += ("Server: " + input.text + "\n");
                Debug.Log(input.text + " Send");
                input.text = "";
                data = new byte[1024];
            }
            catch (Exception e)
            {
                Debug.Log("Error when sending message: " + e);
            }
        }
    }

    void RecieveMessages()
    {
        //server.Listen(1);
        //remoters.Add(server.Accept().RemoteEndPoint);

        while (!finished)
        {
            if (remote == null)
                return;

            try
            {
                byte[] msg = new byte[1024];
                recv = server.ReceiveFrom(msg, SocketFlags.None, ref remote);
                if (recv > 0)
                {
                    text = Encoding.ASCII.GetString(msg, 0, recv);

                    Debug.Log(text + " Received");
                    newMessage = true;

                    data = msg;

                    if (!remoters.Contains(remote))
                    {
                        clientConnected = true;
                        lastUserName = text;
                        remoters.Add(remote);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log("Error when receiving a message: " + e);
            }

        }
    }

    private void OnDisable()
    {
        server.Close();
        if (netThread.IsAlive)
            netThread.Abort();
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
