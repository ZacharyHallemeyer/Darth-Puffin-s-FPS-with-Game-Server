using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefabFFA;
    public GameObject playerPrefabInfection;

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

        Debug.Log("Server starting");
        Server.Start(50, 26950);

        int _sceneCount = SceneManager.sceneCount;
        string _sceneName = "";

        for(int i = 0; i < _sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name[0] == 'S')
                _sceneName = SceneManager.GetSceneAt(i).name.Substring(6);
        }
        Debug.Log(_sceneName);
        switch(SceneManager.GetActiveScene().name.Substring(6))
        {
            case "FreeForAll":
                Server.ChangeServerDataToFreeForAll();
                break;
            case "Infection":
                Server.ChangeServerDataToInfection();
                break;
            default:
                break;
        }
        Debug.Log("Server started");
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    // Spawns player
    public PlayerFFA InstantiatePlayerFFA()
    {
        return Instantiate(playerPrefabFFA, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<PlayerFFA>();
        /*
        return Instantiate(playerPrefab, 
                           EnvironmentGenerator.spawnPoints[Random.Range(0, EnvironmentGenerator.spawnPoints.Count)]
                           , Quaternion.identity).GetComponent<Player>();
        */
    }

    public PlayerFFA InstantiatePlayerInfection()
    {
        return Instantiate(playerPrefabInfection, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<PlayerFFA>();
        /*
        return Instantiate(playerPrefab, 
                           EnvironmentGenerator.spawnPoints[Random.Range(0, EnvironmentGenerator.spawnPoints.Count)]
                           , Quaternion.identity).GetComponent<Player>();
        */
    }
}