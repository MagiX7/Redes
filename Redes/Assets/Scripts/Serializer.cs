using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

enum MessageType
{
    CHAT,
    PLAYER_DATA,
}

public static class Serializer
{
    public static byte[] SerializeString(string value)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(value);
        Debug.Log("serialized!");
        byte[] ret = stream.GetBuffer();
        int porsi = 0;
        porsi += 1;
        return ret;
    }

    public static string DeserializeString(byte[] info, int length)
    {
        MemoryStream stream = new MemoryStream(info, 0, length);

        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        Debug.Log("deserialized!");
        return reader.ReadString();
    }

    public static byte[] SerializePlayerData(PlayerData playerData)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)MessageType.PLAYER_DATA);
        writer.Write(playerData.life);
        writer.Write(playerData.damage);
        writer.Write(playerData.position.x);
        writer.Write(playerData.position.y);
        writer.Write(playerData.position.z);

        return stream.GetBuffer();
    }

    public static PlayerData DeserializePlayerData(BinaryReader reader, MemoryStream stream)
    {
        PlayerData playerData = new PlayerData();

        playerData.life = reader.ReadInt32();
        playerData.damage = reader.ReadInt32();
        playerData.position.x = reader.ReadInt32();
        playerData.position.y = reader.ReadInt32();
        playerData.position.z = reader.ReadInt32();
        Debug.Log("deserialized!");

        return playerData;
    }
}
