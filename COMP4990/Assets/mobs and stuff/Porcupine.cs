using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Porcupine : Animal
{

    private enum EState
    {
        Idle,
        Left,
        Right,
        Up,
        Down
    }

    private List<EState> states;

    /*
    Every second the porcupine will have a chance to switch directions
    the chance will go up every second, making it more and more likely the longer it moves in the same direction

    porcupine will have a higher chance to be idle, the more times it switches to a non-idle state in a row
    */

    /*
    need to figure out why the porcupine moves in the wrong direction at the start sometimes
    (bro is moonwalking)
    */

    [SerializeField]
    private float chanceToSwitch = 0.2f; // 0 to 1 1 being 100%
    private float currentChance;

    [SerializeField]
    private float chanceIncreasePerSecond = 0.01f; // 1% increased chance to swap each second

    [SerializeField]
    private float checkTime = 1f;

    private bool isMad;

    private EState currentState;

    private Animator anim;

    private Vector3 direction;

    public float moveSpeed = 1f;

    private float currentHealth;

    // for changing colours on hit to red
    SpriteRenderer spriteRenderer;
    Color originalColour;

    // for agro when mad
    private Transform target; // target to move towards and attack when angry (player)
    [SerializeField]
    private float timeUntilBored = 30f; // maximum amount of time that should be spent chasing target (reset when damage is taken or dealt)
    private float currentTimeUntilBored; // current time from last damage taken or dealt'
    [SerializeField]
    private float madMoveSpeedMultiplier = 1.2f; // will move quicker when angry

    private EState previousState = EState.Idle;

    void Start()
    {
        // health
        currentHealth = maxHealth;

        // change colours on hit
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColour = spriteRenderer.color;

        anim = GetComponent<Animator>();

        // possible states for the porcupine to switch to
        states = new List<EState>()
        {
            EState.Idle,
            EState.Up,
            EState.Down,
            EState.Left,
            EState.Right
        };

        // start the porcupine in a random state
        currentState = EState.Idle;

        currentChance = chanceToSwitch;

        SetMad(false); // porcupines do not spawn angry

        StartCoroutine(CheckChangeState());

        currentTimeUntilBored = timeUntilBored;
    }

    void Update()
    {
        Move();
    }

    /*
    mods hit this guy
    the porcupine should be agro'd onto the last transform that did damage to it
    */
    public override void Hit(float damage, Transform attacker)
    {
        Debug.Log($"{gameObject.name} was hit by {attacker.name}, taking {damage} damage!");
        TakeDamage(damage);
        if(!isMad)
        {
            SetMad(true);
        }
        // update the targeted transform
        if(target != attacker)
        {
            target = attacker;
            Debug.Log($"{gameObject.name} is now angry at {target.name}!");
        }
        // when the porcupine is hit, the chase timer gets reset and it continues to chase
        currentTimeUntilBored = timeUntilBored;
    }

    private void TakeDamage(float damage)
    {
        StartCoroutine(HitEffect());
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took " + damage + " damage.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitEffect()
    {
        spriteRenderer.color = hitColour;
        yield return new WaitForSeconds(hitDuration);
        spriteRenderer.color = originalColour;
    }

    private Vector2 GetDirection()
    {
        switch(currentState)
        {
            case EState.Up:
                return Vector2.up;
            case EState.Down:
                return Vector2.down;
            case EState.Left:
                return Vector2.left;
            case EState.Right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    private void GetAnimationWhileMad(EState state)
    {
        //Debug.Log($"{anim.GetBool("isMad")}");
        switch(state)
        {
            case EState.Left:
                anim.SetTrigger("switchLeft");
                break;
            case EState.Right:
                anim.SetTrigger("switchRight");
                break;
            case EState.Up:
                anim.SetTrigger("switchUp");
                break;
            case EState.Down:
                anim.SetTrigger("switchDown");
                break;
            default:
                break;
        }
    }

    private void SetMad(bool mad)
    {
        isMad = mad;
        anim.SetBool("isMad", mad);
    }

    private void Move()
    {
        Debug.Log($"{anim.GetBool("isMad")}");

        if(isMad)
        {
            // not bored yet
            if(currentTimeUntilBored > 0)
            {
                currentTimeUntilBored -= Time.deltaTime;

                Vector2 newDirection = (target.position - transform.position).normalized;
                EState newState = GetClosestDirection(newDirection);

                Debug.Log($"newState: {newState}, previousState: {previousState}");

                if(newState != previousState)
                {
                    GetAnimationWhileMad(newState);
                    previousState = newState;
                }

                transform.position += (Vector3)newDirection * (moveSpeed * madMoveSpeedMultiplier) * Time.deltaTime;
            }
            // bored
            else
            {
                SetMad(false);
                ChangeState();
            }
        }
        else
        {
            if(currentState == EState.Idle)
            {
                Debug.Log("Idle");
                return;
            }

            direction = GetDirection();

            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        
    }

    private void ChangeState()
    {
        // dont implement mad states yet because the porcupine should not be mad without following the player for a given range
        currentState = states[UnityEngine.Random.Range(0, states.Count)];
        switch(currentState)
        {
            case EState.Idle:
                anim.SetTrigger("switchIdle");
                break;
            case EState.Up:
                anim.SetTrigger("switchUp");
                break;
            case EState.Down:
                anim.SetTrigger("switchDown");
                break;
            case EState.Left:
                anim.SetTrigger("switchLeft");
                break;
            case EState.Right:
                anim.SetTrigger("switchRight");
                break;
            default:
                break;
        }
        Debug.Log($"Changed states to: {currentState}");
    }

    private IEnumerator CheckChangeState()
    {
        while(true)
        {
            if(!isMad)
            {
                yield return new WaitForSeconds(checkTime);
                //Debug.Log("Checking..");

                // if the current chance to switch is hit
                if(UnityEngine.Random.value < currentChance)
                {
                    //Debug.Log("Changing states..");
                    // state is change
                    ChangeState();
                    currentChance = chanceToSwitch;
                }
                else
                {
                    // limit chance to 100%, not that it would get past that anyways
                    currentChance = Mathf.Min(currentChance + chanceIncreasePerSecond, 1f);
                    //Debug.Log($"Failed: {currentChance}");
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private EState GetClosestDirection(Vector2 direction)
    {
        // We just need to compare the axis with the most magnitude
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) // More horizontal movement
        {
            return direction.x < 0 ? EState.Left : EState.Right;
        }
        else // More vertical movement
        {
            return direction.y < 0 ? EState.Down : EState.Up;
        }
    }
}
