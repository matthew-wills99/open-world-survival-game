using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerPlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;
    public Animator animator;

    private Vector2 movement;

    // Network variables to synchronize position and animation state
    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();
    private NetworkVariable<Vector2> networkMovement = new NetworkVariable<Vector2>();
    private NetworkVariable<float> networkSpeed = new NetworkVariable<float>();

    void Update()
    {
        if (IsOwner)
        {
            // Only the owner processes input
            ProcessInput();
            Animate();

            // Update server with movement and position
            UpdateMovementServerRpc(rb.position, movement);
        }
        else
        {
            // Non-owners use networked values
            ApplyNetworkedMovement();
            ApplyNetworkedAnimation();
        }
    }

    void ProcessInput()
    {
        // Gets input of the user (WASD, ARROW KEYS, CONTROLLER EVEN)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();
    }

    void Animate()
    {
        // Plays the correct animation with the speed+direction you're moving
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // Moves the player locally
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    [ServerRpc]
    private void UpdateMovementServerRpc(Vector2 position, Vector2 moveInput)
    {
        // Update network variables with the latest data from the owner
        networkPosition.Value = position;
        networkMovement.Value = moveInput;
        networkSpeed.Value = moveInput.sqrMagnitude;
    }

    private void ApplyNetworkedMovement()
    {
        // Non-owners smoothly interpolate to the updated position
        rb.position = Vector2.Lerp(rb.position, networkPosition.Value, Time.deltaTime * 10f);
    }

    private void ApplyNetworkedAnimation()
    {
        // Update animations based on networked values
        animator.SetFloat("Horizontal", networkMovement.Value.x);
        animator.SetFloat("Vertical", networkMovement.Value.y);
        animator.SetFloat("Speed", networkSpeed.Value);
    }
}