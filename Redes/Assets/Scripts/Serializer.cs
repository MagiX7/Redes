using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum MessageType
{
    NEW_USER,
    CHAT,
    PLAYER_DATA,
    SHOOT,
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

    public static byte[] SerializeStringWithHeader(MessageType header, string value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
        writer.Write(value);
        return stream.ToArray();
    }

    public static string DeserializeString(BinaryReader reader, MemoryStream stream)
    {
        return reader.ReadString();
    }

    public static byte[] SerializePlayerData(PlayerData playerData)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)MessageType.PLAYER_DATA);
        //writer.Write(playerData.life);
        writer.Write(playerData.damage);
        writer.Write(playerData.position.x);
        writer.Write(playerData.position.y);
        writer.Write(playerData.position.z);
        writer.Write(playerData.rotation.x);
        writer.Write(playerData.rotation.y);
        writer.Write(playerData.rotation.z);
        writer.Write(playerData.rotation.w);

        return stream.GetBuffer();
    }

    public static PlayerData DeserializePlayerData(BinaryReader reader, MemoryStream stream)
    {
        PlayerData playerData = new PlayerData();

        //playerData.life = reader.ReadInt32();
        playerData.damage = reader.ReadInt32();
        playerData.position.x = reader.ReadSingle();
        playerData.position.y = reader.ReadSingle();
        playerData.position.z = reader.ReadSingle();
        playerData.rotation.x = reader.ReadSingle();
        playerData.rotation.y = reader.ReadSingle();
        playerData.rotation.z = reader.ReadSingle();
        playerData.rotation.w = reader.ReadSingle();
        //Debug.Log("deserialized!");

        return playerData;
    }

    public static byte[] SerializeBoolWithHeader(MessageType header, bool value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)header);
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

    public static bool DeserializeBool(BinaryReader reader, MemoryStream stream)
    {
        return reader.ReadBoolean();
    }

}
