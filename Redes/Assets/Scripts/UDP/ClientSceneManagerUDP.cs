using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class ClientSceneManagerUDP : MonoBehaviour
{
    // UI
    [SerializeField] GameObject chat;
    GameObject serverIpInput;
    GameObject userNameInput;
    [SerializeField] GameObject chatInput;
    [SerializeField] GameObject connectButton;

    [SerializeField] ServerUDP serverUDP;
    [SerializeField] ClientUDP clientScript;

    InputField serverIpInputField;
    InputField userNameInputField;

    // Players
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;
    Vector3 initialPlayerPos = Vector3.zero;
    Vector3 initialEnemyPos = Vector3.zero;


    // Fade
    Image fadeImage;
    bool fadingIn = false;
    bool fadingInCompleted = false;
    bool fadingOut = false;


    // Utility
    float blackScreenCounter = 5.0f;
    bool gameEnded = false;
    bool startingNewGame = false;
    public bool gameStarted = false;

    // UI variables
    [SerializeField] GameObject[] UIToDeactivate;

    // Start is called before the first frame update
    void Start()
    {
        serverIpInput = GameObject.Find("Server IP Input");
        userNameInput = GameObject.Find("Username Input");

        fadeImage = GameObject.Find("Fade").GetComponent<Image>();
        if (serverIpInput != null)
        {
            serverIpInputField = serverIpInput.GetComponentInChildren<InputField>();
            userNameInputField = userNameInput.GetComponentInChildren<InputField>();
        }
        else
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        initialPlayerPos = player.transform.position;
        initialEnemyPos = enemy.transform.position;
    }

    void Update()
    {
        if (fadingIn)
        {
            Color color = fadeImage.color;
            color.a += Time.deltaTime * 3.0f;
            fadeImage.color = color;

            if (color.a > 1.01f)
            {
                fadingIn = false;
                fadingInCompleted = true;
            }
        }
        else if (fadingInCompleted)
        {
            if (blackScreenCounter > 0.0f)
            {
                blackScreenCounter -= Time.deltaTime;
            }
            else
            {
                UpdateScene();
                fadingInCompleted = false;
                fadingOut = true;
                startingNewGame = true;
            }
        }
        else if (fadingOut)
        {
            Color color = fadeImage.color;
            color.a -= Time.fixedDeltaTime * 3.0f;
            fadeImage.color = color;

            if (color.a < -0.01f)
            {
                fadingOut = false;
                fadingInCompleted = false;
                fadingIn = false;
            }
        }

        if (gameEnded && startingNewGame)
        {
            ToggleGameUI(true);
            startingNewGame = false;
            gameEnded = false;
        }

    }

    public void StartClientConnection()
    {
        fadingOut = true;
        
        ToggleGameUI(true);

        clientScript.gameObject.SetActive(true);
        clientScript.serverIp = serverIpInputField.text;
        clientScript.userName = userNameInputField.text;

        player.SetActive(true);
        player.GetComponent<PlayerMovement>().isClient = true;
        enemy.SetActive(true);
    }

    public void StartServerConnection()
    {
        fadingOut = true;
        gameStarted = true;
        for (int i = 0; i < UIToDeactivate.Length; ++i)
        {
            UIToDeactivate[i].gameObject.SetActive(false);
        }

        byte[] bytes = Serializer.SerializeBoolWithHeader(MessageType.START_GAME, true);
        serverUDP.Send(bytes);
    }

    public void EndGame()
    {
        gameEnded = true;
        fadingIn = true;
        ToggleGameUI(false);        
    }

    void ToggleGameUI(bool value)
    { 
        chat.SetActive(value);
        chatInput.SetActive(value);
        if (!gameEnded)
        {
            if (serverIpInput != null)
            {
                serverIpInput.SetActive(!value);
                userNameInput.SetActive(!value);
                connectButton.SetActive(!value);
            }
        }
    }

    void UpdateScene()
    {
        // TODO: Replace players, load meshes from new level etc

        player.SetActive(true);
        player.transform.position = initialPlayerPos;
        player.GetComponent<PlayerMovement>().ResetStats();

        enemy.SetActive(true);
        enemy.transform.position = initialEnemyPos;
        enemy.GetComponent<EnemyController>().ResetStats();
    }

}
