using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

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

            _yPos -= 100;

            lobbyRows[_clientId - 1] = new LobbyRow();
            lobbyRows[_clientId - 1].lobbyRowGameObject = _lobbyRow;
            lobbyRows[_clientId - 1].username = _lobbyRow.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            lobbyRows[_clientId - 1].username.text = ClientClientSide.allClients[_clientId];
        }
    }
}
