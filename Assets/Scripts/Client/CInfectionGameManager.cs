using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CInfectionGameManager : MonoBehaviour
{
    public static CInfectionGameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, PlayerActionInfection> playersActionsInfection = new Dictionary<int, PlayerActionInfection>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject groundPrefab;
    public GameObject wallPrefab;
    public GameObject roofPrefab;
    public GameObject buildingPrefab;
    public GameObject sunPrefab;
    public GameObject directionalLight;

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

    private void Start()
    {
        if(SceneManager.GetActiveScene().name[0] == 'S')
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("ClientInfection"));
        }
    }

    /// <summary>
    /// Spawns new player to client server and inits player
    /// </summary>
    /// <param name="_id"> client ID </param>
    /// <param name="_username"> client username </param>
    /// <param name="_position"> player spawn position </param>
    /// <param name="_rotation"> player spawn rotation </param>
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation,
                            string _gunName, int _currentAmmo, int _reserveAmmo)
    {
        GameObject _player;
        if (_id != ClientClientSide.instance.myId)
        {
            // Not Local Player
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.GetComponent<PlayerManager>().Initialize(_id, _username);
            players.Add(_id, _player.GetComponent<PlayerManager>());
            _player.GetComponent<PlayerActionInfection>().Initialize(_id, _gunName, _currentAmmo, _reserveAmmo);
            playersActionsInfection.Add(_id, _player.GetComponent<PlayerActionInfection>());
            playersActionsInfection[_id].PlayerInitGun(_gunName, _currentAmmo, _reserveAmmo);
        }
        else
        {
            // Local Player
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _player.GetComponent<PlayerActionInfection>().Initialize(_id, _gunName, _currentAmmo, _reserveAmmo);
            playersActionsInfection.Add(_id, _player.GetComponent<PlayerActionInfection>());
            _player.GetComponent<PlayerManager>().Initialize(_id, _username);
            players.Add(_id, _player.GetComponent<PlayerManager>());
        }

        PlayerUI _playerUI = FindObjectOfType<PlayerUI>();
        if (_playerUI != null)
            _playerUI.InitScoreBoard();
        Debug.Log("Player added");
    }

    public void SpawnBuilding(Vector3 _position, Vector3 _localScale, int _key)
    {
        GameObject building;
        if (_key == 0)
            building = Instantiate(groundPrefab, _position, Quaternion.identity);
        else if(_key >= 1 && _key <= 4)
            building = Instantiate(wallPrefab, _position, Quaternion.identity);
        else if (_key == 5)
            building = Instantiate(groundPrefab, _position, Quaternion.identity);
        else 
            building = Instantiate(buildingPrefab, _position, Quaternion.identity);
        building.transform.localScale = _localScale;
    }

    public void Spawn(Vector3 _position, Vector3 _localScale)
    {
        GameObject building = Instantiate(buildingPrefab, _position, Quaternion.identity);
        building.transform.localScale = _localScale;
    }

    public void SpawnSun(Vector3 _position, Vector3 _localScale)
    {
        GameObject sun = Instantiate(sunPrefab, _position, Quaternion.identity);
        sun.transform.localScale = _localScale;
    }
}
