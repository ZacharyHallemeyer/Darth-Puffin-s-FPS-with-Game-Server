using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;


public class MainMenu : MonoBehaviour
{
    public Text hostUserNameFieldText, joinUserNameFieldText;
    public Text hostUserNamePlaceholderText, joinUserNamePlaceholderText;
    public Text ipFieldText, ipFieldPlaceholderText;

    public GameObject mainMenuFirstButton, gameSelectionHostFirstButton, gameSelectionJoinFirstButton;
    public GameObject optionsMenuFirstButton, statsMenuFirstButton, aboutMenuFirstButton;

    private void Start()
    {
        MoveToMainMenu();
        SetUpUserNameField();
        SetUpIPField();
    }

    #region Set Up

    public void SetUpUserNameField()
    {
        // Create random username if no username is found
        if (PlayerPrefs.GetString("Username", "NULLNULL") == "NULLNULL")
            PlayerPrefs.SetString("Username", RandomUsernameGenerator(15));
        hostUserNameFieldText.text = PlayerPrefs.GetString("Username", "NULLNULL");
        joinUserNameFieldText.text = PlayerPrefs.GetString("Username", "NULLNULL");
        hostUserNamePlaceholderText.text = PlayerPrefs.GetString("Username", "NULLNULL");
        joinUserNamePlaceholderText.text = PlayerPrefs.GetString("Username", "NULLNULL");
    }

    public void SetUpIPField()
    {
        // If no previous IP found then use local host IP
        if (PlayerPrefs.GetString("HostIP", "NULL") == "NULL")
            PlayerPrefs.SetString("HostIP", "127.0.0.1");
        ipFieldPlaceholderText.text = PlayerPrefs.GetString("HostIP");
        ipFieldText.text = PlayerPrefs.GetString("HostIP");
    }

    #endregion

    #region Navigation
    private void MoveToMainMenu()
    {
        SetCurrentEvent(null);
        SetCurrentEvent(mainMenuFirstButton);
    }

    private void MoveToGameModeSelectionHost()
    {
        SetCurrentEvent(null);
        SetCurrentEvent(gameSelectionHostFirstButton);
    }

    private void MoveToGameModeSelectionJoin()
    {
        SetCurrentEvent(null);
        SetCurrentEvent(gameSelectionJoinFirstButton);
    }

    private void MoveToOptionsMenu()
    {
        SetCurrentEvent(null);
        SetCurrentEvent(optionsMenuFirstButton);
    }

    public void MoveToStatsMenu()
    {
        SetCurrentEvent(null);
        SetCurrentEvent(statsMenuFirstButton);
    }

    public void MoveToAboutMenu()
    {
        SetCurrentEvent(null);
        SetCurrentEvent(aboutMenuFirstButton);
    }

    public void SetCurrentEvent(GameObject currentObject)
    {
        EventSystem.current.SetSelectedGameObject(currentObject);
    }

    #endregion

    #region Actions

    public void ChangeUsername(string _username)
    {
        PlayerPrefs.SetString("Username", _username);
        SetUpUserNameField();
    }

    public void ChangeHostIP(string _ip)
    {
        PlayerPrefs.SetString("HostIP", _ip);
        SetUpUserNameField();
    }

    public void ChangeServerGameMode(string _gameModeName)
    {
        AsyncOperation _asyncOperation = SceneManager.LoadSceneAsync(_gameModeName, LoadSceneMode.Additive);
        if (!_asyncOperation.isDone)
        {
            StartCoroutine(WaitAndSetActiveScene(_asyncOperation, _gameModeName));
        }
        else
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_gameModeName));
            ChangeClientScene("Client" + _gameModeName.Substring(6));
        }
    }

    public IEnumerator WaitAndSetActiveScene(AsyncOperation _asyncOperation, string _gameModeName)
    {
        yield return new WaitForEndOfFrame();
        if(_asyncOperation.isDone)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_gameModeName));
            ChangeClientScene("Client" + _gameModeName.Substring(6));
        }
        else
            StartCoroutine(WaitAndSetActiveScene(_asyncOperation, _gameModeName));
    }

    public void ChangeClientScene(string _gameModeName)
    {
        AsyncOperation _asyncOperation = SceneManager.LoadSceneAsync(_gameModeName, LoadSceneMode.Additive);

        if (!_asyncOperation.isDone)
            StartCoroutine(WaitAndUnloadScene(_asyncOperation, "ClientMainMenu"));
        else
            UnloadScene("ClientMainMenu");
    }

    private IEnumerator WaitAndUnloadScene(AsyncOperation __asyncOperation, string _sceneToUnload)
    {
        yield return new WaitForEndOfFrame();
        if (__asyncOperation.isDone)
        {
            ClientSend.StartGenerateEnvironment();
            UnloadScene(_sceneToUnload);
        }
        else
            StartCoroutine(WaitAndUnloadScene(__asyncOperation, _sceneToUnload));
    }

    public void UnloadScene(string _sceneName)
    {
        SceneManager.UnloadSceneAsync(_sceneName, UnloadSceneOptions.None);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #endregion

    #region Tools

    public string RandomUsernameGenerator(int _characterCount)
    {
        string _username = "";
        for(int i = 0; i < _characterCount; i++)
        {
            _username += (char)Random.Range(97, 123);
        }
        return _username;
    }

    #endregion
}
