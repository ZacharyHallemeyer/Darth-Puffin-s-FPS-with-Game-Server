using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

    public static string currentGameMode;
        
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }

    // Starts server
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(50, 26950);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    // Spawns player
    public PlayerServerSide InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<PlayerServerSide>();
        /*
        return Instantiate(playerPrefab, 
                           EnvironmentGenerator.spawnPoints[Random.Range(0, EnvironmentGenerator.spawnPoints.Count)]
                           , Quaternion.identity).GetComponent<Player>();
        */
    }
}