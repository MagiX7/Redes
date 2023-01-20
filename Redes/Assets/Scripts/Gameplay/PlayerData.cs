using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public int damage = 1;
    public int packetID = -1;
    public bool shooted = false;
    public bool isMoving = true;
    public bool isInvulnerable = false;
    public Vector3 rocketPosition = Vector3.zero;
    public Vector3 rocketDirection = Vector3.zero;

    public Vector3 position = Vector3.forward;
    public Vector3 movementDirection = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    public bool chickenGotHit = false;
    public int chickenHitLife = 5;
    public int chickenHitId = -1;
}
