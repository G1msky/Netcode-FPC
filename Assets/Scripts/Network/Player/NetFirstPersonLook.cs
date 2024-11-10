using UnityEngine;
using Unity.Netcode;

[DefaultExecutionOrder(1)]
public class NetFirstPersonLook : NetworkBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;


    void Reset()
    {
        if(IsOwner)
        {
            character = GetComponentInParent<NetFirstPersonCharacter>().transform;
        }
    }

    void Start()
    {
        if(!IsOwner)
        {
            gameObject.SetActive(false);
        } else {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        SetCharacterRotationServerRpc(velocity);
    }

    [ServerRpc]
    void SetCharacterRotationServerRpc(Vector2 rotation, ServerRpcParams rpcParams = default)
    {
        transform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
    }
}