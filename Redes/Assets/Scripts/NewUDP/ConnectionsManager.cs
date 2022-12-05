using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionsManager : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    bool newUser = false;
    int lastNetId = -1;
    int latestAffectedNetId = -1;
    bool needToUpdateEnemy = false;
    BinaryReader latestReader;

    int latestSenderNetId = -1;
    bool clientDisconnected = false;

    [SerializeField] ClientSceneManagerUDP sceneManager;

    public List<int> clientNetIds;
    public List<GameObject> players;
    bool needToInstantiateServer = false;
    bool serverInstanced = false;

    // Start is called before the first frame update
    void Start()
    {
        clientNetIds = new List<int>();
        players = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (newUser)
        {
            ClientConnected();
            newUser = false;
        }

        if (clientDisconnected)
        {
            OnClientDisconnected();
            clientDisconnected = false;
        }

        if (needToInstantiateServer)
        {
            GameObject latestClient = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            int serverId = 0;
            latestClient.name = serverId.ToString();
            clientNetIds.Add(serverId);
            players.Add(latestClient);
            needToInstantiateServer = false;
            serverInstanced = true;
        }

        if (needToUpdateEnemy)
        {
            foreach (int clientId in clientNetIds)
            {
                if (clientId == latestAffectedNetId)
                {
                    //latestAffectedNetId = clientId;
                    EnemyController go = GameObject.Find(clientId.ToString()).GetComponent<EnemyController>();
                    go.playerData = Serializer.DeserializePlayerData(latestReader);
                    break;
                }
            }
        }
    }

    public void OnNewClient(int netId)
    {
        lastNetId = netId;
        newUser = true;
    }

    void ClientConnected()
    {
        GameObject latestClient = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        latestClient.name = lastNetId.ToString();
        clientNetIds.Add(lastNetId);
        players.Add(latestClient);
    }

    public void OnClientDisconnected()
    {
        clientNetIds.Remove(latestSenderNetId);
        GameObject go = GameObject.Find(latestSenderNetId.ToString());
        players.Remove(go);
        Destroy(go);
    }

    void ClientDisconnected()
    {

    }


    public MessageType OnMessageReceived(byte[] bytes, out string chatText, out int clientNetId)
    {
        MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length);
        BinaryReader reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);

        MessageType messageType = (MessageType)reader.ReadInt32();
        int senderNetId = reader.ReadInt32();
        int affectedNetId = reader.ReadInt32();
        switch (messageType)
        {
            case MessageType.NEW_USER:
            {
                chatText = Serializer.DeserializeString(reader);
                sceneManager.OnNewChatMessage(chatText);
                clientNetId = -1;
                break;
            }

            case MessageType.DISCONNECT:
            {
                latestSenderNetId = senderNetId;
                clientDisconnected = true;
                chatText = Serializer.DeserializeString(reader);
                sceneManager.OnNewChatMessage(chatText);
                clientNetId = -1;
                break;
            }

            case MessageType.NET_ID:
            {
                clientNetId = Serializer.DeserializeInt(reader);
                chatText = string.Empty;
                break;
            }

            case MessageType.CHAT:
            {
                chatText = Serializer.DeserializeString(reader);
                sceneManager.OnNewChatMessage(chatText);
                clientNetId = -1;
                break;
            }

            case MessageType.PLAYER_DATA:
            {
                if (affectedNetId > 0)
                {
                    latestAffectedNetId = affectedNetId;
                    needToUpdateEnemy = true;
                    latestReader = reader;
                }
                // Server instancing
                if (!serverInstanced && affectedNetId == 0)
                {
                    needToInstantiateServer = true;
                }
                chatText = string.Empty;
                clientNetId = -1;
                break;
            }

            case MessageType.START_GAME:
            {
                sceneManager.StartClient();
                chatText = string.Empty;
                clientNetId = -1;
                break;
            }

            default:
            {
                chatText = string.Empty;
                clientNetId = -1;
                break;
            }
        }

        return messageType;
    }
}
