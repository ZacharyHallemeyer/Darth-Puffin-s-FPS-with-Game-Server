using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SInfectionGameManager : MonoBehaviour
{
    public static Dictionary<int, SPlayer> humans = new Dictionary<int, SPlayer>();
    public static Dictionary<int, SPlayer> infected = new Dictionary<int, SPlayer>();

    public static void CheckHumansLeft()
    {
        if(humans.Count <= 0)
        {
            //TODO 
              // Restart game or send players to lobby
        }
    }
}
