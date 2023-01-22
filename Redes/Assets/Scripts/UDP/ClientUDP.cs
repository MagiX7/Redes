using System.Net;
using System.Net.Sockets;

using System.Threading;
using UnityEngine;


public class ClientUDP : MonoBehaviour
{
    Socket clientSocket;
    IPEndPoint clientIpep;

    int recv = 0;
    byte[] data;
    [HideInInspector] public string clientIp;
    [HideInInspector] public string serverIp;
    [HideInInspector] public string userName;
    
    EndPoint remote = null;
    int netId = -1;
    bool netIdAssigned = false;

    Thread receiveMsgsThread;

    bool finished = false;
    bool newUser = false; // For other players
    int latestNetId = -1;

    [SerializeField] ClientSceneManagerUDP sceneManager;
    ConnectionsManager connectionsManager;
    
    void Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        clientIpep = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), 5345);
        //clientSocket.Bind(clientIpep);

        remote = new IPEndPoint(IPAddress.Parse(serverIp), 5345);

        data = new byte[1024];
        data = Serializer.SerializeStringWithHeader(MessageType.NEW_USER, netId, userName);
        clientSocket.SendTo(data, data.Length, SocketFlags.None, remote);
        data = new byte[1024];

        receiveMsgsThread = new Thread(ReceiveMessages);
        receiveMsgsThread.Start();

        connectionsManager = GameObject.Find("Connections Manager").GetComponent<ConnectionsManager>();
    }

    private void OnDestroy()
    {
        // Notify Disconnection
        byte[] bytes = Serializer.SerializeStringWithHeader(MessageType.DISCONNECT, netId, userName);
        clientSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
        finished = true;

        clientSocket.Close();
        if (receiveMsgsThread.IsAlive)
            receiveMsgsThread.Abort();
    }

    void Update()
    {
        if (newUser)
        {
            connectionsManager.OnNewClient(latestNetId);
            newUser = false;
        }
        if (netIdAssigned)
        {
            transform.parent.name = netId.ToString();
            transform.parent.position = connectionsManager.positions[netId];
            netIdAssigned = false;
        }
    }

    void ReceiveMessages()
    {
        while (!finished)
        {
            byte[] bytes = new byte[1024];
       
            recv = clientSocket.ReceiveFrom(bytes, SocketFlags.None, ref remote);

            if (recv > 0)
            {
                int incomingNetId;
                int affectedNetId;
                int senderNetId;
                MessageType msgType = connectionsManager.OnMessageReceived(bytes, out _, out incomingNetId, out senderNetId, out affectedNetId);

                if (msgType == MessageType.NET_ID && incomingNetId > 0)
                {
                    netId = incomingNetId;
                    connectionsManager.OnNewClient(senderNetId);
                    netIdAssigned = true;
                }
                else if (msgType == MessageType.NEW_USER && senderNetId != netId)
                {
                    newUser = true;
                    latestNetId = senderNetId;
                    Debug.Log("Entered new user client udp");
                }
            }
        }
    }

    public void Send(byte[] bytes)
    {
        clientSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remote);
    }

    public void SetNetId(int value)
    {
        netId = value;
    }

    public int GetNetId() { return netId; }

    public string GetUserName() { return userName; }

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
