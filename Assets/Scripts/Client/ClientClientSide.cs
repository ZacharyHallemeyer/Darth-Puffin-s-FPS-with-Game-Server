using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System;

public class ClientClientSide : MonoBehaviour
{
    public static ClientClientSide instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(PacketClientSide _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    public static Dictionary<int, string> allClients = new Dictionary<int, string>();

    public Lobby lobby;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        DontDestroyOnLoad(this);
        ip = PlayerPrefs.GetString("HostIP", "127.0.0.1");
        instance.ConnectToServer();
    }

    /// <summary>
    /// Disconnect from server
    /// </summary>
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();

        int _sceneCount = SceneManager.sceneCount;
        string _sceneName = "";

        for (int i = 0; i < _sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name[0] == 'S' || SceneManager.GetSceneAt(i).name[0] == 'C')
                _sceneName = SceneManager.GetSceneAt(i).name.Substring(6);
        }
        switch (_sceneName)
        {
            case "FreeForAll":
                ChangeClientDataToFreeForAll();
                break;
            case "Infection":
                ChangeClientDataToInfection();
                break;
            default:
                InitializeClientData();
                break;
        }

        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private PacketClientSide receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new PacketClientSide();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(PacketClientSide _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManagerClientSide.ExecuteOnMainThread(() =>
                {
                    using (PacketClientSide _packet = new PacketClientSide(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (PacketClientSide _packet = new PacketClientSide())
            {
                SendData(_packet);
            }
        }

        public void SendData(PacketClientSide _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (PacketClientSide _packet = new PacketClientSide(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManagerClientSide.ExecuteOnMainThread(() =>
            {
                using (PacketClientSide _packet = new PacketClientSide(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            socket = null;
            endPoint = null;
        }
    }

    /// <summary>
    /// Inits all client data from server
    /// </summary>
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.addClient, ClientHandle.AddClient },
        };
        Debug.Log("Initialized packets.");
    }

    public void ChangeClientDataToFreeForAll()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.addClient, ClientHandle.AddClient },
            { (int)ServerPackets.environmentReady, ClientHandle.EnvironmentReadyFreeForAll },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayerFFA },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPositionFFA },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotationFFA },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnectedFFA },
            { (int)ServerPackets.otherPlayerTakenDamage, ClientHandle.OtherPlayerTakenDamageFFA },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealthFFA },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawnedFFA },
            { (int)ServerPackets.createNewPlanet, ClientHandle.CreateNewPlanetFFA },
            { (int)ServerPackets.createNewNonGravityObject, ClientHandle.CreateNewNonGravityObjectFFA },
            { (int)ServerPackets.createBoundary, ClientHandle.CreateBoundaryFFA },
            { (int)ServerPackets.playerStartGrapple, ClientHandle.PlayerStartGrappleFFA },
            { (int)ServerPackets.playerContinueGrapple, ClientHandle.PlayerContinueGrappleFFA },
            { (int)ServerPackets.otherPlayerContinueGrapple, ClientHandle.OtherPlayerContinueGrappleFFA },
            { (int)ServerPackets.otherPlayerStopGrapple, ClientHandle.OtherPlayerStopGrappleFFA },
            { (int)ServerPackets.playerStopGrapple, ClientHandle.PlayerStopGrappleFFA },
            { (int)ServerPackets.otherPlayerSwitchedWeapon, ClientHandle.OtherPlayerSwitchedWeaponFFA },
            { (int)ServerPackets.playerSinglefire, ClientHandle.PlayerSingleFireFFA },
            { (int)ServerPackets.playerStartAutomaticFire, ClientHandle.PlayerStartAutomaticFireFFA },
            { (int)ServerPackets.playerContinueAutomaticFire, ClientHandle.PlayerContinueAutomaticFireFFA },
            { (int)ServerPackets.playerStopAutomaticFire, ClientHandle.PlayerStopAutomaticFireFFA },
            { (int)ServerPackets.playerReload, ClientHandle.PlayerReloadFFA },
            { (int)ServerPackets.playerSwitchWeapon, ClientHandle.PlayerSwitchWeaponFFA },
            { (int)ServerPackets.playerShotLanded, ClientHandle.PlayerShotLandedFFA },
            { (int)ServerPackets.playerContinueJetPack, ClientHandle.PlayerContinueJetPackFFA },
            { (int)ServerPackets.updatePlayerKillStats, ClientHandle.UpdatePlayerKillStats },
            { (int)ServerPackets.updatePlayerDeathStats, ClientHandle.UpdatePlayerDeathStats },
        };
        Debug.Log("Initialized Client Free For All Packets.");
    }

    public void ChangeClientDataToInfection()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.addClient, ClientHandle.AddClient },
            { (int)ServerPackets.environmentReady, ClientHandle.EnvironmentReadyInfection },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayerInfection },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPositionInfection },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotationInfection },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnectedInfection },
            { (int)ServerPackets.otherPlayerTakenDamage, ClientHandle.OtherPlayerTakenDamageInfection },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealthInfection },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawnedInfection },
            { (int)ServerPackets.otherPlayerSwitchedWeapon, ClientHandle.OtherPlayerSwitchedWeaponInfection },
            { (int)ServerPackets.playerSinglefire, ClientHandle.PlayerShootInfection },
            { (int)ServerPackets.playerReload, ClientHandle.PlayerReloadInfection },
            { (int)ServerPackets.playerSwitchWeapon, ClientHandle.PlayerSwitchWeaponInfection },
            { (int)ServerPackets.playerShotLanded, ClientHandle.PlayerShotLandedInfection },
            { (int)ServerPackets.playerStartWallrun, ClientHandle.PlayerStartWallRunInfection },
            { (int)ServerPackets.playerContinueWallrun, ClientHandle.PlayerContinueWallRunInfection },
            { (int)ServerPackets.playerStopWallrun, ClientHandle.PlayerStopWallRunInfection },
            { (int)ServerPackets.updatePlayerKillStats, ClientHandle.UpdatePlayerKillStatsInfection },
            { (int)ServerPackets.updatePlayerDeathStats, ClientHandle.UpdatePlayerDeathStatsInfection },
            { (int)ServerPackets.playerStartCrouch, ClientHandle.PlayerStartCrouchInfection },
            { (int)ServerPackets.playerStopCrouch, ClientHandle.PlayerStopCrouchInfection },
            { (int)ServerPackets.createBuilding, ClientHandle.CreateNewBulding },
            { (int)ServerPackets.createNewPlanet, ClientHandle.CreateNewSun },
        };
        Debug.Log("Initialized Client Infection Packets.");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Diconnected from server.");
        }
    }
}