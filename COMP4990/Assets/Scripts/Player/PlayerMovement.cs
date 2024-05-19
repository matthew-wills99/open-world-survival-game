using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;

    public Animator animator;
    Vector2 movement;

    // Update is called once per frame
    void Update()
    {
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
