using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetFirstPersonCharacter))]
public class NetFirstPersonUserControl : NetworkBehaviour
{

    private NetFirstPersonCharacter character; // Reference to the NetFirstPersonCharacter
    private Transform cam;                      // Reference to the main camera
    private NetworkVariable<Vector3> moveInput = new NetworkVariable<Vector3>(); 
    private bool jumpInput;                     // Jump input flag
    private NewControls gameInput;              // Input Actions

    public override void OnNetworkSpawn()
    {
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }

        if(IsOwner)
        {
            cam = GetComponentInChildren<Camera>().transform;
        }

        gameInput = new NewControls();
        gameInput.Gameplay.Enable();
    }

    private void Start()
    {
        character = GetComponent<NetFirstPersonCharacter>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Read jump input
        if (gameInput.Gameplay.Jump.triggered)
        {
            jumpInput = true;
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner && IsClient)
        {
            // Read movement input
            Vector2 inputVector = gameInput.Gameplay.Movement.ReadValue<Vector2>();
            moveInput = new NetworkVariable<Vector3>( new Vector3(inputVector.x, 0, inputVector.y));

            // Normalize input to prevent faster diagonal movement
            if (moveInput.Value.magnitude > 1f)
            {
                moveInput.Value.Normalize();
            }

            // Calculate camera-relative movement
            if (cam != null)
            {
                Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 camRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
                moveInput = new NetworkVariable<Vector3>(moveInput.Value.z * camForward + moveInput.Value.x * camRight);
            }
            else
            {
                // Use world-relative directions if no camera is found
                moveInput = new NetworkVariable<Vector3>(moveInput.Value.z * Vector3.forward + moveInput.Value.x * Vector3.right);
            }

            // Optional: Adjust movement speed (e.g., sprinting)
#if !MOBILE_INPUT
            if (gameInput.Gameplay.Sprint.ReadValue<float>() > 0.5f)
            {
                moveInput.Value *= 2f; // Sprint multiplier
            }
#endif

            // Pass movement input to the character
            character.MoveServerRpc(moveInput.Value);

            // Handle jump
            if (jumpInput)
            {
                character.JumpServerRpc();
                jumpInput = false;
            }
        }

    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.Gameplay.Disable();
        }
    }
}