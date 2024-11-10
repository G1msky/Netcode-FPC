using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class NetFirstPersonCharacter : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        // Freeze rotation to prevent unwanted tilting
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsClient)
        {
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Moves the character based on input movement vector.
    /// </summary>
    /// <param name="movement">A Vector3 representing movement direction and magnitude.</param>
    /// <param name="rpcParams">ServerRpcParams</param>
    [ServerRpc]
    public void MoveServerRpc(Vector3 movement, ServerRpcParams rpcParams = default)
    {
        // Calculate the desired velocity
        Vector3 velocity = movement * moveSpeed;

        // Preserve the existing vertical velocity (e.g., from jumping or falling)
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;
    }

    /// <summary>
    /// Makes the character jump if grounded.
    /// </summary>
    /// <param name="rpcParams">ServerRpcParams</param>
    [ServerRpc]
    public void JumpServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            isGrounded = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Check if the contact point is below the character to determine if grounded
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}