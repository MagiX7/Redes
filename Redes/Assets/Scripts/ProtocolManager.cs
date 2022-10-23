using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtocolManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTCP()
    {
        SceneManager.LoadScene("Selection TCP");
    }

    public void StartUDP()
    {
        SceneManager.LoadScene("Selection UDP");
    }

}
