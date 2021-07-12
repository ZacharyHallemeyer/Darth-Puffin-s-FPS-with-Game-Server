using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static bool isHost = false;
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, ClientServerSide> clients = new Dictionary<int, ClientServerSide>();
    public delegate void PacketHandler(int _fromClient, PackerServerSide _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    /// <summary>Starts the server.</summary>
    /// <param name="_maxPlayers">The maximum players that can be connected simultaneously.</param>
    /// <param name="_port">The port to start the server on.</param>
    public static void Start(int _maxPlayers, int _port)
    {
        isHost = true;
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on port {Port}.");
    }

    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (PackerServerSide _packet = new PackerServerSide(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    // Ensures that the client is not being impersonated by another by sending a false clientID
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
    public static void SendUDPData(IPEndPoint _clientEndPoint, PackerServerSide _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    /// <summary>Initializes all necessary server data.</summary>
    public static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new ClientServerSide(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.startGame, ServerHandle.SendLobbyIntoGame },
            /*
            { (int)ClientPackets.startGenerateEnvironment, ServerHandle.StartGenerateEnvironment },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovementFFA },
            { (int)ClientPackets.playerJetPackMovement, ServerHandle.PlayerJetPackMovement },
            { (int)ClientPackets.playerActions, ServerHandle.PlayerActions },
            { (int)ClientPackets.playerStartGrapple, ServerHandle.PlayerStartGrapple },
            { (int)ClientPackets.playerContinueGrappling, ServerHandle.PlayerContinueGrappling },
            { (int)ClientPackets.playerStopGrapple, ServerHandle.PlayerStopGrapple },
            { (int)ClientPackets.playerStartShoot, ServerHandle.PlayerStartShoot },
            { (int)ClientPackets.playerUpdateShootDirection, ServerHandle.PlayerUpdateShootDirection },
            { (int)ClientPackets.playerStopShoot, ServerHandle.PlayerStopShoot },
            { (int)ClientPackets.playerReload, ServerHandle.PlayerReload },
            { (int)ClientPackets.playerSwitchWeapon, ServerHandle.PlayerSwitchWeapon },
            { (int)ClientPackets.playerMagnetize, ServerHandle.PlayerMagnetize },
            */
        };
        Debug.Log("Initialized Server base packets.");
    }

    public static void ChangeServerDataToFreeForAll ()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.startGame, ServerHandle.SendLobbyIntoGame },
            { (int)ClientPackets.startGenerateEnvironment, ServerHandle.StartGenerateEnvironment },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovementFFA },
            { (int)ClientPackets.playerJetPackMovement, ServerHandle.PlayerJetPackMovement },
            { (int)ClientPackets.playerActions, ServerHandle.PlayerActions },
            { (int)ClientPackets.playerStartGrapple, ServerHandle.PlayerStartGrapple },
            { (int)ClientPackets.playerContinueGrappling, ServerHandle.PlayerContinueGrappling },
            { (int)ClientPackets.playerStopGrapple, ServerHandle.PlayerStopGrapple },
            { (int)ClientPackets.playerStartShoot, ServerHandle.PlayerStartShoot },
            { (int)ClientPackets.playerUpdateShootDirection, ServerHandle.PlayerUpdateShootDirection },
            { (int)ClientPackets.playerStopShoot, ServerHandle.PlayerStopShoot },
            { (int)ClientPackets.playerReload, ServerHandle.PlayerReload },
            { (int)ClientPackets.playerSwitchWeapon, ServerHandle.PlayerSwitchWeapon },
            { (int)ClientPackets.playerMagnetize, ServerHandle.PlayerMagnetize },
        };
        Debug.Log("Initialized Server Free For All packets.");
    }

    public static void ChangeServerDataToInfection()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            { (int)ClientPackets.startGame, ServerHandle.SendLobbyIntoGame },
            { (int)ClientPackets.startGenerateEnvironment, ServerHandle.StartGenerateEnvironmentInfection },
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovementInfection },
            { (int)ClientPackets.playerJump, ServerHandle.PlayerJumpInfection },
            { (int)ClientPackets.playerCrouch, ServerHandle.PlayerCrouchInfection },
            { (int)ClientPackets.playerActions, ServerHandle.PlayerActionsInfection},
            { (int)ClientPackets.playerStartShoot, ServerHandle.PlayerShootInfection },
            { (int)ClientPackets.playerReload, ServerHandle.PlayerReloadInfection },
            { (int)ClientPackets.playerSwitchWeapon, ServerHandle.PlayerSwitchWeaponInfection},
        };
        Debug.Log("Initialized Server Infection Packets.");
    }

    /// <summary>
    /// Closes server
    /// </summary>
    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}