using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionsManager : MonoBehaviour
{
    [SerializeField] GameObject player;
    bool newUser = false;
    int lastNetId = -1;

    string chatText;
    bool newChatMessage = false;
    byte[] latestData;

    [SerializeField] ClientSceneManagerUDP sceneManager;

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

    void ClientConnected()
    {
        GameObject latestClient = Instantiate(player, Vector3.zero, Quaternion.identity);
        latestClient.GetComponent<ClientUDP>().SetNetId(lastNetId);
        latestClient.name = lastNetId.ToString();
        clients.Add(lastNetId);
    }

    public void OnClientDisconnected()
    {

    }

    void ClientDisconnected()
    {

    }


    public void OnMessageReceived(byte[] bytes, out string chatText)
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
                sceneManager.OnNewChatMessage(chatText);
                // Send message to the client with its net id
                break;
            }

            case MessageType.CHAT:
            {
                chatText = Serializer.DeserializeString(reader, stream);
                sceneManager.OnNewChatMessage(chatText);
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
                chatText = string.Empty;
                break;
            }

            default:
            {
                chatText = string.Empty;
                break;
            }
        }

    }




}
