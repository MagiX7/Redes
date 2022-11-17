using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


// Determine the number of bits on how many packets we want to send
using PacketSequenceNumber = System.UInt16;


class InFlightPacket
{
    public PacketSequenceNumber sequenceNumber;
    public float dispatchedTime;

    Dictionary<int, PlayerData> dataDictionary;

    public InFlightPacket(PacketSequenceNumber sequenceNumber)
    {
        this.sequenceNumber = sequenceNumber;
        this.dispatchedTime = Time.time;
        dataDictionary = new Dictionary<int, PlayerData>();
    }

    public void HandleDeliveryFailure(DeliveryNotificationManager deliveryManager)
    {
        foreach (KeyValuePair<int, PlayerData> pair in dataDictionary)
        {
            pair.Value.HandleDeliveryFailure(deliveryManager);
        }
    }

    public void HandleDeliverySuccess(DeliveryNotificationManager deliveryManager)
    {
        foreach (KeyValuePair<int, PlayerData> pair in dataDictionary)
        {
            pair.Value.HandleDeliverySuccess(deliveryManager);
        }
    }

    public void SetData(int key, PlayerData data)
    {
        dataDictionary[key] = data;
    }

    public PlayerData GetData(int key)
    {
        return dataDictionary.ContainsKey(key) ? dataDictionary[key] : null;
    }

}

class AckRange
{
    public int start = 0;
    public uint count = 1;

    public void Write(ref BinaryWriter packet)
    {
        packet.Write(start);
        bool hasCount = count > 1;
        packet.Write(hasCount);
        if (hasCount)
        {
            // Assume we want to Ack a max of 8 bits. Since in C# there is not uint8_t, we use byte (equivalent)
            // It could be less, because sending 256 acks would never be needed
            UInt32 countMinusOne = count - 1;
            byte countToAck = (byte)(countMinusOne > 255 ? 255 : (byte)countMinusOne);
            packet.Write(countToAck);
        }
    }

    public void Read(ref BinaryReader packet)
    {
        start = packet.ReadInt32();
        bool hasCount = packet.ReadBoolean();
        if (hasCount)
        {
            byte countMinusOne = packet.ReadByte();
            count = (uint)countMinusOne + 1;
        }
        else
        {
            count = 1;
        }
    }

    public bool ExtendIfShould(PacketSequenceNumber inSequenceNumber)
    {
        if (inSequenceNumber == start + count)
        {
            count++;
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class DeliveryNotificationManager : MonoBehaviour
{
    // The type of the var is according how many packets are we sending
    // The objective is to send as fewer bits as possible
    PacketSequenceNumber nextOutgoingSequenceNumber = 0;
    PacketSequenceNumber nextExpectedSequenceNumber = 0;
    
    int dispatchedPacketCount = 0;
    int droppedPacketCount = 0;
    int deliveredPacketCount = 0;

    List<InFlightPacket> inFlightPackets = new List<InFlightPacket>();
    List<AckRange> pendingAcks = new List<AckRange>();

    UInt64 kAckTimeout = 500;

    public PacketSequenceNumber WriteSequenceNumber(ref BinaryWriter writer)
    {
        writer.Write(nextOutgoingSequenceNumber++);
        ++dispatchedPacketCount;

        InFlightPacket packet = new InFlightPacket(nextOutgoingSequenceNumber);
        inFlightPackets.Add(packet);
        return nextOutgoingSequenceNumber;
    }


    public bool ProcessSequenceNumber(ref BinaryReader reader)
    {
        PacketSequenceNumber sequenceNumber = reader.ReadUInt16();

        if (sequenceNumber >= nextExpectedSequenceNumber)
        {
            nextExpectedSequenceNumber += ++sequenceNumber;
            AddPendingAck(sequenceNumber);
            return true;
        }
        else if (sequenceNumber < nextExpectedSequenceNumber)
        {
            return false;
        }

        return false;
    }

    void AddPendingAck(PacketSequenceNumber sequenceNumber)
    {
        if (pendingAcks.Count == 0 || pendingAcks[pendingAcks.Count].ExtendIfShould(sequenceNumber))
        {
            AckRange ack = new AckRange();
            ack.start = sequenceNumber;
            pendingAcks.Add(ack);
        }
    }

    void WritePendingAcks(ref BinaryWriter packet)
    {
        bool hasAcks = pendingAcks.Count > 0;
        packet.Write(hasAcks);
        if (hasAcks)
        {
            pendingAcks[0].Write(ref packet);
            pendingAcks.RemoveAt(0);
        }
    }

    void ProcessAcks(ref BinaryReader packet)
    {
        bool hasAcks = packet.ReadBoolean();
        if (hasAcks)
        {
            AckRange ackRange = new AckRange();
            ackRange.Read(ref packet);

            PacketSequenceNumber nextAckdSequenceNumber = (PacketSequenceNumber)ackRange.start;
            UInt32 onePastAckdSequenceNumber = nextAckdSequenceNumber + ackRange.count;

            while (nextAckdSequenceNumber < onePastAckdSequenceNumber && inFlightPackets.Count <= 0)
            {
                InFlightPacket nextInFlightPacket = inFlightPackets[0];
                PacketSequenceNumber nextInFlightPacketSequenceNumber = nextInFlightPacket.sequenceNumber;

                // No ack. Prob not delivered / got lost
                if (nextInFlightPacketSequenceNumber < nextAckdSequenceNumber)
                {
                    InFlightPacket copyOfInFlightPacket = nextInFlightPacket;
                    inFlightPackets.RemoveAt(0);
                    HandlePacketDeliveryFailure(copyOfInFlightPacket);
                }
                else if (nextInFlightPacketSequenceNumber == nextAckdSequenceNumber)
                {
                    HandlePacketDeliverySuccess(nextInFlightPacket);
                    inFlightPackets.RemoveAt(0);
                    ++nextAckdSequenceNumber;
                }
                else if (nextInFlightPacketSequenceNumber > nextAckdSequenceNumber)
                {
                    nextAckdSequenceNumber = nextInFlightPacketSequenceNumber;
                }
            }
        }
    }

    void ProcessTimedOutPackets()
    {
        UInt64 timeoutTime = (UInt64)Time.time * 1000 - kAckTimeout;

        while (inFlightPackets.Count != 0)
        {
            InFlightPacket nextInFlightPacket = inFlightPackets[0];

            if (nextInFlightPacket.dispatchedTime < timeoutTime)
            {
                HandlePacketDeliveryFailure(nextInFlightPacket);
                inFlightPackets.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }

    void HandlePacketDeliveryFailure(InFlightPacket packet)
    {
        ++droppedPacketCount;
        packet.HandleDeliveryFailure(this);
    }

    void HandlePacketDeliverySuccess(InFlightPacket packet)
    {
        ++deliveredPacketCount;
        packet.HandleDeliverySuccess(this);
    }

}
