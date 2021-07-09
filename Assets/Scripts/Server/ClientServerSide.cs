﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientServerSide
{
    public static int dataBufferSize = 4096;

    public int id;
    public string userName;
    public PlayerServerSide player;
    public TCP tcp;
    public UDP udp;

    //public static Dictionary<int, string> allClients = new Dictionary<int, string>();
    public static Dictionary<int, ClientServerSide> allClients = new Dictionary<int, ClientServerSide>();

    public ClientServerSide(int _clientId)
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private PackerServerSide receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }

        /// <summary>Initializes the newly connected client's TCP-related info.</summary>
        /// <param name="_socket">The TcpClient instance of the newly connected client.</param>
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new PackerServerSide();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "Welcome to the server!");
        }

        /// <summary>Sends data to the client via TCP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData(PackerServerSide _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null); // Send data to appropriate client
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {_ex}");
            }
        }

        /// <summary>Reads incoming data from the stream.</summary>
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data)); // Reset receivedData if all data was handled
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving TCP data: {_ex}");
                Server.clients[id].Disconnect();
            }
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_data">The recieved data.</param>
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                // If client's received data contains a packet
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    // If packet contains no data
                    return true; // Reset receivedData instance to allow it to be reused
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManagerServerSide.ExecuteOnMainThread(() =>
                {
                    using (PackerServerSide _packet = new PackerServerSide(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                    }
                });

                _packetLength = 0; // Reset packet length
                if (receivedData.UnreadLength() >= 4)
                {
                    // If client's received data contains another packet
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true; // Reset receivedData instance to allow it to be reused
            }

            return false;
        }

        /// <summary>Closes and cleans up the TCP connection.</summary>
        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        /// <summary>Initializes the newly connected client's UDP-related info.</summary>
        /// <param name="_endPoint">The IPEndPoint instance of the newly connected client.</param>
        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        /// <summary>Sends data to the client via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData(PackerServerSide _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_packetData">The packet containing the recieved data.</param>
        public void HandleData(PackerServerSide _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManagerServerSide.ExecuteOnMainThread(() =>
            {
                using (PackerServerSide _packet = new PackerServerSide(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                }
            });
        }

        /// <summary>Cleans up the UDP connection.</summary>
        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoLobby(string _playerName)
    {
        userName = _playerName;
        foreach(int _clientId in allClients.Keys)
        {
            ServerSend.SendNewClient(_clientId, this);
            if(_clientId != id)
                ServerSend.SendNewClient(id, allClients[_clientId]);
        }
    }

    /// <summary>Sends the client into the game and informs other clients of the new player.</summary>
    /// <param name="_playerName">The username of the new player.</param>
    public void SendIntoGame(string _playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        //player = InstantiateTools.instance.InstantiatePlayer();
        player.Initialize(id, _playerName);

        // Send all players to the new player
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player, _client.player.currentGun.name);
                    ServerSend.UpdatePlayerKillStats(_client.id, _client.player.currentKills);
                    ServerSend.UpdatePlayerDeathStats(_client.id, _client.player.currentDeaths);
                }
            }
        }

        // Send the new player to all players (including himself)
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player, player.currentGun.name);
            }
        }

        // Send environment to new player
        foreach (GameObject _planet in EnvironmentGeneratorServerSide.planets.Values)
        {
            ServerSend.CreateNewPlanet(id, _planet.transform.position, _planet.transform.localScale, Server.clients[id].player.gravityMaxDistance);
        }
        foreach(GameObject _object in EnvironmentGeneratorServerSide.nonGravityObjectDict.Values)
        {
            ServerSend.CreateNonGravityObject(id, _object.transform.position, _object.transform.localScale,
                                              _object.transform.rotation , _object.name);
        }

        ServerSend.CreateBoundary(id, Vector3.zero, EnvironmentGeneratorServerSide.BoundaryDistanceFromOrigin);
    }

    /// <summary>Sends the client into the game and informs other clients of the new player.</summary>
    /// <param name="_playerName">The username of the new player.</param>
    public void SendIntoGameFreeForAll()
    {
        player = NetworkManager.instance.InstantiatePlayer();
        //player = InstantiateTools.instance.InstantiatePlayer();
        player.Initialize(id, userName);

        // Send all players to the new player
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player, _client.player.currentGun.name);
                    ServerSend.UpdatePlayerKillStats(_client.id, _client.player.currentKills);
                    ServerSend.UpdatePlayerDeathStats(_client.id, _client.player.currentDeaths);
                }
            }
        }

        // Send the new player to all players (including himself)
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player, player.currentGun.name);
            }
        }

        // Send environment to new player
        foreach (GameObject _planet in EnvironmentGeneratorServerSide.planets.Values)
        {
            ServerSend.CreateNewPlanet(id, _planet.transform.position, _planet.transform.localScale, Server.clients[id].player.gravityMaxDistance);
        }
        foreach(GameObject _object in EnvironmentGeneratorServerSide.nonGravityObjectDict.Values)
        {
            ServerSend.CreateNonGravityObject(id, _object.transform.position, _object.transform.localScale,
                                              _object.transform.rotation , _object.name);
        }

        ServerSend.CreateBoundary(id, Vector3.zero, EnvironmentGeneratorServerSide.BoundaryDistanceFromOrigin);
    }

    /// <summary>Disconnects the client and stops all network traffic.</summary>
    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        ThreadManagerServerSide.ExecuteOnMainThread(() =>
        {
            if(player != null)
                UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnect(id);
    }
}