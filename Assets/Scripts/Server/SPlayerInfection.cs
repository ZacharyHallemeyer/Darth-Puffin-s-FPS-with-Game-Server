using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPlayerInfection : MonoBehaviour
{
    #region Variables

    public int id;
    public string username;
    public Rigidbody rb;
    public Transform orientation;
    public SPlayer player;

    // Input
    private Vector2 moveDirection;

    //Movement
    private readonly int moveSpeed = 4500;
    private readonly int crouchMoveSpeed = 3000;
    private readonly int maxBaseSpeed = 20;
    private readonly int maxCrouchSpeed = 10;
    private readonly float counterMovement = 0.175f;
    private readonly float threshold = 0.01f;
    private bool shouldSlide;
    public LayerMask whatIsGround;
    public bool isGrounded;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 1, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    public float crouchGunDegree = 50;
    public bool isCrouching;

    public bool isSliding = false;

    //Jumping
    public float jumpForce = 550f;
    private readonly int maxJumps = 2;
    private int jumpsAvaliable = 2;
    public bool isJumping;

    // Ground Check
    public Transform groundCheck;
    public float groundDistance = 1;

    //Wallrunning
    public LayerMask whatIsWall;
    public Transform wallCheck;
    private readonly float wallDistance = 3f;
    public float wallrunForce;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    private float currentHandPosition = .15f;
    private float wallRunSpeed = 100f, maxWallRunSpeed = 40f;
    public float timeOnWall = 0, maxtimeOnWall = 3f;
    public bool isWallRight, isWallLeft, isWallForward, isWallBackward, isOnWall;
    public Transform gunPosition;

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
        playerScale = transform.localScale;

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
        };

        int index = 0;
        gunNames = new string[allGunInformation.Count];
        foreach (string str in allGunInformation.Keys)
        {
            gunNames[index] = str;
            index++;
        }
        currentGun = allGunInformation["Shotgun"];
        secondaryGun = allGunInformation["Pistol"];
    }

    #endregion

    #region Update and Input

    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
        CheckForWall();
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y/2, rb.velocity.z);
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

    #region Ground and Air Movement

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Movement()
    {
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        if (isJumping) return;
        // Reset jumps if player is on ground
        if (isGrounded) jumpsAvaliable = maxJumps;

        //Extra gravity

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook(orientation, rb);
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        if (!isCrouching)
            CounterMovement(moveDirection.x, moveDirection.y, mag);

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (isCrouching)
        {
            if (moveDirection.x > 0 && xMag > maxCrouchSpeed) moveDirection.x = 0;
            if (moveDirection.x < 0 && xMag < -maxCrouchSpeed) moveDirection.x = 0;
            if (moveDirection.y > 0 && yMag > maxCrouchSpeed) moveDirection.y = 0;
            if (moveDirection.y < 0 && yMag < -maxCrouchSpeed) moveDirection.y = 0;
        }
        else if (isOnWall)
        {
            if (moveDirection.x > 0 && xMag > maxWallRunSpeed) moveDirection.x = 0;
            if (moveDirection.x < 0 && xMag < -maxWallRunSpeed) moveDirection.x = 0;
            if (moveDirection.y > 0 && yMag > maxWallRunSpeed) moveDirection.y = 0;
            if (moveDirection.y < 0 && yMag < -maxWallRunSpeed) moveDirection.y = 0;
        }
        else
        {
            if (moveDirection.x > 0 && xMag > maxBaseSpeed) moveDirection.x = 0;
            if (moveDirection.x < 0 && xMag < -maxBaseSpeed) moveDirection.x = 0;
            if (moveDirection.y > 0 && yMag > maxBaseSpeed) moveDirection.y = 0;
            if (moveDirection.y < 0 && yMag < -maxBaseSpeed) moveDirection.y = 0;
        }

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!isGrounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;

            // Allows for bunny hopping
            if (rb.velocity.magnitude > 0.5f && isCrouching)
                shouldSlide = true;
        }

        // Slide as soon as player lands 
        if (isGrounded && shouldSlide)
        {
            Slide();
            return;
        }

        // Prevents player from moving while sliding
        if (isGrounded && isCrouching) multiplierV = 0f;

        // Apply forces to move player
        // crouch walking
        if (isCrouching && !isSliding)
        {
            rb.AddForce(orientation.transform.forward * moveDirection.y * crouchMoveSpeed * Time.deltaTime);
            rb.AddForce(orientation.transform.right * moveDirection.x * crouchMoveSpeed * Time.deltaTime);
        }
        // Wall Running
        else if (isOnWall)
        {
            rb.AddForce(orientation.transform.forward * moveDirection.y * wallRunSpeed * Time.deltaTime, ForceMode.Impulse);
            rb.AddForce(orientation.transform.right * moveDirection.x * wallRunSpeed * Time.deltaTime, ForceMode.Impulse);
        }
        // Normal movement on ground
        else
        {
            rb.AddForce(orientation.transform.forward * moveDirection.y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
            rb.AddForce(orientation.transform.right * moveDirection.x * moveSpeed * Time.deltaTime * multiplier);
        }

        SendPlayerData();
    }

    public void Jump()
    {
        if (jumpsAvaliable <= 0) return;
        if (isOnWall)
        {
            WallJump(false);
            return;
        }

        jumpsAvaliable -= 2;

        //Add jump forces
        rb.AddForce(orientation.up * jumpForce * 1.5f);
        rb.AddForce(orientation.up * jumpForce * 0.5f);

        //If jumping while falling, reset y velocity.
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f)
            rb.velocity = new Vector3(vel.x, 0, vel.z);
        else if (rb.velocity.y > 0)
            rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
    }
    
    public void CrouchController()
    {
        if (isCrouching)
            StopCrouch();
        else
            StartCrouch();
    }

    /// <summary>
    /// Shrinks player and conditionally decides if player should slide
    /// </summary>
    private void StartCrouch()
    {
        ServerSend.PlayerStartCrouchInfection(id, crouchScale);
        isCrouching = true;
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
            shouldSlide = true;

        // Prevents guns and other such objects from shrinking
        foreach (Transform child in gameObject.transform)
            child.localScale = playerScale;
        SendPlayerData();
    }

    /// <summary>
    /// enlarges players to original size
    /// </summary>
    public void StopCrouch()
    {
        ServerSend.PlayerStopCrouchInfection(id, playerScale);
        isCrouching = false;
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

        // Prevents guns and other such objects from expanding larger than intended
        foreach (Transform child in gameObject.transform)
            child.localScale = crouchScale;
        SendPlayerData();
    }

    /// <summary>
    /// Adds forces for player to slide and modifies booleans relating to sliding
    /// Dependencies: TurnOffisSliding
    /// </summary>
    private void Slide()
    {
        shouldSlide = false;
        isSliding = true;
        rb.AddForce(orientation.transform.forward * slideForce);
        StartCoroutine(TurnOffIsSliding());
    }

    /// <summary>
    /// recursively calls itself until player velocity is less than .1 in which case
    /// isSliding is set equal to false
    /// </summary>
    private IEnumerator TurnOffIsSliding()
    {
        yield return new WaitForSeconds(.1f);
        if (Mathf.Abs(rb.velocity.magnitude) > .1f)
            StartCoroutine(TurnOffIsSliding());
        else
            isSliding = false;
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns>Vector2</returns>
    public Vector2 FindVelRelativeToLook(Transform orientation, Rigidbody rb)
    {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!isGrounded || isJumping) return;

        //Slow down sliding
        if (isCrouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxBaseSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxBaseSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    #endregion

    #region Wall Movement

    // Wall Running
    // ================================================================================= //

    /// <summary>
    /// turns off gravity and adds forces to have player stick to wall
    /// </summary>
    private void Wallrun()
    {
        ServerSend.PlayerStartWallrun(id, isWallLeft, isWallRight);
        // Cancel out y velocity
        if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
        if (rb.useGravity)
            rb.useGravity = false;
        // prevents player to be crouched on wall
        if (isCrouching)
            StopCrouch();

        // get player off wall if they go over max time
        if (timeOnWall > maxtimeOnWall)
        {
            // if player is in a corner really send them flying
            if (isWallLeft && isWallRight)
            {
                if (isWallForward)
                    rb.AddForce(-orientation.forward * jumpForce * Time.deltaTime, ForceMode.Impulse);
                else if (isWallBackward)
                    rb.AddForce(orientation.forward * jumpForce * Time.deltaTime, ForceMode.Impulse);
            }
            // otherwise a subtle nudge to get them off a wall
            else
                WallJump(true);
        }

        // Stick player to wall
        if (isWallRight)
            rb.AddForce(orientation.right * wallrunForce * 100 * Time.deltaTime);
        else if (isWallLeft)
            rb.AddForce(-orientation.right * wallrunForce * 100 * Time.deltaTime);
        else if (isWallForward)
            rb.AddForce(orientation.forward * wallrunForce * 100 * Time.deltaTime);
        else if (isWallBackward)
            rb.AddForce(-orientation.forward * wallrunForce * 100 * Time.deltaTime);
    }

    /// <summary>
    /// Turns player gravity back on 
    /// </summary>
    private void StopWallRun()
    {
        ServerSend.PlayerStopWallrun(id);
        rb.useGravity = true;
    }


    /// <summary>
    /// Checks if player is on wall and if so which side of the player is on the wall
    /// </summary> 
    private void CheckForWall() //make sure to call in void Update
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, wallDistance, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, wallDistance, whatIsWall);
        isWallForward = Physics.Raycast(transform.position, orientation.forward, wallDistance, whatIsWall);
        isWallBackward = Physics.Raycast(transform.position, -orientation.forward, wallDistance, whatIsWall);
        isOnWall = Physics.CheckSphere(wallCheck.position + Vector3.up, wallDistance, whatIsWall);
        if (isOnWall) Wallrun();

        //leave wall run
        if (!isOnWall) StopWallRun();
        //reset double jump 
        if (isWallLeft || isWallRight) jumpsAvaliable = maxJumps;
    }


    /// <summary>
    /// Player jumps from a wall
    /// </summary>
    /// <param name="motionless"> boolean: Tells function whether the player is motionless of not </param>
    private void WallJump(bool motionless)
    {
        float forceMultiplier = 0.05f;
        RaycastHit hit;
        Vector3 direction;

        jumpsAvaliable -= 2;

        if (!motionless)
        {
            // Sets multipler for non-motionless wall jump
            forceMultiplier = 1.5f;
        }

        // Jumps off wall (opposite direction relative to wall)
        // If wall to left
        if (Physics.Raycast(transform.position, -orientation.right, out hit, 1f))
        {
            direction = -(hit.point - transform.position).normalized;
            rb.AddForce(direction * jumpForce * forceMultiplier);
        }
        // If wall to right
        else if (Physics.Raycast(transform.position, orientation.right, out hit, 1f))
        {
            direction = -(hit.point - transform.position).normalized;
            rb.AddForce(direction * jumpForce * forceMultiplier);
        }
        // If wall in front
        else if (Physics.Raycast(transform.position, orientation.forward, out hit, 1f))
        {
            direction = -(hit.point - transform.position).normalized;
            rb.AddForce(direction * jumpForce * forceMultiplier);
        }
        // If wall behind
        else if (Physics.Raycast(transform.position, -orientation.forward, out hit, 1f))
        {
            direction = -(hit.point - transform.position).normalized;
            rb.AddForce(direction * jumpForce * forceMultiplier);
        }

        if (!motionless)
            //Add vertical jump forces
            StartCoroutine(WallJumpHelper());

        rb.AddForce(orientation.forward * jumpForce / 10 * forceMultiplier);
    }

    /// <summary>
    /// Adds upward velocity after .1 seconds of scaled time. This is to allow for player to get horizontal distance from wall
    /// before applying an upward velocity (cleaner feel)
    /// </summary>
    private IEnumerator WallJumpHelper()
    {
        yield return new WaitForSeconds(.1f);
        rb.AddForce(orientation.up * jumpForce);
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
    /// Shoots a single raycast and damages if it hits anything that is damagable
    /// </summary>
    public void SingleFireShoot()
    {
        isAnimInProgress = true;
        currentGun.currentAmmo--;
        ServerSend.PlayerSingleFire(id, currentGun.currentAmmo, currentGun.reserveAmmo);
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
                ServerSend.PlayerShotLanded(id, _hit.point);
                if (_hit.collider.CompareTag("Player"))
                    _hit.collider.GetComponent<SPlayer>().TakeDamage(id, currentGun.damage);
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
                    ServerSend.PlayerShotLanded(id, _hit.point);
                    if (_hit.collider.CompareTag("Player"))
                        _hit.collider.GetComponent<SPlayer>().TakeDamage(id, currentGun.damage);
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
    /// Reloads current weapon
    /// </summary>
    public void Reload()
    {
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
        ServerSend.PlayerReload(id, currentGun.currentAmmo, currentGun.reserveAmmo);
    }

    /// <summary>
    /// Switches current weapon
    /// </summary>
    public void SwitchWeapon()
    {
        isAnimInProgress = true;

        GunInformation temp = currentGun;
        currentGun = secondaryGun;
        secondaryGun = temp;

        ServerSend.PlayerSwitchWeapon(id, currentGun.name, currentGun.currentAmmo, currentGun.reserveAmmo);
        // Send to all clients this player has switched it weapon
        foreach (ClientServerSide _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.OtherPlayerSwitchedWeapon(id, _client.id, currentGun.name);
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Sends player position and rotation to all cleints
    /// </summary>
    public void SendPlayerData()
    {
        ServerSend.PlayerPositionInfection(player);
        ServerSend.PlayerRotationInfection(player, orientation.localRotation);
    }
}
