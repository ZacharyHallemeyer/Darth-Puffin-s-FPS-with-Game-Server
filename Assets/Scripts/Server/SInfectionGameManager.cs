using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SInfectionGameManager : MonoBehaviour
{
    public static Dictionary<int, SPlayerInfection> humans = new Dictionary<int, SPlayerInfection>();
    public static Dictionary<int, SPlayerInfection> infected = new Dictionary<int, SPlayerInfection>();

    public static void CheckHumansLeft()
    {

        if(humans.Count <= 0)
        {
            ResetGame();
        }
    }

    public static void ResetGame()
    {
        humans = new Dictionary<int, SPlayerInfection>();
        infected = new Dictionary<int, SPlayerInfection>();

        int[] _infectedIds = ChooseInfectedIds(1, ClientServerSide.allClients.Keys.ToArray());
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if(_client.player != null)
            {
                bool _isInfected = false;
                for (int i = 0; i < _infectedIds.Length; i++)
                {
                    if (_infectedIds[i] == _client.id)
                        _isInfected = true;
                }
                _client.player.transform.position = InfectionEnvironmentGenerator.spawnPoints[
                                                    Random.Range(0, InfectionEnvironmentGenerator.spawnPoints.Count)];
                _client.player.Initialize(_client.player.id, _client.player.username);
                _client.sPlayerInfection.Initialize(_client.player.id, _client.player.username, _client.player, _isInfected);
                _client.sPlayerInfection.SwitchWeapon();

                if (_isInfected)
                    infected.Add(_client.id, Server.clients[_client.id].sPlayerInfection);
                else
                    humans.Add(_client.id, Server.clients[_client.id].sPlayerInfection);
            }
        }
    }

    public static int[] ChooseInfectedIds(int _amount, int[] _ids)
    {
        int[] _infectedIds = new int[_amount];
        for(int i = 0; i < _amount; i++)
        {
            _infectedIds[i] = _ids[Random.Range(0, _ids.Length)];
        }
        return _infectedIds;
    }
}
