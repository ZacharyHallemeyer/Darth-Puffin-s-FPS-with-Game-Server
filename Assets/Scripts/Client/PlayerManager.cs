using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth;
    public MeshRenderer model;
    public InputMaster inputMaster;
    public PlayerActions playerActions;
    public PlayerMovement playerMovement;
    public PlayerUI playerUI;

    // Materials
    public Material basePlayerMaterial;
    public Material damagedPlayerMaterial;
    public Material deadPlayerMaterial;

    public Shader basePlayerShader;
    public Shader damagedPlayerShader;
    public Shader deadPlayerShader;

    public LineRenderer lineRenderer;

    // Stats
    public int currentKills = 0;
    public int currentDeaths = 0;

    // Weapons
    public class GunInformation
    {
        public string name;
        public GameObject gunContainer;
    }
    public Dictionary<string, GunInformation> allGunInformation { get; private set; } = new Dictionary<string, GunInformation>();

    public GameObject gunPistol;
    public GameObject gunSMG;
    public GameObject gunAR;
    public GameObject gunShotgun;

    #region Set Up

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        SetGunInformation();
        if (name != "LocalPlayer(Clone)")
            enabled = false;
    }

    public void SetGunInformation()
    {
        allGunInformation["Pistol"] = new GunInformation
        {
            name = "Pistol",
            gunContainer = gunPistol,
        };

        allGunInformation["SMG"] = new GunInformation
        {
            name = "SMG",
            gunContainer = gunSMG,
        };

        allGunInformation["AR"] = new GunInformation
        {
            name = "AR",
            gunContainer = gunAR,
        };

        allGunInformation["Shotgun"] = new GunInformation
        {
            name = "Shotgun",
            gunContainer = gunShotgun,
        };
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

    #endregion

    private void Update()
    {
        if (inputMaster.Player.ScoreBoard.triggered)
            playerUI.ScoreBoard();
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0)
        {
            Die();
        }
    }

    public void OtherPlayerTakenDamage(int _otherPlayerId)
    {
        GameManager.players[_otherPlayerId].GetComponent<MeshRenderer>().material = damagedPlayerMaterial;
        StartCoroutine(OtherPlayerTakeDamageHelper(_otherPlayerId));
    }

    public IEnumerator OtherPlayerTakeDamageHelper(int _otherPlayerId)
    {
        yield return new WaitForSeconds(.5f); 
        GameManager.players[_otherPlayerId].GetComponent<MeshRenderer>().material = basePlayerMaterial;
    }
    public void ClearOtherPlayerRope(int _otherPlayerId)
    {
        GameManager.players[_otherPlayerId].lineRenderer.positionCount = 0;
    }

    public void Die()
    {
        model.material.shader = deadPlayerShader;
        model.material = deadPlayerMaterial;
    }

    public void Respawn()
    {
        model.material.shader = basePlayerShader;
        model.material = basePlayerMaterial;
        SetHealth(maxHealth);
    }
}
