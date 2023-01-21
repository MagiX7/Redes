using UnityEngine;


public class UDPManager : MonoBehaviour
{
    [SerializeField] ServerUDP server;
    [SerializeField] ClientUDP client;

    public void SendPlayerData(PlayerData playerData, int senderNetId, bool isClient)
    {
        byte[] bytes = Serializer.SerializePlayerData(playerData, senderNetId, senderNetId);

        if (isClient) client.Send(bytes);
        else server.Send(bytes);
    }

    public void SendObjectData(ObjectData objectData, int senderNetId, bool isClient)
    {
        byte[] bytes = Serializer.SerializeObjectData(objectData, senderNetId, senderNetId);

        if (isClient) client.Send(bytes);
        else server.Send(bytes);
    }

    public float GetRTTSecs(bool isClient)
    {
        if (isClient)
        {
            return client.GetRTTSecs();
        }
        return -1;
    }

}
