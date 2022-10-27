using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
}
