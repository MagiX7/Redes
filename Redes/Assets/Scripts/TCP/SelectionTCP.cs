using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionTCP : MonoBehaviour
{
    public void CreateServer()
    {
        SceneManager.LoadScene("Create Game TCP");
    }

    public void EnterClient()
    {
        SceneManager.LoadScene("Join Game TCP");
    }
}
