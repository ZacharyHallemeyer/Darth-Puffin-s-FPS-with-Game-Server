using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(PacketClientSide _packet)
    {
        _packet.WriteLength();
        ClientClientSide.instance.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(PacketClientSide _packet)
    {
        _packet.WriteLength();
        ClientClientSide.instance.udp.SendData(_packet);
    }

    #region Packets
    /// <summary>
    /// Send server client id and client username
    /// </summary>
    public static void WelcomeReceived()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(ClientClientSide.instance.myId);
            _packet.Write(PlayerPrefs.GetString("Username", "Null"));

            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Tell server to start game
    /// </summary>
    /// <param name="_gameModeName"> game mode name to start </param>
    public static void StartGame(string _gameModeName)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.startGame))
        {
            _packet.Write(_gameModeName);

            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Tell server to start spawning environment
    /// </summary>
    public static void StartGenerateEnvironment()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.startGenerateEnvironment))
        {
            _packet.Write(1);

            SendTCPData(_packet);
        }
    }

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs"></param>
    public static void PlayerMovement(Vector2 _moveDirection)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerMovement))
        {
            _packet.Write(_moveDirection);
            // Orientation needs to be the first child of player
            _packet.Write(GameManager.players[ClientClientSide.instance.myId].transform.GetChild(0).transform.localRotation);

            SendUDPData(_packet);
        }
    }

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs"></param>
    public static void PlayerActions(bool _isAnimInProgress)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerActions))
        {
            _packet.Write(_isAnimInProgress);

            SendUDPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to use jetpack 
    /// </summary>
    /// <param name="_direction"> direction to jetpack </param>
    public static void PlayerJetPackMovement(Vector3 _direction)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerJetPackMovement))
        {
            _packet.Write(_direction);

            SendUDPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to use magnetize
    /// </summary>
    public static void PlayerMagnetize()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerMagnetize))
        {
            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to start grapple
    /// </summary>
    /// <param name="_direction"> direction to grapple </param>
    public static void PlayerStartGrapple(Vector3 _direction)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStartGrapple))
        {
            _packet.Write(_direction);

            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to continue grapple
    /// </summary>
    /// <param name="_position"> player positon </param>
    /// <param name="_grapplePoint"> player grapple point </param>
    public static void PlayerContinueGrappling(Vector3 _position, Vector3 _grapplePoint)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerContinueGrappling))
        {
            _packet.Write(_position);
            _packet.Write(_grapplePoint);

            SendUDPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to stop grapple
    /// </summary>
    public static void PlayerStopGrapple()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStopGrapple))
        {
            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to start shooting
    /// </summary>
    /// <param name="_firePoint"> player's fire point </param>
    /// <param name="_fireDirection"> player's fire direction </param>
    public static void PlayerStartShoot(Vector3 _firePoint, Vector3 _fireDirection)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStartShoot))
        {
            _packet.Write(_firePoint);
            _packet.Write(_fireDirection);

            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Send server current fire point and fire direction
    /// </summary>
    /// <param name="_firePoint"> player fire point </param>
    /// <param name="_fireDirection"> player fire direction </param>
    public static void PlayerUpdateShootDirection(Vector3 _firePoint, Vector3 _fireDirection)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerUpdateShootDirection))
        {
            _packet.Write(_firePoint);
            _packet.Write(_fireDirection);

            SendUDPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to stop shooting
    /// </summary>
    public static void PlayerStopShoot()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStopShoot))
        {
            SendTCPData(_packet);
        }
    }
    
    /// <summary>
    /// Send server info for player to reload
    /// </summary>
    public static void PlayerReload()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerReload))
        {
            SendTCPData(_packet);
        }
    }

    /// <summary>
    /// Send server info for player to switch weapon
    /// </summary>
    public static void PlayerSwitchWeapon()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerSwitchWeapon))
        {
            SendTCPData(_packet);
        }
    }

    #endregion
}