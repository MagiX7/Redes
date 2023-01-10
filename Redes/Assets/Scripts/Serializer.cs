using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum MessageType
{
    CHAT = 1,
    NEW_USER,
    PLAYER_DATA,
    NET_ID,
    START_GAME,
    OBJECT_DATA,
    DISCONNECT
}

public static class Serializer
{
    public static byte[] SerializeString(string value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(value);
        return stream.ToArray();
    }

    public static byte[] SerializeStringWithHeader(MessageType header, int senderNetId, string value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(senderNetId);
        writer.Write(-1);
        writer.Write(value);
        return stream.ToArray();
    }

    public static string DeserializeString(BinaryReader reader)
    {
        return reader.ReadString();
    }

    public static byte[] SerializeIntWithHeader(MessageType header, int senderNetId, int value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(senderNetId);
        writer.Write(-1);
        writer.Write(value);
        return stream.ToArray();
    }

    public static int DeserializeInt(BinaryReader reader)
    {
        return reader.ReadInt32();
    }

    public static byte[] SerializePlayerData(PlayerData playerData, int senderNetId, int affectedNetId)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)MessageType.PLAYER_DATA);
        writer.Write(senderNetId);
        writer.Write(affectedNetId);
        writer.Write(playerData.damage);
        writer.Write(playerData.packetID);
        writer.Write(playerData.isMoving);
        writer.Write(playerData.position.x);
        writer.Write(playerData.position.z);
        writer.Write(playerData.movementDirection.x);
        writer.Write(playerData.movementDirection.y);
        Vector3 rot = playerData.rotation.eulerAngles;
        writer.Write(rot.x);
        writer.Write(rot.y);
        writer.Write(rot.z);

        writer.Write(playerData.shooted);

        if (playerData.shooted)
        {
            writer.Write(playerData.rocketPosition.x);
            writer.Write(playerData.rocketPosition.y);
            writer.Write(playerData.rocketPosition.z);
            writer.Write(playerData.rocketDirection.x);
            writer.Write(playerData.rocketDirection.y);
            writer.Write(playerData.rocketDirection.z);
        }

        return stream.GetBuffer();
    }

    public static PlayerData DeserializePlayerData(BinaryReader reader)
    {
        PlayerData playerData = new PlayerData();

        playerData.damage = reader.ReadInt32();
        playerData.packetID = reader.ReadInt32();
        playerData.isMoving = reader.ReadBoolean();
        playerData.position.x = reader.ReadSingle();
        playerData.position.z = reader.ReadSingle();
        playerData.movementDirection.x = reader.ReadSingle();
        playerData.movementDirection.y = reader.ReadSingle();
        Vector3 euler = new Vector3();
        euler.x = reader.ReadSingle();
        euler.y = reader.ReadSingle();
        euler.z = reader.ReadSingle();
        playerData.rotation = Quaternion.Euler(euler);

        playerData.shooted = reader.ReadBoolean();

        if (playerData.shooted)
        {
            playerData.rocketPosition.x = reader.ReadSingle();  
            playerData.rocketPosition.y = reader.ReadSingle();
            playerData.rocketPosition.z = reader.ReadSingle();
            playerData.rocketDirection.x = reader.ReadSingle();
            playerData.rocketDirection.y = reader.ReadSingle();
            playerData.rocketDirection.z = reader.ReadSingle();
        }

        return playerData;
    }

    public static byte[] SerializeBoolWithHeader(MessageType header, int senderNetId, bool value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(senderNetId);
        writer.Write(-1);
        writer.Write(value);
        return stream.ToArray();
    }

    public static byte[] SerializeBool(bool value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(value);
        return stream.ToArray();
    }

    public static bool DeserializeBool(BinaryReader reader)
    {
        return reader.ReadBoolean();
    }


    public static byte[] SerializeObjectData(ObjectData ObjectData, int senderNetId, int affectedNetId)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)MessageType.OBJECT_DATA);
        writer.Write(senderNetId);
        writer.Write(affectedNetId);

        writer.Write(ObjectData.position.x);
        writer.Write(ObjectData.position.y);
        writer.Write(ObjectData.position.z);
        Vector3 rot = ObjectData.rotation.eulerAngles;
        writer.Write(rot.x);
        writer.Write(rot.y);
        writer.Write(rot.z);

        writer.Write(ObjectData.impulse.x);
        writer.Write(ObjectData.impulse.y);
        writer.Write(ObjectData.impulse.z);

        return stream.GetBuffer();
    }
    public static ObjectData DeserializeObjectData(BinaryReader reader)
    {
        ObjectData objectData = new ObjectData();

        objectData.position.x = reader.ReadSingle();
        objectData.position.y = reader.ReadSingle();
        objectData.position.z = reader.ReadSingle();
        
        Vector3 euler = new Vector3();
        euler.x = reader.ReadSingle();
        euler.y = reader.ReadSingle();
        euler.z = reader.ReadSingle();
        objectData.rotation = Quaternion.Euler(euler);

        objectData.impulse.x = reader.ReadSingle();
        objectData.impulse.y = reader.ReadSingle();
        objectData.impulse.z = reader.ReadSingle();

        return objectData;
    }
}
