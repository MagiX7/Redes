using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Selection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CreateServer()
    {
        SceneManager.LoadScene("Create Game UDP");
    }

    public void EnterClient()
    {
        SceneManager.LoadScene("Join Game UDP");
    }
}
