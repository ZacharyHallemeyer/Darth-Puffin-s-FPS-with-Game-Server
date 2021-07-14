using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, PackerServerSide _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, PackerServerSide _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(PackerServerSide _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, PackerServerSide _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(PackerServerSide _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, PackerServerSide _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void SendNewClient(int _toClient, ClientServerSide _client)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.addClient))
        {
            _packet.Write(_client.id);
            _packet.Write(_client.userName);

            SendTCPData(_toClient, _packet);
        }
    }


    #region Player Free For All

    public static void EnvironmentReadyFreeForAll()
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.environmentReady))
        {
            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayerFFA(int _toClient, SPlayer _player, SPlayerFFA _sPlayerFFA)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(FreeForAllEnvironmentGenerator.
                          spawnPoints[Random.Range(0, FreeForAllEnvironmentGenerator.spawnPoints.Count)]);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_sPlayerFFA.currentGun.name);
            _packet.Write(_sPlayerFFA.currentGun.currentAmmo);
            _packet.Write(_sPlayerFFA.currentGun.reserveAmmo);
            _packet.Write(_sPlayerFFA.maxGrappleTime);
            _packet.Write(_sPlayerFFA.maxJetPackPower);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPositionFFA(SPlayer _player)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients including himself </summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotationFFA(SPlayer _player, Quaternion orientation)
    {
        // Send player rotation and orientation to all clients
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.localRotation * orientation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    /// <summary>
    /// Sends a players health to all clients and sends to all clients that another player has taken damage(used for taken damage animations)
    /// </summary>
    /// <param name="_player"> Player that health has changed </param>
    public static void PlayerHealthFFA(SPlayer _player)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }

        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != _player.id)
                {
                    using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.otherPlayerTakenDamage))
                    {
                        _packet.Write(_player.id);
                        _packet.Write(_client.id);

                        SendTCPData(_client.id, _packet);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tells all clients to respawn a player
    /// </summary>
    /// <param name="_player"> player to be respawned </param>
    public static void PlayerRespawnedFFA(SPlayer _player)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// Sends a planet's data to a specific clients
    /// </summary>
    /// <param name="_toClient"> client to send new planet data </param>
    /// <param name="_position"> planet's position </param>
    /// <param name="_localScale"> planet's local scale </param>
    /// <param name="_gravityMaxDistance"> planet's gravity max distance </param>
    public static void CreateNewPlanetFFA(int _toClient, Vector3 _position, Vector3 _localScale, float _gravityMaxDistance)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.createNewPlanet))
        {
            _packet.Write(_position);
            _packet.Write(_localScale);
            _packet.Write(_gravityMaxDistance);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends a non gravity object's data to a specific clients
    /// </summary>
    /// <param name="_toClient"> client to send new planet data </param>
    /// <param name="_position"> non gravity object's position </param>
    /// <param name="_localScale"> non gravity object's local scale </param>
    /// <param name="_rotation"> non gravity object's rotation </param>
    /// <param name="_objectName"> non gravity object's name </param>
    public static void CreateNonGravityObjectFFA(int _toClient, Vector3 _position, Vector3 _localScale, Quaternion _rotation
                                              , string _objectName)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.createNewNonGravityObject))
        {
            _packet.Write(_position);
            _packet.Write(_localScale);
            _packet.Write(_rotation);
            _packet.Write(_objectName);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends boundary info to specific client
    /// </summary>
    /// <param name="_toClient"> Client to send info </param>
    /// <param name="_position"> boundary position </param>
    /// <param name="radius"> boundary radius </param>
    public static void CreateBoundaryFFA(int _toClient, Vector3 _position, float radius)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.createBoundary))
        {
            _packet.Write(_position);
            _packet.Write(radius);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Send specific client info to start grapple on client side
    /// </summary>
    /// <param name="_toClient"> client to send info to </param>
    public static void PlayerStartGrappleFFA(int _toClient)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStartGrapple))
        {
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Send specific client info to continue grapple on client side
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_currentGrappleTime"> clients current grapple time </param>
    public static void PlayerContinueGrappleFFA(int _toClient, float _currentGrappleTime)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerContinueGrapple))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentGrappleTime);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends to all clients except himself info to continue grapple for another player on client side
    /// </summary>
    /// <param name="_fromClient"> client that is grappling </param>
    /// <param name="_position"> client's position </param>
    /// <param name="_grapplePoint"> client's grapple point </param>
    public static void OtherPlayerContinueGrappleFFA(int _fromClient, Vector3 _position, Vector3 _grapplePoint)
    {
        foreach(ClientServerSide _client in Server.clients.Values)
        {
            if(_client.player != null)
            {
                if(_client.id != _fromClient)
                {
                    using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.otherPlayerContinueGrapple))
                    {
                        _packet.Write(_fromClient);
                        _packet.Write(_client.id);
                        _packet.Write(_position);
                        _packet.Write(_grapplePoint);

                        SendUDPData(_fromClient, _packet);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send specific client info to start grapple on client side
    /// </summary>
    /// <param name="_toClient"> Client to send info to </param>
    public static void PlayerStopGrappleFFA(int _toClient)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStopGrapple))
        {
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != _toClient)
                {
                    using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.otherPlayerStopGrapple))
                    {
                        _packet.Write(_toClient);
                        _packet.Write(_client.id);

                        SendUDPData(_toClient, _packet);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sends to specific client data of a client changing weapon
    /// </summary>
    /// <param name="_fromClient"> client that changed weapon </param>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_gunName"> client that changed weapons new weapon's name </param>
    public static void OtherPlayerSwitchedWeaponFFA(int _fromClient, int _toClient, string _gunName)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.otherPlayerSwitchedWeapon))
        {
            _packet.Write(_fromClient);
            _packet.Write(_toClient);
            _packet.Write(_gunName);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to start single fire on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_currentAmmo"> clients current ammo </param>
    /// <param name="_reserveAmmo"> clients current reserve ammo </param>
    public static void PlayerSingleFireFFA(int _toClient, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerSinglefire))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to start automatic fire on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_currentAmmo"> client's current ammo </param>
    /// <param name="_reserveAmmo"> client's current reserve ammo</param>
    public static void PlayerStartAutomaticFireFFA(int _toClient, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStartAutomaticFire))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to continue automatic fire on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_currentAmmo"> client's current ammo </param>
    /// <param name="_reserveAmmo"> client's current reserve ammo</param>
    public static void PlayerContinueAutomaticFireFFA(int _toClient, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerContinueAutomaticFire))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to stop automatic fire on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    public static void PlayerStopAutomaticFireFFA(int _toClient)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStopAutomaticFire))
        {
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to reload on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_currentAmmo"> client's current ammo </param>
    /// <param name="_reserveAmmo"> client's current reserve ammo</param>
    public static void PlayerReloadFFA(int _toClient, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerReload))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to switch wepaon on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_gunName"> client's new weapon name </param>
    /// <param name="_currentAmmo"> client's current ammo </param>
    /// <param name="_reserveAmmo"> client's current reserve ammo</param>
    public static void PlayerSwitchWeaponFFA(int _toClient, string _gunName, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerSwitchWeapon))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);
            _packet.Write(_gunName);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Send info to specific client that a shot landed
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_hitPoint"> position where shot landed </param>
    public static void PlayerShotLandedFFA(int _toClient, Vector3 _hitPoint)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerShotLanded))
        {
            _packet.Write(_toClient);
            _packet.Write(_hitPoint);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Send info to specific client to continue jetpack on client side
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_currentJetPackTime"> client's current jet pack time </param>
    public static void PlayerContinueJetPackFFA(int _toClient, float _currentJetPackTime)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerContinueJetPack))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentJetPackTime);

            SendTCPData(_toClient, _packet);
        }
    }


    #endregion

    #region Player Infection

    public static void EnvironmentReadyInfection()
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.environmentReady))
        {
            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// Sends a building's data to a specific clients
    /// </summary>
    /// <param name="_toClient"> client to send new planet data </param>
    /// <param name="_position"> planet's position </param>
    /// <param name="_localScale"> planet's local scale </param>
    public static void CreateBuildingInfection(int _toClient, Vector3 _position, Vector3 _localScale, int _key)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.createBuilding))
        {
            _packet.Write(_position);
            _packet.Write(_localScale);
            _packet.Write(_key);

            SendTCPData(_toClient, _packet);
        }
    }
    
    /// <summary>
    /// Sends a planet's data to a specific clients
    /// </summary>
    /// <param name="_toClient"> client to send new planet data </param>
    /// <param name="_position"> planet's position </param>
    /// <param name="_localScale"> planet's local scale </param>
    public static void CreateSunsInfection(int _toClient, Vector3 _position, Vector3 _localScale)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.createNewPlanet))
        {
            _packet.Write(_position);
            _packet.Write(_localScale);

            SendTCPData(_toClient, _packet);
        }
    }



    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayerInfection(int _toClient, SPlayer _player, SPlayerInfection _sPlayerInfection)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            //_packet.Write(_player.transform.position);
            _packet.Write(InfectionEnvironmentGenerator.spawnPoints[
                          Random.Range(0, InfectionEnvironmentGenerator.spawnPoints.Count)]);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_sPlayerInfection.currentGun.name);
            _packet.Write(_sPlayerInfection.currentGun.currentAmmo);
            _packet.Write(_sPlayerInfection.currentGun.reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }


    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPositionInfection(SPlayer _player)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients including himself </summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotationInfection(SPlayer _player, Quaternion orientation)
    {
        // Send player rotation and orientation to all clients
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.localRotation * orientation);

            SendTCPDataToAll(_player.id, _packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients including himself </summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerLocalScaleInfection(SPlayer _player)
    {
        // Send player rotation and orientation to all clients
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerLocalScale))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.localScale);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerMeleeInfection(int _id)
    {
        // Send player rotation and orientation to all clients
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerMelee))
        {
            _packet.Write(_id);

            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated crouch state to all clients </summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerStartCrouchInfection(int _id)
    {
        // Send player rotation and orientation to all clients
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStartCrouch))
        {
            _packet.Write(_id);

            SendTCPData(_id, _packet);
        }
    }

    /// <summary>Sends a player's updated crouch state to all clients </summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerStopCrouchInfection(int _id)
    {
        // Send player rotation and orientation to all clients
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStopCrouch))
        {
            _packet.Write(_id);

            SendTCPData(_id, _packet);
        }
    }

    /// <summary>
    /// Sends a players health to all clients and sends to all clients that another player has taken damage(used for taken damage animations)
    /// </summary>
    /// <param name="_player"> Player that health has changed </param>
    public static void PlayerHealthInfection(SPlayer _player)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }

        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != _player.id)
                {
                    using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.otherPlayerTakenDamage))
                    {
                        _packet.Write(_player.id);
                        _packet.Write(_client.id);

                        SendTCPData(_client.id, _packet);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tells all clients to respawn a player
    /// </summary>
    /// <param name="_player"> player to be respawned </param>
    public static void PlayerRespawnedInfection(SPlayer _player)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }


    /// <summary>
    /// Sends to specific client data of a client changing weapon
    /// </summary>
    /// <param name="_fromClient"> client that changed weapon </param>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_gunName"> client that changed weapons new weapon's name </param>
    public static void OtherPlayerSwitchedWeaponInfection(int _fromClient, int _toClient, string _gunName)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.otherPlayerSwitchedWeapon))
        {
            _packet.Write(_fromClient);
            _packet.Write(_toClient);
            _packet.Write(_gunName);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to start single fire on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_currentAmmo"> clients current ammo </param>
    /// <param name="_reserveAmmo"> clients current reserve ammo </param>
    public static void PlayerSingleFireInfection(int _toClient, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerSinglefire))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to reload on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_currentAmmo"> client's current ammo </param>
    /// <param name="_reserveAmmo"> client's current reserve ammo</param>
    public static void PlayerReloadInfection(int _toClient, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerReload))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Sends info to specific client to switch wepaon on client side
    /// </summary>
    /// <param name="_toClient"> client to send data </param>
    /// <param name="_gunName"> client's new weapon name </param>
    /// <param name="_currentAmmo"> client's current ammo </param>
    /// <param name="_reserveAmmo"> client's current reserve ammo</param>
    public static void PlayerSwitchWeaponInfection(int _toClient, string _gunName, int _currentAmmo, int _reserveAmmo)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerSwitchWeapon))
        {
            _packet.Write(_toClient);
            _packet.Write(_currentAmmo);
            _packet.Write(_reserveAmmo);
            _packet.Write(_gunName);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Send info to specific client that a shot landed
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_hitPoint"> position where shot landed </param>
    public static void PlayerShotLandedInfection(int _toClient, Vector3 _hitPoint)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerShotLanded))
        {
            _packet.Write(_toClient);
            _packet.Write(_hitPoint);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>
    /// Send info to specific client that a shot landed
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_hitPoint"> position where shot landed </param>
    public static void PlayerStartWallrun(int _toClient, bool _isWallLeft, bool _isWallRight)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStartWallrun))
        {
            _packet.Write(_isWallLeft);
            _packet.Write(_isWallRight);

            SendTCPData(_toClient, _packet);
        }
    }


    /// <summary>
    /// Send info to specific client that a shot landed
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_hitPoint"> position where shot landed </param>
    public static void PlayerContinueWallrun(int _toClient)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerContinueWallrun))
        {
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }


    /// <summary>
    /// Send info to specific client that a shot landed
    /// </summary>
    /// <param name="_toClient"> client to send info </param>
    /// <param name="_hitPoint"> position where shot landed </param>
    public static void PlayerStopWallrun(int _toClient)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerStopWallrun))
        {
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    #endregion

    /// <summary>
    /// Updata client's kill stats to all clients
    /// </summary>
    /// <param name="_fromClient"> Client to update kill stats </param>
    /// <param name="_currentKills"> Client's current kill stats </param>
    public static void UpdatePlayerKillStats(int _fromClient, int _currentKills)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.updatePlayerKillStats))
        {
            _packet.Write(_fromClient);
            _packet.Write(_currentKills);

            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// Updata client's death stats to all clients
    /// </summary>
    /// <param name="_fromClient"> Client to update death stats </param>
    /// <param name="_currentDeaths"> Client's current death stats </param>
    public static void UpdatePlayerDeathStats(int _fromClient, int _currentDeaths)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.updatePlayerDeathStats))
        {
            _packet.Write(_fromClient);
            _packet.Write(_currentDeaths);

            SendTCPDataToAll(_packet);
        }
    }

    /// <summary>
    /// Tells all clients that a client has disconnected
    /// </summary>
    /// <param name="_playerId"> The player's id that disconnected</param>
    public static void PlayerDisconnect(int _playerId)
    {
        using (PackerServerSide _packet = new PackerServerSide((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }

    #endregion
}