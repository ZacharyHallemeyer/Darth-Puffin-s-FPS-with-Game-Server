using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionInfection : MonoBehaviour
{
    // General Variables
    public int id;
    public Camera playerCam;
    public InputMaster inputMaster;
    public PlayerUI playerUI;

    // Crouch
    private Vector3 playerScale = new Vector3(1, 1.5f, 1);
    private Vector3 CrouchScale = Vector3.one;
    public bool isCrouching = false;

    // Materials
    private Material playerMaterial, damagedPlayerMaterial, deadPlayerMaterial;
    public Material basePlayerMaterial, infectedPlayerMaterial;
    public Material baseDamagedPlayerMaterial, infectedDamagedPlayerMaterial;
    public Material baseDeadPlayerMaterial, infectedDeadPlayerMaterial;

    private Shader playerShader, damagedPlayerShader, deadPlayerShader;
    public Shader basePlayerShader, infectedPlayerShader;
    public Shader baseDamagedPlayerShader, infectedDamagedPlayerShader;
    public Shader baseDeadPlayerShader, infectedDeadPlayerShader;

    // Movement
    public Transform orientation;
    public LayerMask whatIsGravityObject;

    // Guns!
    public class GunInformation
    {
        public string name;
        public float originalGunRadius;
        public GameObject gunContainer;
        public ParticleSystem bullet;
        public ParticleSystem gun;
        public float reloadTime;
        public float fireRate;
    }
    public Dictionary<string, GunInformation> allGunInformation { get; private set; } = new Dictionary<string, GunInformation>();

    public Transform firePoint;
    public GameObject gunPistol;
    public GameObject gunShotgun;
    public GameObject gunMelee;
    public ParticleSystem pistolBullet;
    public ParticleSystem shotgunBullet;
    public ParticleSystem pistol;
    public ParticleSystem shotgun;
    public ParticleSystem melee;

    public GameObject[] hitParticleGameObjects;
    public ParticleSystem[] hitParticles;
    private int particleIndex = 0;

    public GunInformation currentGun, secondaryGun;
    private ParticleSystem.ShapeModule shapeModule;
    public GameObject[] gunObjects;
    public bool isAnimInProgress = false;
    public bool isShooting = false;
    public int animationCounter = 0;
    public float timeSinceLastShoot = 10;

    #region Set Up

    /// <summary>
    /// Inits new player
    /// </summary>
    /// <param name="_id"> Client id </param>
    /// <param name="_gunName"> current gun name </param>
    /// <param name="_currentAmmo"> current ammo </param>
    /// <param name="_reserveAmmo"> current reserve ammo </param>
    /// <param name="_maxGrappleTime"> max grapple time </param>
    public void Initialize(int _id, string _gunName, int _currentAmmo, int _reserveAmmo, bool _isInfected)
    {
        id = _id;
        SetGunInformation();
        PlayerInitGun(_gunName, _currentAmmo, _reserveAmmo);
        if(_isInfected)
        {
            playerMaterial = infectedPlayerMaterial;
            damagedPlayerMaterial = infectedDamagedPlayerMaterial;
            deadPlayerMaterial = infectedDeadPlayerMaterial;
            playerShader = infectedPlayerShader;
            damagedPlayerShader = infectedDamagedPlayerShader;
            deadPlayerShader = infectedDeadPlayerShader;
        }
        else
        {
            playerMaterial = basePlayerMaterial;
            damagedPlayerMaterial = baseDamagedPlayerMaterial;
            deadPlayerMaterial = baseDeadPlayerMaterial;
            playerShader = basePlayerShader;
            damagedPlayerShader = baseDamagedPlayerShader;
            deadPlayerShader = baseDeadPlayerShader;
        }
        GetComponent<MeshRenderer>().material = playerMaterial;
        GetComponent<MeshRenderer>().material.shader = playerShader;

        if (gameObject.name[0] != 'L')
            enabled = false;
    }
    
    public void MakeInfected(string _gunName)
    {
        playerMaterial = infectedPlayerMaterial;
        damagedPlayerMaterial = infectedDamagedPlayerMaterial;
        deadPlayerMaterial = infectedDeadPlayerMaterial;
        playerShader = infectedPlayerShader;
        damagedPlayerShader = infectedDamagedPlayerShader;
        deadPlayerShader = infectedDeadPlayerShader;
        PlayerInitGun(_gunName, 0, 0);
    }

    private void Awake()
    {
        inputMaster = new InputMaster();
    }

    public void OnEnable()
    {
        inputMaster.Enable();
    }

    public void OnDisable()
    {
        inputMaster.Disable();
    }

    /// <summary>
    /// Sets all gun information for all possible weapons
    /// </summary>
    public void SetGunInformation()
    {
        allGunInformation["Pistol"] = new GunInformation
        {
            name = "Pistol",
            gunContainer = gunPistol,
            bullet = pistolBullet,
            gun = pistol,
            originalGunRadius = pistol.shape.radius,
            reloadTime = 1f,
            fireRate = .7f,
        };
        allGunInformation["Shotgun"] = new GunInformation
        {
            name = "Shotgun",
            gunContainer = gunShotgun,
            bullet = shotgunBullet,
            gun = shotgun,
            originalGunRadius = shotgun.shape.radius,
            reloadTime = 1f,
            fireRate = 1f,
        };
        allGunInformation["Melee"] = new GunInformation
        {
            name = "Melee",
            gunContainer = gunMelee,
            gun = melee,
            originalGunRadius = shotgun.shape.radius,
            reloadTime = 1f,
            fireRate = 1f,
        };
    }

    #endregion

    private void Update()
    {
        timeSinceLastShoot += Time.deltaTime;

        if(!isCrouching)
        {
            if (inputMaster.Player.Crouch.ReadValue<float>() != 0)
            {
                isCrouching = true;
                ClientSend.PlayerCrouchInfection();
            }
        }
        else
        {
            if (inputMaster.Player.Crouch.ReadValue<float>() == 0)
            {
                isCrouching = false;
                ClientSend.PlayerCrouchInfection();
            }
        }
        if (inputMaster.Player.Jump.triggered)
            ClientSend.PlayerJumpInfection();

        // Handle Guns
        if (isAnimInProgress) return;

        if (!isShooting)
        {
            if (inputMaster.Player.Shoot.ReadValue<float>() != 0 && timeSinceLastShoot > currentGun.fireRate)
            {
                timeSinceLastShoot = 0;
                ClientSend.PlayerShootInfection(firePoint.position, playerCam.transform.forward);
            }
        }
        if (inputMaster.Player.Reload.triggered)
            ClientSend.PlayerReloadInfection();
        if (inputMaster.Player.SwitchWeaponMouseWheel.ReadValue<Vector2>().y != 0
            || inputMaster.Player.SwitchWeaponButton.triggered)
            ClientSend.PlayerSwitchWeaponInfection();
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>
    /// Sends player action input to server 
    /// </summary>
    private void SendInputToServer()
    {
        Vector2 _moveDirection = inputMaster.Player.Movement.ReadValue<Vector2>();

        ClientSend.PlayerMovementInfection(_moveDirection);
        ClientSend.PlayerActionsInfection(isAnimInProgress);
    }

    public void StartCrouch()
    {
        // Prevents guns and other such objects from expanding larger than intended
        foreach (Transform child in gameObject.transform)
            child.localScale = playerScale;
        playerUI.ChangeToCrouch();
    }

    public void StopCrouch()
    {
        isCrouching = false;
        // Prevents guns and other such objects from expanding larger than intended
        foreach (Transform child in gameObject.transform)
            child.localScale = CrouchScale;
        playerUI.ChangeToStand();
    }

    #region Guns

    public void Melee()
    {
        isAnimInProgress = true;
        InvokeRepeating("MeleeHelperSwing", 0, .01f);
    }

    public void MeleeHelperSwing()
    {
        if(gunMelee.transform.localEulerAngles.y > 289 || gunMelee.transform.localEulerAngles.y == 0)
        {
            gunMelee.transform.localRotation *= Quaternion.Euler(0, -5, 0);
        }
        else if(gunMelee.transform.localEulerAngles.x > 350 || gunMelee.transform.localEulerAngles.x == 0)
        {
            gunMelee.transform.localRotation *= Quaternion.Euler(-2, 0, 0);
        }
        else
        {
            gunMelee.transform.localRotation = Quaternion.Euler(350, 290, 0);
            InvokeRepeating("MeleeHelperReturn", 0, .01f);
            CancelInvoke("MeleeHelperSwing");
        }
    }

    public void MeleeHelperReturn()
    {
        if (gunMelee.transform.localEulerAngles.y < 354)
        {
            gunMelee.transform.localRotation *= Quaternion.Euler(0, 5, 0);
        }
        else if (gunMelee.transform.localEulerAngles.x > 0 && gunMelee.transform.localEulerAngles.x < 349)
        {
            gunMelee.transform.localRotation *= Quaternion.Euler(2, 0, 0);
        }
        else
        {
            gunMelee.transform.localRotation = Quaternion.Euler(0, 0, 0);
            CancelInvoke("MeleeHelperReturn");
            isAnimInProgress = false;
        }
    }

    /// <summary>
    /// Single fire animation start
    /// </summary>
    /// <param name="_currentAmmo"> current gun's current ammo </param>
    /// <param name="_reserveAmmo"> current gun's reserve ammo </param>
    public void PlayerStartSingleFireAnim(int _currentAmmo, int _reserveAmmo)
    {
        isAnimInProgress = true;
        isShooting = true;
        currentGun.gun.Stop();
        currentGun.bullet.Play();
        playerUI.ChangeGunUIText(_currentAmmo, _reserveAmmo);
        InvokeRepeating("PlayerSingleFireAnim", currentGun.fireRate, 0f);
    }

    /// <summary>
    /// Single fire animation stop
    /// </summary>
    public void PlayerSingleFireAnim()
    {
        isAnimInProgress = false;
        isShooting = false;
        currentGun.gun.Play();
        CancelInvoke("PlayerSingleFireAnim");
    }

    /// <summary>
    /// reload animation start
    /// </summary>
    /// <param name="_currentAmmo"> current gun's current ammo </param>
    /// <param name="_reserveAmmo"> current gun's reserve ammo </param>
    public void PlayerStartReloadAnim(int _currentAmmo, int _reserveAmmo)
    {
        isAnimInProgress = true;
        playerUI.ChangeGunUIText(_currentAmmo, _reserveAmmo);
        InvokeRepeating("ReloadAnimCompress", 0f, currentGun.reloadTime / 6f);
    }

    /// <summary>
    /// reload animation part 2 (Compresses weapon)
    /// </summary>
    public void ReloadAnimCompress()
    {
        if (shapeModule.radius > currentGun.originalGunRadius * 10)
        {
            InvokeRepeating("ReloadAnimExpand", 0f, currentGun.reloadTime / 4);
            CancelInvoke("ReloadAnimCompress");
            return;
        }
        currentGun.gun.Stop();
        shapeModule.radius *= 1.5f;
        currentGun.gun.Play();
    }

    /// <summary>
    /// reload animation part 1 (expands weapon)
    /// </summary>
    public void ReloadAnimExpand()
    {
        if (shapeModule.radius < currentGun.originalGunRadius)
        {
            shapeModule.radius = currentGun.originalGunRadius;
            isAnimInProgress = false;
            CancelInvoke("ReloadAnimExpand");
            return;
        }
        currentGun.gun.Stop();
        shapeModule.radius *= .5f;
        currentGun.gun.Play();
    }

    /// <summary>
    /// switch weapon animation start
    /// </summary>
    /// <param name="_newGunName"> new gun's name</param>
    /// <param name="_currentAmmo"> new gun's current ammo </param>
    /// <param name="_reserveAmmo"> new gun's reserve ammo </param>
    public void PlayerStartSwitchWeaponAnim(string _newGunName, int _currentAmmo, int _reserveAmmo)
    {
        isAnimInProgress = true;
        PlayerInitGun(_newGunName, _currentAmmo, _reserveAmmo);

        foreach (GunInformation _gun in allGunInformation.Values)
        {
            if (_gun.name == _newGunName)
                secondaryGun = _gun;
        }

        GunInformation temp = currentGun;
        currentGun = secondaryGun;
        secondaryGun = temp;

        playerUI.ChangeGunUIText(_currentAmmo, _reserveAmmo);
        InvokeRepeating("ChangeCurrentGunAnimationExapnd", 0, 1f / 10f);
    }

    /// <summary>
    /// Expands current gun by increasing particle system radius
    /// Has be called with invoke repeating and takes 10 iterations to finished. Divide total time by 10 for time repeat 
    /// Dependencies: ChangeCurrentGunAnimationCompress
    /// </summary>
    private void ChangeCurrentGunAnimationExapnd()
    {
        if (animationCounter >= 10)
        {
            animationCounter = 0;
            CancelInvoke("ChangeCurrentGunAnimationExapnd");
            shapeModule.radius = currentGun.originalGunRadius;

            secondaryGun.gunContainer.SetActive(false);
            currentGun.gunContainer.SetActive(true);
            shapeModule = currentGun.gun.shape;
            shapeModule.radius *= 20;

            InvokeRepeating("ChangeCurrentGunAnimationCompress", 0f, 1f / 10);
            return;
        }
        secondaryGun.gun.Stop();
        shapeModule.radius *= 2;
        secondaryGun.gun.Play();

        animationCounter++;
    }

    /// <summary>
    /// Compress current gun by decreasing particle system radius
    /// Has be called with invoke repeating and takes 10 iterations to finished. Divide total time by 10 for time repeat 
    /// </summary>
    private void ChangeCurrentGunAnimationCompress()
    {
        if (animationCounter >= 10)
        {
            animationCounter = 0;
            CancelInvoke("ChangeCurrentGunAnimationCompress");

            shapeModule.radius = currentGun.originalGunRadius;
            isAnimInProgress = false;
            return;
        }
        // else
        currentGun.gun.Stop();
        shapeModule.radius /= 2;
        currentGun.gun.Play();

        animationCounter++;
    }

    /// <summary>
    /// Plays shot landed animation
    /// </summary>
    /// <param name="_hitPoint"> position where shot landed </param>
    public void PlayerShotLanded(Vector3 _hitPoint)
    {
        if (particleIndex >= hitParticleGameObjects.Length)
            particleIndex = 0;
        hitParticleGameObjects[particleIndex].transform.position = _hitPoint;
        hitParticles[particleIndex].Play();

        particleIndex++;
    }

    /// <summary>
    /// Inits player gun animation
    /// </summary>
    /// <param name="_gunName"> gun to init </param>
    /// <param name="_currentAmmo"> gun's current ammo </param>
    /// <param name="_reserveAmmo"> gun's reserve ammo </param>
    public void PlayerInitGun(string _gunName, int _currentAmmo, int _reserveAmmo)
    {
        foreach (GunInformation _gunInfo in allGunInformation.Values)
        {
            if (allGunInformation[_gunInfo.name].gunContainer.activeSelf == true)
                allGunInformation[_gunInfo.name].gunContainer.SetActive(false);
            if (_gunName == _gunInfo.name)
                currentGun = _gunInfo;
        }
        currentGun.gunContainer.SetActive(true);
        shapeModule = currentGun.gun.shape;
        if (playerUI == null)
            playerUI = FindObjectOfType<PlayerUI>();
        playerUI.ChangeGunUIText(_currentAmmo, _reserveAmmo);
    }

    /// <summary>
    /// show other player's current weapon
    /// </summary>
    /// <param name="_id"> client's id to show current weapon </param>
    /// <param name="_gunName"> client's current weapon </param>
    public void ShowOtherPlayerActiveWeapon(int _id, string _gunName)
    {
        if (CInfectionGameManager.playersActionsInfection.TryGetValue(_id, out PlayerActionInfection _player))
        {
            _player = CInfectionGameManager.playersActionsInfection[_id];
        }
        else return;

        foreach (GunInformation _gunInfo in _player.allGunInformation.Values)
        {
            if (_player.allGunInformation[_gunInfo.name].gunContainer.activeSelf == true)
                _player.allGunInformation[_gunInfo.name].gunContainer.SetActive(false);
            if (_gunInfo.name == _gunName)
            {
                _gunInfo.gunContainer.SetActive(true);
            }
        }
    }

    #endregion

    public void Die(int _otherPlayerId)
    {
        CInfectionGameManager.playersActionsInfection[id].GetComponent<MeshRenderer>().material = deadPlayerMaterial;
        CInfectionGameManager.playersActionsInfection[id].GetComponent<MeshRenderer>().material.shader = deadPlayerShader;
    }

    /// <summary>
    /// Respawn animation and set health
    /// </summary>
    public void Respawn(int _health)
    {
        CInfectionGameManager.playersActionsInfection[id].GetComponent<MeshRenderer>().material = playerMaterial;
        CInfectionGameManager.playersActionsInfection[id].GetComponent<MeshRenderer>().material.shader = playerShader;
        CInfectionGameManager.players[id].health = _health;
    }

    /// <summary>
    /// Show other player take damage animation
    /// </summary>
    /// <param name="_otherPlayerId"> player that took damage id </param>
    public void OtherPlayerTakenDamage(int _otherPlayerId)
    {
        CInfectionGameManager.players[_otherPlayerId].GetComponent<MeshRenderer>().material = damagedPlayerMaterial;
        CInfectionGameManager.players[_otherPlayerId].GetComponent<MeshRenderer>().material.shader = damagedPlayerShader;
        StartCoroutine(OtherPlayerTakeDamageHelper(_otherPlayerId));
    }

    public IEnumerator OtherPlayerTakeDamageHelper(int _otherPlayerId)
    {
        yield return new WaitForSeconds(.5f);
        CInfectionGameManager.players[_otherPlayerId].GetComponent<MeshRenderer>().material = playerMaterial;
        CInfectionGameManager.players[_otherPlayerId].GetComponent<MeshRenderer>().material.shader = playerShader;
    }
}
