using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SPlayer : MonoBehaviour
{
    #region Variables
    // Generic variables
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100;
    public Rigidbody rb;
    public Transform orientation;

    // Possible player scripts
    public SPlayerFFA sPlayerFFA;
    public SPlayerInfection sPlayerInfection;

    // Stats
    public int currentKills = 0;
    public int currentDeaths = 0;

    // Input
    private Vector2 moveDirection;

    // Movement variables
    private readonly int moveSpeed = 4500;
    public LayerMask whatIsGround;
    public bool isGrounded;

    // Ground Check
    public Transform groundCheck;
    public float groundDistance = 1;

    // Gravity variables (Disable use gravity for rigid body)
    public LayerMask whatIsGravityObject;
    public float gravityMaxDistance = 500;
    public float gravityForce = 4500;
    public float maxDistanceFromOrigin = 600, forceBackToOrigin = 4500f;
    public Quaternion lastOrientationRotation;

    // JetPack 
    public float jetPackForce = 20;
    public bool isJetPackRecoveryActive = true;
    public float maxJetPackPower = 2.1f;
    public float currentJetPackPower;
    public float jetPackBurstCost = .5f;
    public float jetPackRecoveryIncrementor = .1f;

    // Grappling Variables ==================
    // Components
    private SpringJoint joint;
    public LayerMask whatIsGrapple;

    // Numerical variables
    private float maxGrappleDistance = 10000f, minGrappleDistance = 5f;
    public float maxGrappleTime = 1000f, grappleRecoveryIncrement = 50f;
    public float timeLeftToGrapple, grappleTimeLimiter;

    public bool IsGrappleRecoveryInProgress { get; set; } = false;
    public bool IsGrappling { get; private set; }
    public Vector3 GrapplePoint { get; private set; }

    // Magnetize
    public int magnetizeForce = 100;

    // Gun Variables

    public class GunInformation
    {
        public string name;
        public GameObject gunContainer;
        public ParticleSystem bullet;
        public ParticleSystem gun;
        public float originalGunRadius;
        public int magSize;
        public int ammoIncrementor;
        public int reserveAmmo;
        public int currentAmmo;
        public float damage;
        public float fireRate;
        public float accuaracyOffset;
        public float reloadTime;
        public float range;
        public float rightHandPosition;
        public float leftHandPosition;
        public bool isAutomatic;
    }

    public Dictionary<string, GunInformation> allGunInformation { get; private set; } = new Dictionary<string, GunInformation>();

    // Guns
    public GunInformation currentGun;
    public GunInformation secondaryGun;

    public string[] gunNames;

    public LayerMask whatIsShootable;
    public bool isShooting = false;

    private Vector3 firePoint;
    private Vector3 fireDirection;

    public bool isAnimInProgress;

    #endregion

    private void Start()
    {
        maxDistanceFromOrigin = EnvironmentGeneratorServerSide.BoundaryDistanceFromOrigin;
        currentJetPackPower = maxJetPackPower;
        timeLeftToGrapple = maxGrappleTime;
        grappleTimeLimiter = maxGrappleTime / 4;
    }

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

    #region Stats and Generic

    /// <summary>
    /// Take damage and update health and update stats
    /// </summary>
    /// <param name="_fromId"> Client that called this function </param>
    /// <param name="_damage"> Value to subtract from health </param>
    public void TakeDamage(int _fromId, float _damage)
    {
        if (health <= 0)
            return;

        health -= _damage;

        if (health <= 0)
        {
            health = 0;
            currentDeaths++;
            ServerSend.UpdatePlayerDeathStats(id, currentDeaths);
            Server.clients[_fromId].player.currentKills++;
            ServerSend.UpdatePlayerKillStats(_fromId, Server.clients[_fromId].player.currentKills);
            // Teleport to random spawnpoint
            transform.position = EnvironmentGeneratorServerSide.spawnPoints[
                                 Random.Range(0, EnvironmentGeneratorServerSide.spawnPoints.Count)];
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    /// <summary>
    /// Respawns player after 5 seconds
    /// </summary>
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        ServerSend.PlayerRespawned(this);
    }

    #endregion

    /// <summary>
    /// Sends player position and rotation to all cleints
    /// </summary>
    public void SendPlayerData()
    {
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this, orientation.localRotation);
    }
}