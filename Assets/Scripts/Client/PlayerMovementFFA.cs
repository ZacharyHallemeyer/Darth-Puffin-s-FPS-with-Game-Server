﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementFFA : MonoBehaviour
{
    // General Variables
    public int id;
    public InputMaster inputMaster;
    public PlayerUI playerUI;
    public Transform orientation;
    public LayerMask whatIsGravityObject;

    #region Set Up

    /// <summary>
    /// Inits player info
    /// </summary>
    /// <param name="_id"> Client id </param>
    /// <param name="_maxJetPackTime"> Client max jet pack time </param>
    public void Initialize(int _id, float _maxJetPackTime)
    {
        id = _id;
        if (gameObject.name != "LocalPlayerFFA(Clone)")
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

    /// <summary>
    /// Rotate player according to gravity on client side (for cleaner movement)
    /// </summary>
    /// <param name="_gravityObjectCollider"></param>
    public void RotatePlayerAccordingToGravity(Collider _gravityObjectCollider)
    {
        Transform _gravityObject = _gravityObjectCollider.transform;
        Quaternion desiredRotation = Quaternion.FromToRotation(_gravityObject.up, -(_gravityObject.position - transform.position).normalized);
        desiredRotation = Quaternion.Lerp(transform.localRotation, desiredRotation, Time.deltaTime * 2);
        transform.localRotation = desiredRotation;
    }

    #region Jetpack

    /// <summary>
    /// Set jet pack UI
    /// </summary>
    /// <param name="_jetPackTime"> player's current jet pack time </param>
    public void PlayerContinueJetPack(float _jetPackTime)
    {
        playerUI.SetJetPack(_jetPackTime);
    }

    #endregion
}