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
        switch(_sceneName)
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
    public SPlayer InstantiatePlayerFFA()
    {
        return Instantiate(playerPrefabFFA, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<SPlayer>();
    }

    public SPlayer InstantiatePlayerInfection()
    {
        return Instantiate(playerPrefabInfection,InfectionEnvironmentGenerator.spawnPoints[
                           Random.Range(0, InfectionEnvironmentGenerator.spawnPoints.Count)], 
                           Quaternion.identity).GetComponent<SPlayer>();
    }
}