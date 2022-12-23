using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionsManager : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [HideInInspector] public bool newUser = false;
    int lastNetId = -1;
    int latestAffectedNetId = -1;
    [HideInInspector] public bool needToUpdateEnemy = false;
    PlayerData latestPlayerData;

    ObjectData latestObjectData;
    bool needToUpdateObject = false;
    int  latestObjectId = -1;
    public List<GameObject> destroyableObjects = new List<GameObject>();

    int latestSenderNetId = -1;
    bool clientDisconnected = false;
    string disconnectedUserName = string.Empty;

    [SerializeField] ClientSceneManagerUDP sceneManager;

    public List<int> clientNetIds;
    public List<GameObject> players;
    bool needToInstantiateServer = false;
    bool serverInstanced = false;

    Mutex mutex = new Mutex();

    // Start is called before the first frame update
    void Start()
    {
        clientNetIds = new List<int>();
        players = new List<GameObject>();
    }

    private void OnDisable()
    {
        mutex.Dispose();
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

        if (needToUpdateEnemy)
        {
            mutex.WaitOne();
            {
                foreach (int clientId in clientNetIds)
                {
                    if (clientId == latestAffectedNetId)
                    {
                        //latestAffectedNetId = clientId;
                        EnemyController go = GameObject.Find(clientId.ToString()).GetComponent<EnemyController>();
                        go.playerData = latestPlayerData;
                        break;
                    }
                }
            }
            mutex.ReleaseMutex();
        }

        if (needToUpdateObject)
        {
            mutex.WaitOne();
            {
                // Update the objects that are moving
                for (int i = 0; i < destroyableObjects.Count; ++i)
                {
                    ObjectDestructor objectDestructor = destroyableObjects[i].GetComponent<ObjectDestructor>();
                    if (objectDestructor.objectID == latestObjectId)
                    {
                        objectDestructor.objectData = latestObjectData;
                        objectDestructor.ApplyImpulseForce();
                        break;
                    }
                }
                needToUpdateObject = false;
            }
            mutex.ReleaseMutex();
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
        sceneManager.RemoveUserFromList(disconnectedUserName);
        players.Remove(go);
        Destroy(go);
        disconnectedUserName = string.Empty;
    }

    public MessageType OnMessageReceived(byte[] bytes, out string chatText, out int clientNetId, out int senderNetId, out int affectedNetId)
    {
        MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length);
        BinaryReader reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);

        MessageType messageType = (MessageType)reader.ReadInt32();
        senderNetId = reader.ReadInt32();
        affectedNetId = reader.ReadInt32();
        switch (messageType)
        {
            case MessageType.NEW_USER:
                {
                    chatText = Serializer.DeserializeString(reader);
                    sceneManager.OnNewChatMessage(chatText);
                    clientNetId = -1;
                    // On the server this is done when a remoters is not in the list
                    //OnNewClient(senderNetId);
                    break;
                }

            case MessageType.DISCONNECT:
                {
                    latestSenderNetId = senderNetId;
                    clientDisconnected = true;
                    //chatText = Serializer.DeserializeString(reader);
                    disconnectedUserName = Serializer.DeserializeString(reader);
                    chatText = "[" + disconnectedUserName + "]: Disconnected";
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
                    //if (affectedNetId == 0)
                    //{
                    //    Debug.Log(affectedNetId);
                    //    latestAffectedNetId = affectedNetId;
                    //    needToUpdateEnemy = true;
                    //    latestPlayerData = Serializer.DeserializePlayerData(reader);
                    //}
                    //else if (affectedNetId == 1)
                    //{
                    //    Debug.Log(affectedNetId);
                    //    latestAffectedNetId = affectedNetId;
                    //    needToUpdateEnemy = true;
                    //    latestPlayerData = Serializer.DeserializePlayerData(reader);
                    //}
                    //else if (affectedNetId == 2)
                    //{
                    //    Debug.Log(affectedNetId);
                    //    latestAffectedNetId = affectedNetId;
                    //    needToUpdateEnemy = true;
                    //    latestPlayerData = Serializer.DeserializePlayerData(reader);
                    //}
                    //// Server instancing
                    //if (!serverInstanced && affectedNetId == 0)
                    //{
                    //    needToInstantiateServer = true;
                    //}

                    latestAffectedNetId = affectedNetId;
                    needToUpdateEnemy = true;
                    latestPlayerData = Serializer.DeserializePlayerData(reader);

                    chatText = string.Empty;
                    clientNetId = -1;
                    break;
                }

            case MessageType.OBJECT_DATA:
                {
                    if (affectedNetId >= 0)
                    {
                        latestObjectId = affectedNetId;
                        latestObjectData = Serializer.DeserializeObjectData(reader);
                        needToUpdateObject = true;
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
