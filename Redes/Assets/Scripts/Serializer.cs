using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum MessageType
{
    NEW_USER,
    CHAT,
    PLAYER_DATA,
    SHOOT,
    NEW_PLAYER,
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

    // NEW ///////////////////////////////////////////////////////////

    public static byte[] SerializePlayerList(List<EndPoint> playerList)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)MessageType.NEW_PLAYER);
        writer.Write(playerList.Count);
        foreach (EndPoint player in playerList)
        {
            writer.Write(player.ToString());
        }

        return stream.GetBuffer();
    }

    public static List<string> DeserializePlayerList(BinaryReader reader, MemoryStream stream)
    {
        List<string> playerList = new List<string>();
        string player = "null";

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            player = reader.ReadString();
            player = player.Substring(0, player.LastIndexOf(":"));
            playerList.Add(player);
        }

        return playerList;
    }

    public static byte[] NewSerializePlayerData(PlayerData playerData, string ip)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write((int)MessageType.PLAYER_DATA);
        writer.Write(playerData.damage);
        writer.Write(playerData.position.x);
        writer.Write(playerData.position.y);
        writer.Write(playerData.position.z);
        writer.Write(playerData.rotation.x);
        writer.Write(playerData.rotation.y);
        writer.Write(playerData.rotation.z);
        writer.Write(playerData.rotation.w);
        writer.Write(ip.Length);
        writer.Write(ip);

        return stream.GetBuffer();
    }

    public static PlayerData NewDeserializePlayerData(BinaryReader reader, MemoryStream stream, ref string enemyIp)
    {
        PlayerData playerData = new PlayerData();

        playerData.damage = reader.ReadInt32();
        playerData.position.x = reader.ReadSingle();
        playerData.position.y = reader.ReadSingle();
        playerData.position.z = reader.ReadSingle();
        playerData.rotation.x = reader.ReadSingle();
        playerData.rotation.y = reader.ReadSingle();
        playerData.rotation.z = reader.ReadSingle();
        playerData.rotation.w = reader.ReadSingle();
        int size = reader.ReadInt32();
        size += 1;
        byte[] bytes = new byte[size];
        bytes = reader.ReadBytes(size);
        enemyIp = ASCIIEncoding.ASCII.GetString(bytes);
        enemyIp = enemyIp.Substring(1);

        return playerData;
    }
}
