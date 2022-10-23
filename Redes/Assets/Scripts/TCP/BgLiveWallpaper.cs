using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgLiveWallpaper : MonoBehaviour
{
    private float initialPosX, initialPosY;
    public float velX;
    public float velY;

    private void Start()
    {
        initialPosX = this.transform.position.x;
        initialPosY = this.transform.position.y;
    }

    void Update()
    {
        if (this.transform.position.x > initialPosX + 30.0f || this.transform.position.x < initialPosX - 30.0f)
        {
            velX = -velX;
        }
        if (this.transform.position.y > initialPosY + 30.0f || this.transform.position.y < initialPosY - 30.0f)
        {
            velY = -velY;
        }
       
        this.transform.position = new Vector3(this.transform.position.x + velX * Time.deltaTime, this.transform.position.y + velY * Time.deltaTime, this.transform.position.z);
    }
}
