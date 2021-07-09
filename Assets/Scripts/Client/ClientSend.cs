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
    public static void WelcomeReceived()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(ClientClientSide.instance.myId);
            _packet.Write(PlayerPrefs.GetString("Username", "Null"));

            SendTCPData(_packet);
        }
    }

    public static void HostChangeGameMode(string _sceneName)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.hostChangeGameMode))
        {
            _packet.Write(_sceneName);

            SendTCPData(_packet);
        }
    }

    public static void StartGame()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.hostStartGame))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerJoinLobby()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerJoinLobby))
        {
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

    public static void PlayerJetPackMovement(Vector3 _direction)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerJetPackMovement))
        {
            _packet.Write(_direction);

            SendUDPData(_packet);
        }
    }

    public static void PlayerMagnetize()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerMagnetize))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerStartGrapple(Vector3 _direction)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStartGrapple))
        {
            _packet.Write(_direction);

            SendTCPData(_packet);
        }
    }

    public static void PlayerContinueGrappling(Vector3 _position, Vector3 _grapplePoint)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerContinueGrappling))
        {
            _packet.Write(_position);
            _packet.Write(_grapplePoint);

            SendUDPData(_packet);
        }
    }

    public static void PlayerStopGrapple()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStopGrapple))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerStartShoot(Vector3 _firePoint, Vector3 _fireDirection)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStartShoot))
        {
            _packet.Write(_firePoint);
            _packet.Write(_fireDirection);

            SendTCPData(_packet);
        }
    }

    public static void PlayerUpdateShootDirection(Vector3 _firePoint, Vector3 _fireDirection)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerUpdateShootDirection))
        {
            _packet.Write(_firePoint);
            _packet.Write(_fireDirection);

            SendUDPData(_packet);
        }
    }

    public static void PlayerStopShoot()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerStopShoot))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerReload()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerReload))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerSwitchWeapon()
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerSwitchWeapon))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerThrowItem(Vector3 _facing)
    {
        using (PacketClientSide _packet = new PacketClientSide((int)ClientPackets.playerThrowItem))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }

    #endregion
}