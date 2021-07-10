using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionsFFA : MonoBehaviour
{
    // General Variables
    public int id;
    public Camera playerCam;
    public InputMaster inputMaster;
    public PlayerUI playerUI;

    // Grapple
    public LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    public bool isGrappling = false;
    public bool releasedGrappleControlSinceLastGrapple = true;

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

    public GameObject gunPistol;
    public GameObject gunSMG;
    public GameObject gunAR;
    public GameObject gunShotgun;
    public ParticleSystem pistolMuzzleFlash;
    public ParticleSystem smgMuzzleFlash;
    public ParticleSystem arMuzzleFlash;
    public ParticleSystem shotgunMuzzleFlash;
    public ParticleSystem pistol;
    public ParticleSystem smg;
    public ParticleSystem ar;
    public ParticleSystem shotgun;

    public GameObject[] hitParticleGameObjects;
    public ParticleSystem[] hitParticles;
    private int particleIndex = 0;

    public GunInformation currentGun, secondaryGun;
    private ParticleSystem.ShapeModule shapeModule;
    public GameObject[] gunObjects;
    public bool isAnimInProgress;
    public bool isShooting;
    public int animationCounter = 0;
    public float timeSinceLastShoot = 0;

    #region Set Up

    /// <summary>
    /// Inits new player
    /// </summary>
    /// <param name="_id"> Client id </param>
    /// <param name="_gunName"> current gun name </param>
    /// <param name="_currentAmmo"> current ammo </param>
    /// <param name="_reserveAmmo"> current reserve ammo </param>
    /// <param name="_maxGrappleTime"> max grapple time </param>
    public void Initialize(int _id, string _gunName, int _currentAmmo, int _reserveAmmo,
                           float _maxGrappleTime)
    {
        id = _id;
        SetGunInformation();
        if (gameObject.name != "LocalPlayerFFA(Clone)")
        {
            enabled = false;
            return;
        }
        PlayerInitGun(_gunName, _currentAmmo, _reserveAmmo);
        playerUI.SetMaxGrapple(_maxGrappleTime);
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
            bullet = pistolMuzzleFlash,
            gun = pistol,
            originalGunRadius = pistol.shape.radius,
            reloadTime = 1f,
            fireRate = .7f,
        };

        allGunInformation["SMG"] = new GunInformation
        {
            name = "SMG",
            gunContainer = gunSMG,
            bullet = smgMuzzleFlash,
            gun = smg,
            originalGunRadius = smg.shape.radius,
            reloadTime = 1f,
            fireRate = .1f,
        };

        allGunInformation["AR"] = new GunInformation
        {
            name = "AR",
            gunContainer = gunAR,
            bullet = arMuzzleFlash,
            gun = ar,
            originalGunRadius = ar.shape.radius,
            reloadTime = 1f,
            fireRate = .2f,
        };

        allGunInformation["Shotgun"] = new GunInformation
        {
            name = "Shotgun",
            gunContainer = gunShotgun,
            bullet = shotgunMuzzleFlash,
            gun = shotgun,
            originalGunRadius = shotgun.shape.radius,
            reloadTime = 1f,
            fireRate = 1f,
        };
    }

    #endregion

    private void Update()
    {
        timeSinceLastShoot += Time.deltaTime;
        // Handle grapple
        if (!isGrappling)
        {
            if (inputMaster.Player.Grapple.ReadValue<float>() != 0 && releasedGrappleControlSinceLastGrapple)
                ClientSend.PlayerStartGrapple(playerCam.transform.forward);
        }
        if (!releasedGrappleControlSinceLastGrapple)
        {
            if (inputMaster.Player.Grapple.ReadValue<float>() == 0
                || Mathf.Abs((transform.position - grapplePoint).magnitude) < 5f)
            {
                releasedGrappleControlSinceLastGrapple = true;
                ClientSend.PlayerStopGrapple();
                StopGrapple();
            }
        }

        if (isGrappling)
            DrawRope();

        // Magnitize
        if (inputMaster.Player.Magnetize.triggered)
            ClientSend.PlayerMagnetize();

        // Handle Guns
        if (isAnimInProgress) return;

        if (!isShooting)
        {
            if (inputMaster.Player.Shoot.ReadValue<float>() != 0 && timeSinceLastShoot > currentGun.fireRate)
            {
                timeSinceLastShoot = 0;
                ClientSend.PlayerStartShoot(playerCam.transform.position, playerCam.transform.forward);
            }
        }
        if (isShooting)
        {
            ClientSend.PlayerUpdateShootDirection(playerCam.transform.position, playerCam.transform.forward);
            if (inputMaster.Player.Shoot.ReadValue<float>() == 0)
                ClientSend.PlayerStopShoot();
        }
        if (inputMaster.Player.Reload.triggered)
            ClientSend.PlayerReload();
        if (inputMaster.Player.SwitchWeaponMouseWheel.ReadValue<Vector2>().y != 0
            || inputMaster.Player.SwitchWeaponButton.triggered)
            ClientSend.PlayerSwitchWeapon();
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>
    /// Sends player action input to server 
    /// </summary>
    public void SendInputToServer()
    {
        ClientSend.PlayerActions(isAnimInProgress);
    }

    #region Guns

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
    /// automatic fire animation start
    /// </summary>
    /// <param name="_currentAmmo"> current gun's current ammo </param>
    /// <param name="_reserveAmmo"> current gun's reserve ammo </param>
    public void PlayerStartAutomaticFireAnim(int _currentAmmo, int _reserveAmmo)
    {
        playerUI.ChangeGunUIText(_currentAmmo, _reserveAmmo);
        isShooting = true;
        ParticleSystem.RotationOverLifetimeModule rot = currentGun.gun.rotationOverLifetime;
        rot.enabled = true;
    }

    /// <summary>
    /// automatic fire animation continue
    /// </summary>
    /// <param name="_currentAmmo"> current gun's current ammo </param>
    /// <param name="_reserveAmmo"> current gun's reserve ammo </param>
    public void PlayerContinueAutomaticFireAnim(int _currentAmmo, int _reserveAmmo)
    {
        playerUI.ChangeGunUIText(_currentAmmo, _reserveAmmo);
        currentGun.bullet.Play();
    }

    /// <summary>
    /// automatic fire animation stop
    /// </summary>
    /// <param name="_currentAmmo"> current gun's current ammo </param>
    /// <param name="_reserveAmmo"> current gun's reserve ammo </param>
    public void PlayerStopAutomaticFireAnim()
    {
        isShooting = false;
        ParticleSystem.RotationOverLifetimeModule rot = currentGun.gun.rotationOverLifetime;
        rot.enabled = false;
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
        // else
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
        if (GameManager.players.TryGetValue(_id, out PlayerManager _player))
        {
            _player = GameManager.players[_id];
        }
        else return;

        foreach (PlayerManager.GunInformation _gunInfo in _player.allGunInformation.Values)
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

    #region Grapple

    /// <summary>
    /// start grapple rope 
    /// </summary>
    public void StartGrapple()
    {
        releasedGrappleControlSinceLastGrapple = false;
        isGrappling = true;
        if (Physics.Raycast(transform.position, playerCam.transform.forward, out RaycastHit _hit))
            grapplePoint = _hit.point;
        lineRenderer.positionCount = 2;
    }

    /// <summary>
    /// Draws grapple rope
    /// </summary>
    public void DrawRope()
    {
        ClientSend.PlayerContinueGrappling(transform.position, grapplePoint);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    /// <summary>
    /// Draws other player's grapple rope
    /// </summary>
    /// <param name="_otherPlayerId"> other player to draw rope for </param>
    /// <param name="_position"> other player's position </param>
    /// <param name="_grapplePoint"> other player's grapple point </param>
    public void DrawOtherPlayerRope(int _otherPlayerId, Vector3 _position, Vector3 _grapplePoint)
    {
        GameManager.players[_otherPlayerId].lineRenderer.positionCount = 2;
        GameManager.players[_otherPlayerId].lineRenderer.SetPosition(0, _position);
        GameManager.players[_otherPlayerId].lineRenderer.SetPosition(1, _grapplePoint);
    }

    /// <summary>
    /// Updates grapple UI
    /// </summary>
    /// <param name="_currentGrappleTime"> current grapple time </param>
    public void ContinueGrapple(float _currentGrappleTime)
    {
        playerUI.SetGrapple(_currentGrappleTime);
    }

    /// <summary>
    /// Stops drawing rope
    /// </summary>
    public void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
    }

    #endregion 

}
