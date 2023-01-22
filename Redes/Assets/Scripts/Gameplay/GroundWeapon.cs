using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundWeapon : MonoBehaviour
{
    public enum weaponType
    {
        NONE = 0,
        ROCKETLAUNCHER,
        ASSAULTRIFLE
    }

    public weaponType type;

    // Update is called once per frame
    void Update()
    {
        
    }
}
