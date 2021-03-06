using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamController : MonoBehaviour
{
    // TODO Change these to not be static
    public static bool isOnWall, isWallLeft, isWallRight;
    public float wallRunCameraTilt, maxWallRunCameraTilt = 10;


    public Transform playerRotation;
    public Transform orientation;
    public Camera playerCam;
    public float clampAngle = 85f;
    public InputMaster inputMaster;

    private float verticleRotation;
    private float horizontalRotation;

    // Camera
    //public Transform playerCamPosition;
    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float desiredX;
    private float normalFOV = 60;
    private float adsFOV = 40;
    private float sensitivity = 2000f;
    private float normalSensitivity = 2000f;
    private float adsSensitivity = 500f;
    // Default value for sens multipliers are 1 

    public float sensMultiplier { get; set; } = 1f;
    public float adsSensMultiplier { get; set; } = 1f;

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

    private void Start()
    {
        Cursor.visible = !Cursor.visible;
        Cursor.lockState = CursorLockMode.Locked;

        verticleRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (inputMaster.Player.Escape.triggered)
            ToggleCursorMode();
        if (Cursor.lockState == CursorLockMode.Locked)
            Look();
        Debug.DrawRay(transform.position, transform.forward * 2, Color.green);

        // ADS 
            // TODO
    }

    /// <summary>
    /// Update camera and orientation rotations
    /// </summary>
    private void Look()
    {
        mouseX = playerCam.ScreenToViewportPoint(inputMaster.Player.MouseLook.ReadValue<Vector2>()).x 
                 * sensitivity * Time.deltaTime * sensMultiplier;
        mouseY = playerCam.ScreenToViewportPoint(inputMaster.Player.MouseLook.ReadValue<Vector2>()).y
                 * sensitivity * Time.deltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // Perform the rotations
        transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.localRotation = Quaternion.Euler(0, desiredX, 0);


        // Tilts camera in .5 second
        // Prevents camera from spinning on the y-axis at a very fast angular velocity
        // maxWallRunCameraTilt - wallRunCameraTilt < -.1f, wallRunCameraTilt < maxWallRunCameraTilt
        if (wallRunCameraTilt < maxWallRunCameraTilt && isOnWall && isWallRight)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        if (wallRunCameraTilt > -maxWallRunCameraTilt && isOnWall && isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;

        // Tilts camera back again
        if (wallRunCameraTilt > .2f && !isWallRight && !isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        else if (wallRunCameraTilt < -.2f && !isWallRight && !isWallLeft)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
    }

    /// <summary>
    /// locks and unlocks cursor depending on its current lock state
    /// </summary>
    private void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}
