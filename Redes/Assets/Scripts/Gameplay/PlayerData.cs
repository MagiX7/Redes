using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransmissionData
{
    

}

public class PlayerData : TransmissionData
{
    //public int life = 5;
    public int damage = 1;
    public bool shooted = false;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    bool deliveryFailed = false;
    bool deliverySucceeded = false;

    public void HandleDeliverySuccess(DeliveryNotificationManager deliveryManager)
    {
        deliverySucceeded = true;
    }

    public void HandleDeliveryFailure(DeliveryNotificationManager deliveryManager)
    {
        deliveryFailed = true;
    }
}
