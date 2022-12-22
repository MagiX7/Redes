using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public int damage = 1;
    public bool shooted = false;
    public int netIDComprovant = -1;
    public Vector3 position = Vector3.forward;
    public Vector3 movementDirection = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
}
