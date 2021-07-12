using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPlayerFFA : MonoBehaviour
{
    #region Variables

    public int id;
    public string username;
    public Rigidbody rb;
    public Transform orientation;
    public SPlayer player;

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
    public float gravityMaxDistance = 50;
    public float gravityForce = 100;
    public float maxDistanceFromOrigin = 600, forceBackToOrigin = 4500f;

    // JetPack 
    public float jetPackForce = 20;
    public bool isJetPackRecoveryActive = true;
    public float maxJetPackPower = 2.1f;
    public float currentJetPackPower;
    public float jetPackBurstCost = .5f;
    public float jetPackRecoveryIncrementor = .1f;

    // Grappling Variables 
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

    #region Set up

    public void Initialize(int _id, string _username, SPlayer _player)
    {
        id = _id;
        username = _username;
        player = _player;

        SetGunInformation();
    }

    public void SetGunInformation()
    {
        allGunInformation["Pistol"] = new GunInformation
        {
            name = "Pistol",
            magSize = 6,
            ammoIncrementor = 60,
            reserveAmmo = 60,
            currentAmmo = 6,
            damage = 30,
            fireRate = .7f,
            accuaracyOffset = 0,
            reloadTime = 1f,
            range = 1000f,
            rightHandPosition = -.3f,
            leftHandPosition = -1.5f,
            isAutomatic = false,
        };

        allGunInformation["SMG"] = new GunInformation
        {
            name = "SMG",
            magSize = 30,
            ammoIncrementor = 300,
            reserveAmmo = 300,
            currentAmmo = 30,
            damage = 10,
            fireRate = .1f,
            accuaracyOffset = .025f,
            reloadTime = 1f,
            range = 1000f,
            rightHandPosition = -.3f,
            leftHandPosition = -1.5f,
            isAutomatic = true,
        };

        allGunInformation["AR"] = new GunInformation
        {
            name = "AR",
            magSize = 20,
            ammoIncrementor = 260,
            reserveAmmo = 260,
            currentAmmo = 20,
            damage = 20,
            fireRate = .2f,
            accuaracyOffset = .02f,
            reloadTime = 1f,
            range = 1000f,
            rightHandPosition = -.3f,
            leftHandPosition = -1.5f,
            isAutomatic = true,
        };

        allGunInformation["Shotgun"] = new GunInformation
        {
            name = "Shotgun",
            magSize = 8,
            ammoIncrementor = 80,
            reserveAmmo = 80,
            currentAmmo = 8,
            damage = 10,
            fireRate = .7f,
            accuaracyOffset = .1f,
            reloadTime = 1f,
            range = 75f,
            rightHandPosition = -.3f,
            leftHandPosition = -.3f,
            isAutomatic = false,
        };

        int index = 0;
        gunNames = new string[allGunInformation.Count];
        foreach (string str in allGunInformation.Keys)
        {
            gunNames[index] = str;
            index++;
        }

        do
        {
            currentGun = allGunInformation[gunNames[Random.Range(0, gunNames.Length)]];
            secondaryGun = allGunInformation[gunNames[Random.Range(0, gunNames.Length)]];
        } while (currentGun.name.Equals(secondaryGun.name));
    }


    private void Start()
    {
        maxDistanceFromOrigin = EnvironmentGeneratorServerSide.BoundaryDistanceFromOrigin;
        currentJetPackPower = maxJetPackPower;
        timeLeftToGrapple = maxGrappleTime;
        grappleTimeLimiter = maxGrappleTime / 4;
    }

    #endregion

    #region Update and Input

    private void FixedUpdate()
    {
        SendPlayerData();
        GravityController();

        if (isGrounded)
            Movement();
        else if (moveDirection.x > 0)
            JetPackThrust(orientation.right);
        else if (moveDirection.x < 0)
            JetPackThrust(-orientation.right);
        else if (moveDirection.y > 0)
            JetPackThrust(orientation.forward);
        else if (moveDirection.y < 0)
            JetPackThrust(-orientation.forward);
        if (IsGrappling)
        {
            if (timeLeftToGrapple > 0)
                ContinueGrapple();
            else
                StopGrapple();
        }

        if (currentJetPackPower < maxJetPackPower)
        {
            currentJetPackPower += jetPackRecoveryIncrementor;
            ServerSend.PlayerContinueJetPackFFA(id, currentJetPackPower);
        }
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("GravityObject"))
        {
            rb.velocity /= 2;
        }
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetMovementInput(Vector2 _moveDirection, Quaternion _rotation)
    {
        moveDirection = _moveDirection;

        orientation.localRotation = _rotation;
    }

    /// <summary>
    /// Updates whether player is currently in an animation
    /// </summary>
    /// <param name="_isAnimInProgress"></param>
    public void SetActionInput(bool _isAnimInProgress)
    {
        isAnimInProgress = _isAnimInProgress;
    }
    #endregion

    #region Movement

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Movement()
    {
        rb.AddForce(orientation.forward * moveDirection.y * moveSpeed * Time.deltaTime);
        rb.AddForce(orientation.right * moveDirection.x * moveSpeed * Time.deltaTime);

        // Stick to ground object
        Collider[] _groundCollider = Physics.OverlapSphere(transform.position, groundDistance * 2, whatIsGround);
        rb.AddForce((_groundCollider[0].transform.position - transform.position) * gravityForce * 3 * Time.deltaTime);
    }

    #endregion

    #region JetPack

    /// <summary>
    /// Adds impulse force to player in the form of a powerful thrust
    /// </summary>
    /// <param name="_direction"> direction to add force </param>
    public void JetPackThrust(Vector3 _direction)
    {
        if (currentJetPackPower < jetPackBurstCost) return;
        currentJetPackPower -= jetPackBurstCost;

        rb.AddForce(_direction * jetPackForce * 50 * Time.deltaTime, ForceMode.Impulse);
    }

    /// <summary>
    /// Adds impulse force to player in the form of a continuous force
    /// </summary>
    /// <param name="_direction"> direction to add force </param>
    public void JetPackMovement(Vector3 _direction)
    {
        if (isGrounded)
            rb.AddForce(_direction * jetPackForce * 5 * Time.deltaTime, ForceMode.Impulse);
        //ServerSend.PlayerContinueJetPack(id, currentJetPackTime);
        else
            rb.AddForce(_direction * jetPackForce * Time.deltaTime, ForceMode.Impulse);
    }
    #endregion

    #region Artificial Gravity

    /// <summary>
    /// Decides whether gravity should be applied and applies it
    /// </summary>
    public void GravityController()
    {
        // if player is beyond boundary. Add force to pull player to origin
        if (transform.position.magnitude > maxDistanceFromOrigin)
        {
            rb.AddForce(-transform.position.normalized * forceBackToOrigin * Time.deltaTime);
            return;
        }

        if (IsGrappling) return;    // Do not apply gravity is player is grappling

        Transform[] _gravityObjects = FindGravityObjects();

        for (int i = 0; i < _gravityObjects.Length; i++)
        {
            if (_gravityObjects[i] != null)
            {
                ApplyGravity(_gravityObjects[i]);   // Apply gravity for each gravity object in range
            }
        }
    }

    /// <summary>
    /// Finds the transforms of all gravity objects in gravityMaxDistance distance
    /// </summary>
    /// <returns>returns all the transforms of all gravity objects in gravityMaxDistance distance </returns>
    public Transform[] FindGravityObjects()
    {
        int index = 0;

        Collider[] _gravityObjectColiiders = Physics.OverlapSphere(transform.position, gravityMaxDistance, whatIsGravityObject);
        Transform[] _gravityObjects = new Transform[_gravityObjectColiiders.Length];
        foreach (Collider _gravityObjectCollider in _gravityObjectColiiders)
        {
            _gravityObjects[index] = _gravityObjectCollider.transform;
            index++;
        }

        return _gravityObjects;
    }

    /// <summary>
    /// add force to player relative to _gravityObject position
    /// </summary>
    /// <param name="_gravityObject"> gravity object to pull player towards </param>
    public void ApplyGravity(Transform _gravityObject)
    {
        rb.AddForce((_gravityObject.position - transform.position) * gravityForce * Time.deltaTime);

        if (isGrounded)
        {
            // Rotate Player to stand straight on gravity object
            Quaternion desiredRotation = Quaternion.FromToRotation(_gravityObject.up, -(_gravityObject.position - transform.position).normalized);
            desiredRotation = Quaternion.Lerp(transform.localRotation, desiredRotation, Time.deltaTime * 2);
            transform.localRotation = desiredRotation;
            // Add extra force to stick player to planet surface
            rb.AddForce((_gravityObject.position - transform.position) * gravityForce * 1 * Time.deltaTime);
        }
    }

    #endregion

    #region Magnetize 

    /// <summary>
    /// Adds force to player to have player go to the nearest gravity relative to player
    /// </summary>
    public void PlayerMagnetize()
    {
        rb.velocity = Vector3.zero;
        Vector3 _desiredPosition = FindNearestGravityObjectPosition();

        rb.AddForce((_desiredPosition - transform.position) * magnetizeForce * Time.deltaTime, ForceMode.Impulse);
    }

    /// <summary>
    /// Finds the nearest gravity object position
    /// </summary>
    /// <returns> a vector3 of the position of the nearest gravity object </returns>
    public Vector3 FindNearestGravityObjectPosition()
    {
        float _checkingDistance = 100, _errorCatcher = 0;
        Collider[] _gravityObjectColiiders;
        do
        {
            _gravityObjectColiiders = Physics.OverlapSphere(transform.position, _checkingDistance, whatIsGravityObject);
            _checkingDistance += 500;
            _errorCatcher++;
            if (_errorCatcher > 10)
                return Vector3.zero; // Return to sender (Origin) (This prevents infinite loop )
        } while (_gravityObjectColiiders.Length == 0);

        Transform _nearestGravityObject = _gravityObjectColiiders[0].transform;
        float _lastDistance = 100000;   // garbage value
        // Find closest gravity object
        foreach (Collider _gravityObject in _gravityObjectColiiders)
        {
            if (Vector3.Distance(_gravityObject.transform.position, transform.position) < _lastDistance)
            {
                _lastDistance = Vector3.Distance(_gravityObject.transform.position, transform.position);
                _nearestGravityObject = _gravityObject.transform;
            }
        }
        return _nearestGravityObject.transform.position;
    }

    #endregion

    #region Weapons

    /// <summary>
    /// Controls how player should shoot
    /// </summary>
    /// <param name="_firePoint"> player's current fire point </param>
    /// <param name="_fireDirection"> player's current fire direction </param>
    public void ShootController(Vector3 _firePoint, Vector3 _fireDirection)
    {
        if (isAnimInProgress) return;

        firePoint = _firePoint;
        fireDirection = _fireDirection;

        if (!isShooting)
        {
            if (currentGun.isAutomatic)
                StartAutomaticFire();
            else
                SingleFireShoot();
        }
    }

    /// <summary>
    /// Updates player fire point and fire direction
    /// </summary>
    /// <param name="_firePoint"> player's fire point </param>
    /// <param name="_fireDirection"> player's fire direction </param>
    public void UpdateShootDirection(Vector3 _firePoint, Vector3 _fireDirection)
    {
        firePoint = _firePoint;
        fireDirection = _fireDirection;
    }

    /// <summary>
    /// Stops automatic shot
    /// </summary>
    public void StopShootContoller()
    {
        StopAutomaticShoot();
    }

    /// <summary>
    /// Shoots a single raycast and damages if it hits anything that is damagable
    /// </summary>
    public void SingleFireShoot()
    {
        isAnimInProgress = true;
        currentGun.currentAmmo--;
        ServerSend.PlayerSingleFireFFA(id, currentGun.currentAmmo, currentGun.reserveAmmo);
        // Reduce accuracy by a certain value 
        Vector3 reduceAccuracy = fireDirection + new Vector3(Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset),
                                                                Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset));

        // Reload if current ammo is zero
        if (currentGun.currentAmmo <= 0)
        {
            if (currentGun.reserveAmmo > 0)
                Reload();
        }

        if (currentGun.name == "Pistol")
        {
            Ray ray = new Ray(firePoint, reduceAccuracy);
            if (Physics.Raycast(ray, out RaycastHit _hit, currentGun.range, whatIsShootable))
            {
                ServerSend.PlayerShotLandedFFA(id, _hit.point);
                if (_hit.collider.CompareTag("Player"))
                    _hit.collider.GetComponent<SPlayerFFA>().TakeDamage(id, currentGun.damage);
            }
        }
        else     // Shotgun
        {
            Vector3 trajectory;
            for (int i = 0; i < 10; i++)
            {
                trajectory = fireDirection + new Vector3(Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset),
                                                                Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset),
                                                                Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset));
                Ray ray = new Ray(firePoint, trajectory);
                if (Physics.Raycast(ray, out RaycastHit _hit, currentGun.range, whatIsShootable))
                {
                    ServerSend.PlayerShotLandedFFA(id, _hit.point);
                    if (_hit.collider.CompareTag("Player"))
                        _hit.collider.GetComponent<SPlayerFFA>().TakeDamage(id, currentGun.damage);
                }
            }
        }
        // Reload if current ammo is zero
        if (currentGun.currentAmmo <= 0)
        {
            if (currentGun.reserveAmmo > 0)
                Reload();
        }
    }

    /// <summary>
    /// Starts automatic fire w/ invoke repeating
    /// </summary>
    public void StartAutomaticFire()
    {
        isShooting = true;
        ServerSend.PlayerStartAutomaticFireFFA(id, currentGun.currentAmmo, currentGun.reserveAmmo);
        InvokeRepeating("AutomaticShoot", 0f, currentGun.fireRate);
    }

    /// <summary>
    /// Shoots a single raycast and damages if it hits anything that is damagable
    /// </summary>
    public void AutomaticShoot()
    {
        // Reduce accuracy by a certain value 
        currentGun.currentAmmo--;
        ServerSend.PlayerContinueAutomaticFireFFA(id, currentGun.currentAmmo, currentGun.reserveAmmo);
        Vector3 reduceAccuracy = fireDirection + new Vector3(Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset),
                                                                Random.Range(-currentGun.accuaracyOffset, currentGun.accuaracyOffset));


        // Reload if current ammo is zero
        if (currentGun.currentAmmo <= 0)
        {
            StopAutomaticShoot();
            if (currentGun.reserveAmmo > 0)
                Reload();
        }

        Ray ray = new Ray(firePoint, reduceAccuracy);
        if (Physics.Raycast(ray, out RaycastHit _hit, currentGun.range, whatIsShootable))
        {
            ServerSend.PlayerShotLandedFFA(id, _hit.point);
            if (_hit.collider.CompareTag("Player"))
                _hit.collider.GetComponent<SPlayerFFA>().TakeDamage(id, currentGun.damage);
        }
    }

    /// <summary>
    /// Stops automatic shoting with CancelInvoke
    /// </summary>
    public void StopAutomaticShoot()
    {
        ServerSend.PlayerStopAutomaticFireFFA(id);
        CancelInvoke("AutomaticShoot");
        isShooting = false;
    }

    /// <summary>
    /// Reloads current weapon
    /// </summary>
    public void Reload()
    {
        if (isShooting && currentGun.isAutomatic)
            StopAutomaticShoot();

        // Reload gun
        if (currentGun.reserveAmmo > currentGun.magSize)
        {
            currentGun.reserveAmmo += -currentGun.magSize + currentGun.currentAmmo;
            currentGun.currentAmmo = currentGun.magSize;
        }
        else
        {
            if (currentGun.magSize - currentGun.currentAmmo <= currentGun.reserveAmmo)
            {
                currentGun.reserveAmmo -= currentGun.magSize - currentGun.currentAmmo;
                currentGun.currentAmmo = currentGun.magSize;
            }
            else
            {
                currentGun.currentAmmo += currentGun.reserveAmmo;
                currentGun.reserveAmmo = 0;
            }
        }
        ServerSend.PlayerReloadFFA(id, currentGun.currentAmmo, currentGun.reserveAmmo);
    }

    /// <summary>
    /// Switches current weapon
    /// </summary>
    public void SwitchWeapon()
    {
        StopAutomaticShoot();
        isAnimInProgress = true;

        GunInformation temp = currentGun;
        currentGun = secondaryGun;
        secondaryGun = temp;

        ServerSend.PlayerSwitchWeaponFFA(id, currentGun.name, currentGun.currentAmmo, currentGun.reserveAmmo);
        // Send to all clients this player has switched it weapon
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.OtherPlayerSwitchedWeaponFFA(id, _client.id, currentGun.name);
                }
            }
        }
    }

    #endregion

    #region Grapple

    /// <summary>
    /// Turns off gravity, creates joints for grapple
    /// Call whenever player inputs for grapple
    /// Dependencies: DrawRope
    /// </summary>
    public void StartGrapple(Vector3 _direction)
    {
        if (timeLeftToGrapple < grappleTimeLimiter)
            return;
        Ray ray = new Ray(transform.position, _direction);
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, whatIsGrapple))
        {
            if (Vector3.Distance(transform.position, hit.point) < minGrappleDistance)
                return;

            if (IsGrappleRecoveryInProgress)
            {
                IsGrappleRecoveryInProgress = false;
                CancelInvoke("GrappleRecovery");
            }


            IsGrappling = true;

            // Create joint ("Grapple rope") and anchor to player and grapple point
            GrapplePoint = hit.point;
            if (joint != null)
                Destroy(joint);
            joint = transform.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = GrapplePoint;

            joint.spring = 0f;
            joint.damper = 0f;
            joint.massScale = 0f;

            ServerSend.PlayerStartGrappleFFA(id);
        }
    }

    /// <summary>
    /// Call every frame while the player is grappling
    /// </summary>
    public void ContinueGrapple()
    {
        timeLeftToGrapple -= Time.deltaTime;
        ServerSend.PlayerContinueGrappleFFA(id, timeLeftToGrapple);
        if (timeLeftToGrapple < 0)
            StopGrapple();

        // Pull player to grapple point
        Vector3 direction = (GrapplePoint - transform.position).normalized;
        rb.AddForce(direction * 50 * Time.deltaTime, ForceMode.Impulse);

        // Prevent grapple from phasing through/into objectsz
        // (Game objects such as buildings must have a rotation for this section to work)
        if (Physics.Raycast(GrapplePoint, (transform.position - GrapplePoint), Vector3.Distance(GrapplePoint, transform.position) - 5, whatIsGrapple))
            StopGrapple();
    }

    /// <summary>
    /// Erases grapple rope, turns player gravity on, and destroys joint
    /// </summary>
    public void StopGrapple()
    {
        if (!IsGrappleRecoveryInProgress)
        {
            IsGrappleRecoveryInProgress = true;
            InvokeRepeating("GrappleRecovery", 0f, .1f);
        }
        IsGrappling = false;
        Destroy(joint);
        ServerSend.PlayerStopGrappleFFA(id);
    }


    /// <summary>
    /// adds time to player's amount of grapple left. Must be called through invoke repeating with
    /// a repeat time of .1 seconds of scaled time
    /// </summary>
    public void GrappleRecovery()
    {
        if (timeLeftToGrapple <= maxGrappleTime)
        {
            timeLeftToGrapple += grappleRecoveryIncrement;
        }
        else
            CancelInvoke("GrappleRecovery");
        ServerSend.PlayerContinueGrappleFFA(id, timeLeftToGrapple);
    }

    #endregion

    /// <summary>
    /// Sends player position and rotation to all cleints
    /// </summary>
    public void SendPlayerData()
    {
        ServerSend.PlayerPositionFFA(player);
        ServerSend.PlayerRotationFFA(player, orientation.localRotation);
    }

    /// <summary>
    /// Take damage and update health and update stats
    /// </summary>
    /// <param name="_fromId"> Client that called this function </param>
    /// <param name="_damage"> Value to subtract from health </param>
    public void TakeDamage(int _fromId, float _damage)
    {
        if (player.health <= 0)
            return;

        player.health -= _damage;

        if (player.health <= 0)
        {
            player.health = 0;
            player.currentDeaths++;
            ServerSend.UpdatePlayerDeathStats(id, player.currentDeaths);
            Server.clients[_fromId].player.currentKills++;
            ServerSend.UpdatePlayerKillStats(_fromId, Server.clients[_fromId].player.currentKills);
            // Teleport to random spawnpoint
            transform.position = EnvironmentGeneratorServerSide.spawnPoints[
                                 Random.Range(0, EnvironmentGeneratorServerSide.spawnPoints.Count)];
            ServerSend.PlayerPositionFFA(player);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealthFFA(player);
    }

    /// <summary>
    /// Respawns player after 5 seconds
    /// </summary>
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        player.health = player.maxHealth;
        ServerSend.PlayerRespawnedFFA(player);
    }
}
