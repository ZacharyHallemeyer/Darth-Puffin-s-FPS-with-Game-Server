using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    /// <summary>
    /// Connects client to server
    /// </summary>
    /// <param name="_packet">msg and id</param>
    public static void Welcome(PacketClientSide _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        ClientClientSide.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        ClientClientSide.instance.udp.Connect(((IPEndPoint)ClientClientSide.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    /// <summary>
    /// Adds client to allClient dictionary and inits lobby with new client
    /// </summary>
    /// <param name="_packet"></param>
    public static void AddClient(PacketClientSide _packet)
    {
        int _clientId = _packet.ReadInt();
        string _clientUsername = _packet.ReadString();

        ClientClientSide.allClients.Add(_clientId, _clientUsername);
        ClientClientSide.instance.lobby.InitLobbyUI();   
    }

    /// <summary>
    /// Spawn player
    /// </summary>
    /// <param name="_packet"> id, username, position, rotation, gunName, currentAmmo, 
    /// reserveAmmo, maxGrappleTime,  maxJetPackTime </param>
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

        // Turn off lobby UI if it has not already
        if (ClientClientSide.instance.lobby.lobbyParent.activeInHierarchy)
            ClientClientSide.instance.lobby.lobbyParent.SetActive(false);
        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _gunName, _currentAmmo, 
                                         _reserveAmmo, _maxGrappleTime, _maxJetPackTime);
    }

    /// <summary>
    /// Recieve player position from server
    /// </summary>
    /// <param name="_packet">id, position</param>
    public static void PlayerPosition(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.position = _position;
        }
    }

    /// <summary>
    /// Recieve player rotation from server
    /// </summary>
    /// <param name="_packet">id, rotation</param>
    public static void PlayerRotation(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.localRotation = _rotation;
        }
    }

    /// <summary>
    /// Recieve which player disconnected from server and remove from dictionries
    /// </summary>
    /// <param name="_packet"> id </param>
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

    /// <summary>
    /// Recieve specific player health from health
    /// </summary>
    /// <param name="_packet"> id, health </param>
    public static void PlayerHealth(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _health = _packet.ReadInt();

        GameManager.players[_id].SetHealth(_health);
    }

    /// <summary>
    /// Recieve which player took damage from server and start damage animation for that player
    /// </summary>
    /// <param name="_packet">fromId, toId</param>
    public static void OtherPlayerTakenDamage(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        GameManager.players[_toId].OtherPlayerTakenDamage(_fromId);
    }

    /// <summary>
    /// Recieve info that player respawned from server
    /// </summary>
    /// <param name="_packet">id</param>
    public static void PlayerRespawned(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.players[_id].Respawn();
    }

    /// <summary>
    /// Recieve planet info from server and create new planet
    /// </summary>
    /// <param name="_packet">position, localScale, gravityField (float) </param>
    public static void CreateNewPlanet(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        float _gravityField = _packet.ReadFloat();

        GameManager.instance.CreatePlanet(_position, _localScale, _gravityField);
    }

    /// <summary>
    /// Recieve non gravity object info from server and create new non gravity object
    /// </summary>
    /// <param name="_packet">position, localScale, rotation, name</param>
    public static void CreateNewNonGravityObject(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _name = _packet.ReadString();

        GameManager.instance.CreateNonGravityObject(_position, _localScale, _rotation, _name);
    }

    /// <summary>
    /// Recieve boundary info from server and create new boundary
    /// </summary>
    /// <param name="_packet">position, radius </param>
    public static void CreateBoundary(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        float _radius = _packet.ReadFloat();

        GameManager.instance.CreateBoundaryVisual(_position, _radius);
    }

    #region Player Actions

    /// <summary>
    /// Recieve player started grapple from server and play animation
    /// </summary>
    /// <param name="_packet">id/param>
    public static void PlayerStartGrapple(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.playersActions[_id].StartGrapple();
    }

    /// <summary>
    /// Recieve player continue grapple from server and play animation
    /// </summary>
    /// <param name="_packet">id, currentGrappleTime/param>
    public static void PlayerContinueGrapple(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        float _currentGrappleTime = _packet.ReadFloat();

        GameManager.playersActions[_id].ContinueGrapple(_currentGrappleTime);
    }

    /// <summary>
    /// Recieve other player started grapple from server and play animation
    /// </summary>
    /// <param name="_packet"> fromId(client grappling), toId(this client), _position (client grappling position), 
    /// grapplePoint(client grappling position), /param>
    public static void OtherPlayerContinueGrapple(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Vector3 _grapplePoint = _packet.ReadVector3();

        //GameManager.playersActions[_toId].DrawOtherPlayerRope(_fromId, _position, _grapplePoint);
    }

    /// <summary>
    /// Recieve player stoped grapple from server and play animation
    /// </summary>
    /// <param name="_packet">fromId (client that stoped grapple), toId (this client)/param>
    public static void OtherPlayerStopGrapple(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        GameManager.players[_toId].ClearOtherPlayerRope(_fromId);
    }

    /// <summary>
    /// Recieve player stopped grapple from server and play animation
    /// </summary>
    /// <param name="_packet">id/param>
    public static void PlayerStopGrapple(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        GameManager.playersActions[_id].StopGrapple();
    }

    /// <summary>
    /// Recieve other player switched weapon from server and play animation
    /// </summary>
    /// <param name="_packet">fromId, toId, gunName/param>
    public static void OtherPlayerSwitchedWeapon(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        string _gunName = _packet.ReadString();

        GameManager.playersActions[_toId].ShowOtherPlayerActiveWeapon(_fromId, _gunName);
    }

    /// <summary>
    /// Recieve player started single firefrom server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerSingleFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStartSingleFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player started automatic fire from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerStartAutomaticFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStartAutomaticFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player continue automatic fire from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerContinueAutomaticFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerContinueAutomaticFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player stoped automatic fire from server and play animation
    /// </summary>
    /// <param name="_packet">id/param>
    public static void PlayerStopAutomaticFire(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStopAutomaticFireAnim();
    }

    /// <summary>
    /// Recieve player stoped reloading from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerReload(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        GameManager.playersActions[_fromId].PlayerStartReloadAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player swithced weapon from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo /param>
    public static void PlayerSwitchWeapon(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        string _newGunName = _packet.ReadString();

        GameManager.playersActions[_fromId].PlayerStartSwitchWeaponAnim(_newGunName, _currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve shot landed from server and play animation
    /// </summary>
    /// <param name="_packet">id, hit point/param>
    public static void PlayerShotLanded(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        Vector3 _hitPoint = _packet.ReadVector3();

        GameManager.playersActions[_fromId].PlayerShotLanded(_hitPoint);
    }

    #endregion

    /// <summary>
    /// Recieve player continue Jet Pack from server and play animation
    /// </summary>
    /// <param name="_packet">id, jetPackTime/param>
    public static void PlayerContinueJetPack(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        float _jetPackTime = _packet.ReadFloat();

        GameManager.playersMovement[_fromId].PlayerContinueJetPack(_jetPackTime);
    }


    #region Player Stats
    /// <summary>
    /// Recieve player kills stats and update stats
    /// </summary>
    /// <param name="_packet">id, current kills/<param>
    public static void UpdatePlayerKillStats(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentKills = _packet.ReadInt();

        GameManager.players[_id].currentKills = _currentKills;
    }

    /// <summary>
    /// Recieve player death stats and update stats
    /// </summary>
    /// <param name="_packet">id, current deaths/<param>
    public static void UpdatePlayerDeathStats(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentDeaths = _packet.ReadInt();

        GameManager.players[_id].currentDeaths = _currentDeaths;
    }
    #endregion
}