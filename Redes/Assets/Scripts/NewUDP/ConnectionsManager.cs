using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConnectionsManager : MonoBehaviour
{
    [SerializeField] GameObject player;
    bool newUser = false;
    int lastNetId = -1;

    string chatText;
    bool newChatMessage = false;
    byte[] latestData;

    List<int> clients;

    // Start is called before the first frame update
    void Start()
    {
        latestData = new byte[1024];
        clients = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (newUser)
        {
            ClientConnected();
            newUser = false;
        }
    }

    public void OnNewClient(int netId)
    {
        newUser = true;
        lastNetId = netId;
    }

    public void OnClientDisconnected()
    {

    }

    void ClientConnected()
    {
        GameObject latestClient = Instantiate(player, Vector3.zero, Quaternion.identity);
        latestClient.GetComponent<ClientUDP>().SetNetId(lastNetId);
        latestClient.name = lastNetId.ToString();
        clients.Add(lastNetId);
    }

    void ClientDisconnected()
    {

    }


    public void OnMessageReceived(byte[] bytes)
    {
        MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length);
        BinaryReader reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);

        MessageType messageType = (MessageType)reader.ReadInt32();
        int netId = reader.ReadInt32();
        switch (messageType)
        {
            case MessageType.NEW_USER:
            {
                chatText = Serializer.DeserializeString(reader, stream);
                // Send message to the client with its net id
                break;
            }

            case MessageType.CHAT:
            {
                chatText = Serializer.DeserializeString(reader, stream);
                newChatMessage = true;
                latestData = bytes;
                break;
            }

            case MessageType.PLAYER_DATA:
            {
                foreach(int client in clients)
                {
                    if (client == netId)
                    {
                        EnemyController go = GameObject.Find(client.ToString()).GetComponent<EnemyController>();
                        go.playerData = Serializer.DeserializePlayerData(reader, stream);
                        break;
                    }
                }
                
                break;
            }

            default:
                break;
        }
    }

}
