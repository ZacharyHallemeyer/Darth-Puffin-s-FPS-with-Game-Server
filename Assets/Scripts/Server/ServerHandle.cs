using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerHandle
{
    /// <summary>
    /// Send client to lobby
    /// </summary>
    /// <param name="_fromClient"> client that just connected to server </param>
    /// <param name="_packet"> client id and client username </param>
    public static void WelcomeReceived(int _fromClient, PackerServerSide _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            return;
        }
        ClientServerSide _client = new ClientServerSide(_fromClient);
        _client.userName = _username;
        ClientServerSide.allClients.Add(_fromClient, _client);
        Server.clients[_fromClient].SendIntoLobby(_username);
    }

    /// <summary>
    /// Sends all client in lobby into game
    /// </summary>
    /// <param name="_fromClient"> client that called this method </param>
    /// <param name="_packet"> game mode name to send clients into </param>
    public static void SendLobbyIntoGame(int _fromClient, PackerServerSide _packet)
    {
        string gameModeName = _packet.ReadString();

        foreach(ClientServerSide _client in ClientServerSide.allClients.Values)
        {
            switch (gameModeName)
            {
                case "FreeForAll":
                    _client.SendIntoGameFreeForAll();
                    break;
                case "TeamDeathMatch":
                    _client.SendIntoGameFreeForAll();
                    break;
                case "CaptureTheFlag":
                    _client.SendIntoGameFreeForAll();
                    break;
                case "KingOfTheHill":
                    _client.SendIntoGameFreeForAll();
                    break;
                case "Infection":
                    _client.SendIntoGameFreeForAll();
                    break;
                case "WaveMode":
                    _client.SendIntoGameFreeForAll();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Start generate game environment
    /// </summary>
    /// <param name="_fromClient"> client that called this method </param>
    /// <param name="_packet"> NULL </param>
    public static void StartGenerateEnvironment(int _fromClient, PackerServerSide _packet)
    {
        EnvironmentGeneratorServerSide.instance.StartCoroutine(EnvironmentGeneratorServerSide.instance.GenerateEnvironment());
    }

    /// <summary>
    /// Collects input from client in regard to player movement
    /// </summary>
    /// <param name="_fromClient"> Client that is connected to new input </param>
    /// <param name="_packet"> move direction and rotation </param>
    public static void PlayerMovement(int _fromClient, PackerServerSide _packet)
    {
        Vector2 _moveDirection = _packet.ReadVector2();
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetMovementInput(_moveDirection, _rotation);
    }

    /// <summary>
    /// Tells server to have player use jetpack
    /// </summary>
    /// <param name="_fromClient"> client that wants to use jetpack </param>
    /// <param name="_packet"> direction </param>
    public static void PlayerJetPackMovement(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _direction = _packet.ReadVector3();

        Server.clients[_fromClient].player.JetPackMovement(_direction);
    }

    /// <summary>
    /// Tells server whether an animation is in progress on client side
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> isAnimaInProgress </param>
    public static void PlayerActions(int _fromClient, PackerServerSide _packet)
    {
        bool _isAnimInProgress = _packet.ReadBool();

        Server.clients[_fromClient].player.SetActionInput(_isAnimInProgress);
    }

    /// <summary>
    /// Tells server that player wants to use magnetize ability
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> NULL </param>
    public static void PlayerMagnetize(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.PlayerMagnetize();
    }

    /// <summary>
    /// Tells server that client wants to start grapple 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> direction </param>
    public static void PlayerStartGrapple(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _direction = _packet.ReadVector3();

        Server.clients[_fromClient].player.StartGrapple(_direction);
    }

    /// <summary>
    /// Tells server that client wants to continue grapple 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> position and grapple point </param>
    public static void PlayerContinueGrappling(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _grapplePoint = _packet.ReadVector3();

        ServerSend.OtherPlayerContinueGrapple(_fromClient, _position, _grapplePoint);
    }

    /// <summary>
    /// Tells server that client wants to stop grapple 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> NULL </param>
    public static void PlayerStopGrapple(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.StopGrapple();
    }

    /// <summary>
    /// Tells server that client wants to start shooting 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> fire point and fire direction </param>
    public static void PlayerStartShoot(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _firePoint = _packet.ReadVector3();
        Vector3 _fireDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.ShootController(_firePoint, _fireDirection);
    }

    /// <summary>
    /// Tells server the updated fire point and fire direction
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> fire point nad fire direction </param>
    public static void PlayerUpdateShootDirection(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _firePoint = _packet.ReadVector3();
        Vector3 _fireDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.UpdateShootDirection(_firePoint, _fireDirection);
    }

    /// <summary>
    /// Tells server that client wants to stop shooting 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> NULL </param>
    public static void PlayerStopShoot(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.StopShootContoller();
    }

    /// <summary>
    /// Tells server that client wants to reload 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> NULL </param>
    public static void PlayerReload(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.Reload();
    }

    /// <summary>
    /// Tells server that client wants to switch wepaon 
    /// </summary>
    /// <param name="_fromClient"> client that called this funtion</param>
    /// <param name="_packet"> NULL </param>
    public static void PlayerSwitchWeapon(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.SwitchWeapon();
    }
}