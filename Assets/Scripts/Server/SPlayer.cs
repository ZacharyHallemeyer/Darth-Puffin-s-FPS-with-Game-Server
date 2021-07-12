using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPlayer : MonoBehaviour
{
    #region Variables
    // Generic variables
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100;

    // Possible player scripts
    public SPlayerFFA sPlayerFFA;
    public SPlayerInfection sPlayerInfection;

    // Stats
    public int currentKills = 0;
    public int currentDeaths = 0;

    #endregion

    /// <summary>
    /// Inits new player info
    /// </summary>
    /// <param name="_id"> Client id </param>
    /// <param name="_username"> Client username </param>
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }
}