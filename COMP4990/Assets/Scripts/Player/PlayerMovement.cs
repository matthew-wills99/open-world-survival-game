using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;

    public Animator animator;
    Vector2 movement;

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        ProcessInput();
        Animate();
        
    }

    void ProcessInput()
    {
        // Gets input of the user (WASD, ARROW KEYS, CONTROLLER EVEN)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        //Locks diagonal movement so it doesnt add both inputs
        movement.Normalize();
    }

    void Animate()
    {
        //Plays the correct animation with the speed+direction your moving (if speed >0.01 animation horizonatal or left is played)
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    private void FixedUpdate() {
        //Moves the player
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
