using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgLiveWallpaper : MonoBehaviour
{
    float timeSwap = 15.0f;
    public float velX;
    public float velY;

    // Update is called once per frame
    void Update()
    {
        timeSwap -= Time.deltaTime;
        if (timeSwap <= 0.0f)
        {
            int num = Random.Range(1, 3);
            switch (num)
            {
                case 0:
                    velX = -velX;
                    velY = -velY;
                    break;
                case 1:
                    velX = -velX;
                    break;
                case 2:
                    velY = -velY;
                    break;
            }
            
            timeSwap = 10.0f;
        }
       
        this.transform.position = new Vector3(this.transform.position.x + velX * Time.deltaTime, this.transform.position.y + velY * Time.deltaTime, this.transform.position.z);
    }
}
