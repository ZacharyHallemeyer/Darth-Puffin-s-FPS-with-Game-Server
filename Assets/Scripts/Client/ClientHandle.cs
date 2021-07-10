using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(PacketClientSide _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        ClientClientSide.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        ClientClientSide.instance.udp.Connect(((IPEndPoint)ClientClientSide.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void AddClient(PacketClientSide _packet)
    {
        int _clientId = _packet.ReadInt();
        string _clientUsername = _packet.ReadString();

        ClientClientSide.allClients.Add(_clientId, _clientUsername);
        ClientClientSide.instance.lobby.InitLobbyUI();   
    }

    public static void SpawnPlayer(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _gunName = _packet.ReadString();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        float _maxGrappleTime = _packet.ReadFloat();
        float _maxJetPackTime = _packet.ReadFloat();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _gunName, _currentAmmo, 
                                         _reserveAmmo, _maxGrappleTime, _maxJetPackTime);
    }

    public static void PlayerPosition(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.position = _position;
        }
    }

    public static void PlayerRotation(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.rotation = _rotation;
        }
    }

    public static void PlayerDisconnected(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        ClientClientSide.allClients.Remove(_id);
        ClientClientSide.instance.lobby.InitLobbyUI();
        if (GameManager.players.TryGetValue(_id, out PlayerManager _playerManager))
        {
            Destroy(_playerManager.gameObject);
            GameManager.players.Remove(_id);
        }
        foreach(PlayerManager _player in GameManager.players.Values)
        {
            _player.playerUI.InitScoreBoard();
        }
    }

    public static void OtherPlayerTakenDamage(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        GameManager.players[_toId].OtherPlayerTakenDamage(_fromId);
    }

    public static void PlayerHealth(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _health = _packet.ReadInt();

        GameManager.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    public static void CreateNewPlanet(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        float _gravityField = _packet.ReadFloat();

        GameManager.instance.CreatePlanet(_position, _localScale, _gravityField);
    }

    public static void CreateNewNonGravityObject(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _name = _packet.ReadString();

        GameManager.instance.CreateNonGravityObject(_position, _localScale, _rotation, _name);
    }

    public static void CreateBoundary(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        float _radius = _packet.ReadFloat();

        GameManager.instance.CreateBoundaryVisual(_position, _radius);
    }
    public static void PlayerStartGrapple(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.playersActions[_id].StartGrapple();
    }

    public static void PlayerContinueGrapple(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        float _currentGrappleTime = _packet.ReadFloat();

        GameManager.playersActions[_id].ContinueGrapple(_currentGrappleTime);
    }

    public static void OtherPlayerContinueGrapple(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Vector3 _grapplePoint = _packet.ReadVector3();

        GameManager.playersActions[_toId].DrawOtherPlayerRope(_fromId, _position, _grapplePoint);
    }

    public static void OtherPlayerStopGrapple(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        GameManager.players[_toId].ClearOtherPlayerRope(_fromId);
    }

    public static void PlayerStopGrapple(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.playersActions[_id].StopGrapple();
    }

    public static void OtherPlayerSwitchedWeapon(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        string _gunName = _packet.ReadString();

        GameManager.playersActions[_toId].ShowOtherPlayerActiveWeapon(_fromId, _gunName);
    }

    public static void PlayerSingleFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStartSingleFireAnim(_currentAmmo, _reserveAmmo);
    }

    public static void PlayerStartAutomaticFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStartAutomaticFireAnim(_currentAmmo, _reserveAmmo);
    }

    public static void PlayerContinueAutomaticFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerContinueAutomaticFireAnim(_currentAmmo, _reserveAmmo);
    }   

    public static void PlayerStopAutomaticFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStopAutomaticFireAnim();
    }

    public static void PlayerReload(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStartReloadAnim(_currentAmmo, _reserveAmmo);
    }

    public static void PlayerSwitchWeapon(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        string _newGunName = _packet.ReadString();

        GameManager.playersActions[_fromId].PlayerStartSwitchWeaponAnim(_newGunName, _currentAmmo, _reserveAmmo);
    }

    public static void PlayerShotLanded(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        Vector3 _hitPoint = _packet.ReadVector3();

        GameManager.playersActions[_fromId].PlayerShotLanded(_hitPoint);
    }

    public static void PlayerContinueJetPack(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        float _jetPackTime = _packet.ReadFloat();

        GameManager.playersMovement[_fromId].PlayerContinueJetPack(_jetPackTime);
    }

    public static void UpdatePlayerKillStats(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentKills = _packet.ReadInt();

        GameManager.players[_id].currentKills = _currentKills;
    }

    public static void UpdatePlayerDeathStats(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentDeaths = _packet.ReadInt();

        GameManager.players[_id].currentDeaths = _currentDeaths;
    }

    public static void DisplayeScoreBoard(PacketClientSide _packet)
    {
        foreach(PlayerManager _player in GameManager.players.Values)
        {
            if(_player != null)
            {

            }
        }
    }
}