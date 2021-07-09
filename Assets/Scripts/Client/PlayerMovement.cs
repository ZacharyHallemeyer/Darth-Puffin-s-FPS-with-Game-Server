using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // General Variables
    public int id;
    public InputMaster inputMaster;
    public PlayerUI playerUI;
    public Transform orientation;
    public LayerMask whatIsGravityObject;

    #region Set Up

    public void Initialize(int _id, float _maxJetPackTime)
    {
        id = _id;
        if (gameObject.name != "LocalPlayer(Clone)")
        {
            enabled = false;
            return;
        }
        playerUI.SetMaxJetPack(_maxJetPackTime);

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
        if (Physics.OverlapSphere(transform.position, 10, whatIsGravityObject).Length != 0)
            RotatePlayerAccordingToGravity(Physics.OverlapSphere(transform.position, 10, whatIsGravityObject)[0]);

        // Jetpack up and down
        if (inputMaster.Player.Jump.ReadValue<float>() != 0)
            ClientSend.PlayerJetPackMovement(orientation.up);
        if (inputMaster.Player.Crouch.ReadValue<float>() != 0)
            ClientSend.PlayerJetPackMovement(-orientation.up);
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        Vector2 _moveDirection = inputMaster.Player.Movement.ReadValue<Vector2>();

        ClientSend.PlayerMovement(_moveDirection);
    }

    public void RotatePlayerAccordingToGravity(Collider _gravityObjectCollider)
    {
        Transform _gravityObject = _gravityObjectCollider.transform;
        Quaternion desiredRotation = Quaternion.FromToRotation(_gravityObject.up, -(_gravityObject.position - transform.position).normalized);
        desiredRotation = Quaternion.Lerp(transform.localRotation, desiredRotation, Time.deltaTime * 2);
        transform.localRotation = desiredRotation;
    }

    #region Jetpack

    public void PlayerContinueJetPack(float _jetPackTime)
    {
        playerUI.SetJetPack(_jetPackTime);
    }

    #endregion
}
