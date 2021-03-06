using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Net;

public class Lobby : MonoBehaviour
{
    public class LobbyRow
    {
        public GameObject lobbyRowGameObject;
        public TextMeshProUGUI username;
    }

    public GameObject lobbyParent;
    public GameObject lobbyRowPrefab;
    public LobbyRow[] lobbyRows;

    public TextMeshProUGUI publicIP;
    public TextMeshProUGUI localIP;

    public Button startButton;

    private void Awake()
    {
        startButton.enabled = false;
    }

    /// <summary>
    /// Adds a row of client info for each client
    /// </summary>
    public void InitLobbyUI()
    {
        float _yPos = 230f;
        if (lobbyRows != null)
        {
            foreach (LobbyRow _lobbyRow in lobbyRows)
            {
                Destroy(_lobbyRow.lobbyRowGameObject);
            }
        }
        lobbyRows = new LobbyRow[ClientClientSide.allClients.Count];
        foreach(int _clientId in ClientClientSide.allClients.Keys)
        {
            GameObject _lobbyRow = Instantiate(lobbyRowPrefab, lobbyParent.transform);
            _lobbyRow.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, _yPos, 0);

            _yPos -= 75;

            lobbyRows[_clientId - 1] = new LobbyRow();
            lobbyRows[_clientId - 1].lobbyRowGameObject = _lobbyRow;
            lobbyRows[_clientId - 1].username = _lobbyRow.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            lobbyRows[_clientId - 1].username.text = ClientClientSide.allClients[_clientId];
        }

        publicIP.text = GetPublicIPAddress();
        IPHostEntry ipHostEntry = Dns.GetHostEntry(string.Empty);
        localIP.text = Dns.GetHostEntry(string.Empty).AddressList[5].ToString();
    } 

    static string GetPublicIPAddress()
    {
        string address = "";
        WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
        using (WebResponse response = request.GetResponse())
        using (StreamReader stream = new StreamReader(response.GetResponseStream()))
        {
            address = stream.ReadToEnd();
        }

        int first = address.IndexOf("Address: ") + 9;
        int last = address.LastIndexOf("</body>");
        address = address.Substring(first, last - first);

        return address;
    }

    /// <summary>
    /// Starts current gamemode
    /// </summary>
    public void StartGame()
    {
        string _sceneName = SceneManager.GetActiveScene().name;
        _sceneName = _sceneName.Substring(6);
        ClientSend.StartGame(_sceneName);
    }

    /// <summary>
    /// Unloads current scenes and load main menu
    /// </summary>
    public void ExitGame()
    {
        if(InfectionEnvironmentGenerator.instance != null)
        {
            InfectionEnvironmentGenerator.buildings = new Dictionary<int, GameObject>();
            InfectionEnvironmentGenerator.lights = new Dictionary<int, GameObject>();
            InfectionEnvironmentGenerator.suns = new Dictionary<int, GameObject>();
        }
        if(FreeForAllEnvironmentGenerator.instance != null)
        {
            FreeForAllEnvironmentGenerator.planets = new Dictionary<int, GameObject>();
            FreeForAllEnvironmentGenerator.nonGravityObjectDict = new Dictionary<int, GameObject>();
        }

        ClientClientSide.allClients = new Dictionary<int, string>();
        ClientServerSide.allClients = new Dictionary<int, ClientServerSide>();
        // If player is host than close server and network manager
        if(Server.isHost)
        {
            Server.clients = new Dictionary<int, ClientServerSide>();
            Server.Stop();
            Destroy(FindObjectOfType<NetworkManager>().gameObject);
        }

        Destroy(FindObjectOfType<ClientClientSide>().gameObject);
        Destroy(FindObjectOfType<EventSystem>().gameObject);
        for(int i = 0; i < SceneManager.sceneCount; i++)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        }
        SceneManager.LoadSceneAsync("ClientMainMenu");
        //SceneManager.UnloadSceneAsync("");
    }

    public void ToggleStartButtonState()
    {
        startButton.enabled = true;
    }
}
