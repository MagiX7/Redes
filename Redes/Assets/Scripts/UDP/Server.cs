using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    Socket serverSocket;
    IPEndPoint ipep;
    int recv = 0; // Size of the data
    byte[] data;

    EndPoint remote = null;

    Thread receiveMsgsThread;
    //Thread sendMsgsThread;
    //Thread updateMsgsThread;
    bool finished = false;

    bool newMessage = false;
    bool messageSent = false;
    //bool sendingMsg = false;
    //bool msgReceived = false;
    bool clientConnected = false;
    string lastUserName = string.Empty;

    string text;
    List<EndPoint> remoters;

    [SerializeField] Text chat;
    [SerializeField] InputField input;
    [SerializeField] Text connectedPeople;

    void Start()
    {
        remoters = new List<EndPoint>();

        data = new byte[1024];

        ipep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverSocket.Bind(ipep);

        remote = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);

        receiveMsgsThread = new Thread(RecieveMessages);
        receiveMsgsThread.Start();

        //sendMsgsThread = new Thread(OnMessageSent);
        //sendMsgsThread.Start();

        //updateMsgsThread = new Thread(OnMessageReceived);
        //updateMsgsThread.Start();

        connectedPeople.text += ("You (Server)\n");
    }
    
    private void OnDisable()
    {
        finished = true;

        serverSocket.Close();
        if (receiveMsgsThread.IsAlive)
            receiveMsgsThread.Abort();

        //if (sendMsgsThread.IsAlive)
        //    sendMsgsThread.Abort();
        //if (updateMsgsThread.IsAlive)
        //    updateMsgsThread.Abort();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            finished = true;
        }

        if (!clientConnected && newMessage)
        {
            //msgReceived = true;
            OnMessageReceived();
        }
        else if (clientConnected)
        {
            OnClientConnected();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //sendingMsg = true;
            OnMessageSent();
        }
        if (messageSent)
        {
            chat.text += ("[Server]: " + input.text + "\n");
            input.text = "";
            data = new byte[1024];
            messageSent = false;
        }
    }

    void RecieveMessages()
    {
        while (!finished)
        {
            try
            {
                byte[] msg = new byte[1024];
                recv = serverSocket.ReceiveFrom(msg, SocketFlags.None, ref remote);
                if (recv > 0)
                {
                    text = Encoding.ASCII.GetString(msg, 0, recv);
                    newMessage = true;
                    data = msg;

                    if (!remoters.Contains(remote))
                    {
                        clientConnected = true;
                        lastUserName = text;
                        remoters.Add(remote);

                        for (int i = 0; i < remoters.Count; i++)
                        {
                            if (remote == remoters[i])
                                text = "Welcome to the UDP server";
                            else
                                text = lastUserName + " Connected!\n";

                            msg = Encoding.ASCII.GetBytes(text);
                            serverSocket.SendTo(msg, msg.Length, SocketFlags.None, remoters[i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error when receiving a message: " + e);
            }
        }
    }

    void OnMessageReceived()
    {
        //while (!finished)
        //{
        //    if (msgReceived)
        //    {
        //        try
        //        {
        //            data = Encoding.ASCII.GetBytes(text);
        //            recv = data.Length;

        //            for (int i = 0; i < remoters.Count; i++)
        //                serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);

        //            chat.text += (text + "\n");

        //            newMessage = false;
        //            msgReceived = false;
        //            data = new byte[1024];
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Log("Error when sending message: " + e);
        //        }
        //    }
        //}

        data = Encoding.ASCII.GetBytes(text);
        recv = data.Length;

        for (int i = 0; i < remoters.Count; i++)
            serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);

        chat.text += (text + "\n");

        newMessage = false;
        data = new byte[1024];
    }

    void OnMessageSent()
    {
        //while (!finished)
        //{
        //    if (sendingMsg)
        //    {
        //        try
        //        {
        //            data = Encoding.ASCII.GetBytes("[Server]: " + input.text);
        //            recv = data.Length;
        //            for (int i = 0; i < remoters.Count; i++)
        //            {
        //                serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);
        //            }
        //            messageSent = true;
        //            sendingMsg = false;
        //            chat.text += ("Server: " + input.text + "\n");
        //            input.text = "";
        //            data = new byte[1024];
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Log("Error when sending message: " + e);
        //        }
        //    }
        //}

        data = Encoding.ASCII.GetBytes("[Server]: " + input.text);
        recv = data.Length;
        for (int i = 0; i < remoters.Count; i++)
        {
            serverSocket.SendTo(data, recv, SocketFlags.None, remoters[i]);
        }

        messageSent = true;
        data = new byte[1024];
    }

    void OnClientConnected()
    {
        //byte[] msg;
        //string text = string.Empty;
        //for (int i = 0; i < remoters.Count; i++)
        //{
        //    if (remote == remoters[i])
        //    {
        //        text = "Welcome to the UDP server";
        //    }
        //    else
        //    {
        //        text = lastUserName + " Connected!\n";
        //    }
        //
        //    msg = Encoding.ASCII.GetBytes(text);
        //    serverSocket.SendTo(msg, msg.Length, SocketFlags.None, remoters[i]);
        //}

        chat.text += (lastUserName + " Connected!\n");
        connectedPeople.text += (lastUserName + "\n");

        clientConnected = false;
        lastUserName = string.Empty;

        newMessage = false;
        data = new byte[1024];
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
