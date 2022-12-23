using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtocolManager : MonoBehaviour
{
    public void StartTCP()
    {
        SceneManager.LoadScene("Selection TCP");
    }

    public void StartUDP()
    {
        SceneManager.LoadScene("Selection UDP");
    }

}
