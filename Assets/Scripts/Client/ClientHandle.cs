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

    #region Player Free For All

    public static void EnvironmentReadyFreeForAll(PacketClientSide _packet)
    {
        ClientClientSide.instance.lobby.ToggleStartButtonState();
    }

    /// <summary>
    /// Spawn Free for all player
    /// </summary>
    /// <param name="_packet"> id, username, position, rotation, gunName, currentAmmo, 
    /// reserveAmmo, maxGrappleTime,  maxJetPackTime </param>
    public static void SpawnPlayerFFA(PacketClientSide _packet)
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
        CFFAGameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _gunName, _currentAmmo, 
                                         _reserveAmmo, _maxGrappleTime, _maxJetPackTime);
    }

    /// <summary>
    /// Recieve player position from server
    /// </summary>
    /// <param name="_packet">id, position</param>
    public static void PlayerPositionFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (CFFAGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.position = _position;
        }
    }

    /// <summary>
    /// Recieve player rotation from server
    /// </summary>
    /// <param name="_packet">id, rotation</param>
    public static void PlayerRotationFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (CFFAGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.localRotation = _rotation;
        }
    }

    /// <summary>
    /// Recieve which player disconnected from server and remove from dictionries
    /// </summary>
    /// <param name="_packet"> id </param>
    public static void PlayerDisconnectedFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        ClientClientSide.allClients.Remove(_id);
        ClientClientSide.instance.lobby.InitLobbyUI();
        if (CFFAGameManager.players.TryGetValue(_id, out PlayerManager _playerManager))
        {
            Destroy(_playerManager.gameObject);
            CFFAGameManager.players.Remove(_id);
            CFFAGameManager.playersActions.Remove(_id);
        }
        foreach(PlayerManager _player in CFFAGameManager.players.Values)
        {
            _player.playerUI.InitScoreBoard();
        }
    }

    /// <summary>
    /// Recieve specific player health from health
    /// </summary>
    /// <param name="_packet"> id, health </param>
    public static void PlayerHealthFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _health = _packet.ReadInt();

        CFFAGameManager.players[_id].SetHealth(_health);
    }

    /// <summary>
    /// Recieve which player took damage from server and start damage animation for that player
    /// </summary>
    /// <param name="_packet">fromId, toId</param>
    public static void OtherPlayerTakenDamageFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        CFFAGameManager.players[_toId].OtherPlayerTakenDamage(_fromId);
    }

    /// <summary>
    /// Recieve info that player respawned from server
    /// </summary>
    /// <param name="_packet">id</param>
    public static void PlayerRespawnedFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        CFFAGameManager.players[_id].Respawn();
    }

    /// <summary>
    /// Recieve planet info from server and create new planet
    /// </summary>
    /// <param name="_packet">position, localScale, gravityField (float) </param>
    public static void CreateNewPlanetFFA(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        float _gravityField = _packet.ReadFloat();

        CFFAGameManager.instance.CreatePlanet(_position, _localScale, _gravityField);
    }

    /// <summary>
    /// Recieve non gravity object info from server and create new non gravity object
    /// </summary>
    /// <param name="_packet">position, localScale, rotation, name</param>
    public static void CreateNewNonGravityObjectFFA(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _name = _packet.ReadString();

        CFFAGameManager.instance.CreateNonGravityObject(_position, _localScale, _rotation, _name);
    }

    /// <summary>
    /// Recieve boundary info from server and create new boundary
    /// </summary>
    /// <param name="_packet">position, radius </param>
    public static void CreateBoundaryFFA(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        float _radius = _packet.ReadFloat();

        CFFAGameManager.instance.CreateBoundaryVisual(_position, _radius);
    }

    /// <summary>
    /// Recieve player started grapple from server and play animation
    /// </summary>
    /// <param name="_packet">id/param>
    public static void PlayerStartGrappleFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        CFFAGameManager.playersActions[_id].StartGrapple();
    }

    /// <summary>
    /// Recieve player continue grapple from server and play animation
    /// </summary>
    /// <param name="_packet">id, currentGrappleTime/param>
    public static void PlayerContinueGrappleFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        float _currentGrappleTime = _packet.ReadFloat();

        CFFAGameManager.playersActions[_id].ContinueGrapple(_currentGrappleTime);
    }

    /// <summary>
    /// Recieve other player started grapple from server and play animation
    /// </summary>
    /// <param name="_packet"> fromId(client grappling), toId(this client), _position (client grappling position), 
    /// grapplePoint(client grappling position), /param>
    public static void OtherPlayerContinueGrappleFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Vector3 _grapplePoint = _packet.ReadVector3();

        //CFFAGameManager.playersActions[_toId].DrawOtherPlayerRope(_fromId, _position, _grapplePoint);
    }

    /// <summary>
    /// Recieve player stoped grapple from server and play animation
    /// </summary>
    /// <param name="_packet">fromId (client that stoped grapple), toId (this client)/param>
    public static void OtherPlayerStopGrappleFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        CFFAGameManager.players[_toId].ClearOtherPlayerRope(_fromId);
    }

    /// <summary>
    /// Recieve player stopped grapple from server and play animation
    /// </summary>
    /// <param name="_packet">id/param>
    public static void PlayerStopGrappleFFA(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        CFFAGameManager.playersActions[_id].StopGrapple();
    }

    /// <summary>
    /// Recieve other player switched weapon from server and play animation
    /// </summary>
    /// <param name="_packet">fromId, toId, gunName/param>
    public static void OtherPlayerSwitchedWeaponFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        string _gunName = _packet.ReadString();

        CFFAGameManager.playersActions[_toId].ShowOtherPlayerActiveWeapon(_fromId, _gunName);
    }

    /// <summary>
    /// Recieve player started single firefrom server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerSingleFireFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        CFFAGameManager.playersActions[_fromId].PlayerStartSingleFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player started automatic fire from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerStartAutomaticFireFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        CFFAGameManager.playersActions[_fromId].PlayerStartAutomaticFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player continue automatic fire from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerContinueAutomaticFireFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        CFFAGameManager.playersActions[_fromId].PlayerContinueAutomaticFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player stoped automatic fire from server and play animation
    /// </summary>
    /// <param name="_packet">id/param>
    public static void PlayerStopAutomaticFireFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        CFFAGameManager.playersActions[_fromId].PlayerStopAutomaticFireAnim();
    }

    /// <summary>
    /// Recieve player stoped reloading from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo/param>
    public static void PlayerReloadFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        CFFAGameManager.playersActions[_fromId].PlayerStartReloadAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve player swithced weapon from server and play animation
    /// </summary>
    /// <param name="_packet">id, current ammo, reserve ammo /param>
    public static void PlayerSwitchWeaponFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        string _newGunName = _packet.ReadString();

        CFFAGameManager.playersActions[_fromId].PlayerStartSwitchWeaponAnim(_newGunName, _currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// Recieve shot landed from server and play animation
    /// </summary>
    /// <param name="_packet">id, hit point/param>
    public static void PlayerShotLandedFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        Vector3 _hitPoint = _packet.ReadVector3();

        CFFAGameManager.playersActions[_fromId].PlayerShotLanded(_hitPoint);
    }

    /// <summary>
    /// Recieve player continue Jet Pack from server and play animation
    /// </summary>
    /// <param name="_packet">id, jetPackTime/param>
    public static void PlayerContinueJetPackFFA(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        float _jetPackTime = _packet.ReadFloat();

        CFFAGameManager.playersActions[_fromId].PlayerContinueJetPack(_jetPackTime);
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

        CFFAGameManager.players[_id].currentKills = _currentKills;
    }

    /// <summary>
    /// Recieve player death stats and update stats
    /// </summary>
    /// <param name="_packet">id, current deaths/<param>
    public static void UpdatePlayerDeathStats(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentDeaths = _packet.ReadInt();

        CFFAGameManager.players[_id].currentDeaths = _currentDeaths;
    }

    #endregion


    #endregion

    #region Player Infection

    public static void EnvironmentReadyInfection(PacketClientSide _packet)
    {
        ClientClientSide.instance.lobby.ToggleStartButtonState();
    }

    /// <summary>
    /// Recieve planet info from server and create new planet
    /// </summary>
    /// <param name="_packet">position, localScale, gravityField (float) </param>
    public static void CreateNewSun(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();

        CInfectionGameManager.instance.SpawnSun(_position, _localScale);
    }

    /// <summary>
    /// Recieve planet info from server and create new planet
    /// </summary>
    /// <param name="_packet">position, localScale, gravityField (float) </param>
    public static void CreateNewBulding(PacketClientSide _packet)
    {
        Vector3 _position = _packet.ReadVector3();
        Vector3 _localScale = _packet.ReadVector3();
        int _key = _packet.ReadInt();

        CInfectionGameManager.instance.SpawnBuilding(_position, _localScale, _key);
    }

    /// <summary>
    /// Spawn player
    /// </summary>
    /// <param name="_packet"> id, username, position, rotation, gunName, currentAmmo, 
    /// reserveAmmo, maxGrappleTime,  maxJetPackTime </param>
    public static void SpawnPlayerInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        string _gunName = _packet.ReadString();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();

        // Turn off lobby UI if it has not already
        if (ClientClientSide.instance.lobby.lobbyParent.activeInHierarchy)
            ClientClientSide.instance.lobby.lobbyParent.SetActive(false);
        CInfectionGameManager.instance.SpawnPlayer(_id, _username, _position, _rotation, _gunName, _currentAmmo,
                                                   _reserveAmmo);
    }

    /// <summary>
    /// Updates player position 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerPositionInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        
        if(CInfectionGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.position = _position;
        }
    }

    /// <summary>
    /// Updates player rotation
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerRotationInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (CInfectionGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.localRotation = _rotation; 
        }
    }

    /// <summary>
    /// Updates player position 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerLocalPlayerInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _localScale = _packet.ReadVector3();
        
        if(CInfectionGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.localScale = _localScale;
        }
    }

    /// <summary>
    /// Updates player rotation
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerStartCrouchInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _localScale = _packet.ReadVector3();

        if (CInfectionGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.localScale = _localScale;
            CInfectionGameManager.playersActionsInfection[_id].StartCrouch();
        }
    }

    /// <summary>
    /// Updates player rotation
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerStopCrouchInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _localScale= _packet.ReadVector3();

        if (CInfectionGameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player.transform.localScale = _localScale;
            CInfectionGameManager.playersActionsInfection[_id].StopCrouch();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerDisconnectedInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        ClientClientSide.allClients.Remove(_id);
        ClientClientSide.instance.lobby.InitLobbyUI();
        if (CInfectionGameManager.players.TryGetValue(_id, out PlayerManager _playerManager))
        {
            Destroy(_playerManager.gameObject);
            CInfectionGameManager.players.Remove(_id);
            CInfectionGameManager.playersActionsInfection.Remove(_id);
        }
        foreach (PlayerManager _player in CFFAGameManager.players.Values)
        {
            _player.playerUI.InitScoreBoard();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerHealthInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _health = _packet.ReadInt();

        CInfectionGameManager.players[_id].SetHealth(_health);
    }

    /// <summary>
    /// Recieve which player took damage from server and start damage animation for that player
    /// </summary>
    /// <param name="_packet">fromId, toId</param>
    public static void OtherPlayerTakenDamageInfection(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();

        CInfectionGameManager.playersActionsInfection[_toId].OtherPlayerTakenDamage(_fromId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerRespawnedInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();

        CInfectionGameManager.players[_id].Respawn();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void OtherPlayerSwitchedWeaponInfection(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _toId = _packet.ReadInt();
        string _gunName = _packet.ReadString();

        CInfectionGameManager.playersActionsInfection[_toId].ShowOtherPlayerActiveWeapon(_fromId, _gunName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerShootInfection(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        CInfectionGameManager.playersActionsInfection[_fromId].PlayerStartSingleFireAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerReloadInfection(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        CInfectionGameManager.playersActionsInfection[_fromId].PlayerStartReloadAnim(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerSwitchWeaponInfection(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        int _currentAmmo = _packet.ReadInt();
        int _reserveAmmo = _packet.ReadInt();
        string _newGunName = _packet.ReadString();

        CInfectionGameManager.playersActionsInfection[_fromId].PlayerStartSwitchWeaponAnim(_newGunName, _currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerShotLandedInfection(PacketClientSide _packet)
    {
        int _fromId = _packet.ReadInt();
        Vector3 _hitPoint = _packet.ReadVector3();

        CInfectionGameManager.playersActionsInfection[_fromId].PlayerShotLanded(_hitPoint);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerStartWallRunInfection(PacketClientSide _packet)
    {
        PlayerCamController.isWallLeft = _packet.ReadBool();
        PlayerCamController.isWallRight = _packet.ReadBool();
        PlayerCamController.isOnWall = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerContinueWallRunInfection(PacketClientSide _packet)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_packet"></param>
    public static void PlayerStopWallRunInfection(PacketClientSide _packet)
    {
        PlayerCamController.isWallLeft = false;
        PlayerCamController.isWallRight = false;
        PlayerCamController.isOnWall = true;
    }

    /// <summary>
    /// Recieve player kills stats and update stats
    /// </summary>
    /// <param name="_packet">id, current kills/<param>
    public static void UpdatePlayerKillStatsInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentKills = _packet.ReadInt();

        CInfectionGameManager.players[_id].currentKills = _currentKills;
    }

    /// <summary>
    /// Recieve player death stats and update stats
    /// </summary>
    /// <param name="_packet">id, current deaths/<param>
    public static void UpdatePlayerDeathStatsInfection(PacketClientSide _packet)
    {
        int _id = _packet.ReadInt();
        int _currentDeaths = _packet.ReadInt();

        CInfectionGameManager.players[_id].currentDeaths = _currentDeaths;
    }

    #endregion
}