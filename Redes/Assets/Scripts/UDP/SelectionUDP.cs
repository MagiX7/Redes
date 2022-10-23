using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionUDP : MonoBehaviour
{
    public void CreateServer()
    {
        SceneManager.LoadScene("Create Game UDP");
    }

    public void EnterClient()
    {
        SceneManager.LoadScene("Join Game UDP");
    }
}
