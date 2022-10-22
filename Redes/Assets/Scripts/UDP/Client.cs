using System;
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
    Socket clientSocket;
    IPEndPoint clientIpep;

    int recv = 0; // Size of the data
    byte[] data;
    public string clientIp;
    public string serverIp;
    [HideInInspector] public string userName;
    
    EndPoint remote = null;
    IPEndPoint sender;

    Thread netThread;
    //Thread sendMsgsThread;
    bool finished = false;
    bool newMessage = false;
    //bool messageSend = false;

    string text;

    [SerializeField] Text chat;
    [SerializeField] InputField input;

    // Start is called before the first frame update
    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        clientSocket.Bind(clientIpep);

        sender = new IPEndPoint(IPAddress.Parse(serverIp), 5345);
        remote = (EndPoint)(sender);

        data = new byte[1024];
        data = Encoding.ASCII.GetBytes(userName);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, remote);
        data = new byte[1024];

        netThread = new Thread(RecieveMessages);
        netThread.Start();

        //sendMsgsThread = new Thread(OnMessageSent);
        //sendMsgsThread.Start();
    }

    private void OnDisable()
    {
        finished = true;

        clientSocket.Close();
        if (netThread.IsAlive)
            netThread.Abort();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            finished = true;
        }

        if (newMessage)
        {
            chat.text += (text + "\n");
            newMessage = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //messageSend = true;
            OnMessageSent();
        }
    }

    void RecieveMessages()
    {
        while (!finished)
        {
            try
            {
                byte[] msg = new byte[1024];
                recv = clientSocket.ReceiveFrom(msg, SocketFlags.None, ref remote);
                text = Encoding.ASCII.GetString(msg, 0, recv);
                newMessage = true;
                data = msg;
            }
            catch (Exception e)
            {
                Debug.Log("Error when sending the message: " + e);
            }
        }
    }

    void OnMessageSent()
    {
        //while (!finished)
        //{
        //    if (messageSend)
        //    {
        //        try
        //        {
        //            string msg = "[" + userName + "]" + ": " + input.text;
        //            data = Encoding.ASCII.GetBytes(msg);
        //            recv = data.Length;
        //            clientSocket.SendTo(data, recv, SocketFlags.None, remote);
        //            input.text = "";
        //            messageSend = false;
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Log("Error when sending the message: " + e);
        //        }

        //        messageSend = false;
        //    }
        //}


        string msg = "[" + userName + "]" + ": " + input.text;
        data = Encoding.ASCII.GetBytes(msg);
        recv = data.Length;
        clientSocket.SendTo(data, recv, SocketFlags.None, remote);
        input.text = "";
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
