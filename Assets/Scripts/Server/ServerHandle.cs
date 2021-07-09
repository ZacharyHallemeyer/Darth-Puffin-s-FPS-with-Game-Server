using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, PackerServerSide _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
        //Server.clients[_fromClient].SendIntoLobby(_username);
    }

    public static void SendLobbyIntoGame(int _fromClient, PackerServerSide packet)
    {
        foreach(ClientServerSide _client in Server.clients.Values)
        {
            switch (NetworkManager.currentGameMode)
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

    public static void ChangeGameMode(int _fromClient, PackerServerSide _packet)
    {
        string gameModeName = _packet.ReadString();
        NetworkManager.currentGameMode = gameModeName;

        NetworkManager.instance.ChangeScene();
    }

    public static void StartGameMode(int _fromClient, PackerServerSide packet)
    {
         
    }

    public static void PlayerMovement(int _fromClient, PackerServerSide _packet)
    {
        Vector2 _moveDirection = _packet.ReadVector2();
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetMovementInput(_moveDirection, _rotation);
    }

    public static void PlayerJetPackMovement(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _direction = _packet.ReadVector3();

        Server.clients[_fromClient].player.JetPackMovement(_direction);
    }

    public static void PlayerActions(int _fromClient, PackerServerSide _packet)
    {
        bool _isAnimInProgress = _packet.ReadBool();

        Server.clients[_fromClient].player.SetActionInput(_isAnimInProgress);
    }

    public static void PlayerMagnetize(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.PlayerMagnetize();
    }

    public static void PlayerStartGrapple(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _direction = _packet.ReadVector3();

        Server.clients[_fromClient].player.StartGrapple(_direction);
    }

    public static void PlayerContinueGrappling(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _grapplePoint = _packet.ReadVector3();

        //ServerSend.OtherPlayerContinueGrapple(_fromClient, _position, _grapplePoint);
    }

    public static void PlayerStopGrapple(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.StopGrapple();
    }

    public static void PlayerStartShoot(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _firePoint = _packet.ReadVector3();
        Vector3 _fireDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.ShootController(_firePoint, _fireDirection);
    }

    public static void PlayerUpdateShootDirection(int _fromClient, PackerServerSide _packet)
    {
        Vector3 _firePoint = _packet.ReadVector3();
        Vector3 _fireDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.UpdateShootDirection(_firePoint, _fireDirection);
    }

    public static void PlayerStopShoot(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.StopShootContoller();
    }

    public static void PlayerReload(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.Reload();
    }

    public static void PlayerSwitchWeapon(int _fromClient, PackerServerSide _packet)
    {
        Server.clients[_fromClient].player.SwitchWeapon();
    }
}