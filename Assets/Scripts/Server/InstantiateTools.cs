using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateTools : MonoBehaviour
{
    public static InstantiateTools instance;

    public GameObject playerPrefab;

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
    }

    /// <summary>
    /// Spawns player
    /// </summary>
    /// <returns></returns>
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
