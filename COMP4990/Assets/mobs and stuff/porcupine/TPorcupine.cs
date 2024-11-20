using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

/*
Porcupine

Movement: -----------------------------------------------------------------------------------------------------

Porcupine will wander aimlessly
    Wander in a random direction (up, down, left, right) for a random amount of time within a range
    then:
        75% chance to idle for a random amount of time within a range.
        25% chance to change to a different direction for a random amount of time within a range minus an offset
        for secondary movement.

Movement: -----------------------------------------------------------------------------------------------------

Agro: ---------------------------------------------------------------------------------------------------------

Porcupine will become mad when provoked
    Switch to chase state
    Stop wander coroutine
    Start chase coroutine

    Move towards the target until it comes within range to shoot
    Once it reaches the range from the target:
        stop moving
        shoot at the target until the target is shooting range + a buffer distance away

Agro: ---------------------------------------------------------------------------------------------------------

States:
    Wander
    Chase
    Attack
*/

public class TPorcupine : Animal
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Movement -------------------------------------------------------------------------------------------------

    private enum EDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    
    private Dictionary<EDirection, string> animDirectionTriggers = new Dictionary<EDirection, string>
    {
        { EDirection.Up, "switchUp" },
        { EDirection.Down, "switchDown" },
        { EDirection.Left, "switchLeft" },
        { EDirection.Right, "switchRight" }
    };

    private Dictionary<EDirection, Vector2> directionToVector = new Dictionary<EDirection, Vector2>
    {
        { EDirection.Up, Vector2.up },
        { EDirection.Down, Vector2.down },
        { EDirection.Left, Vector2.left },
        { EDirection.Right, Vector2.right }
    };

    private List<EDirection> directions = new List<EDirection>
    {
        EDirection.Up,
        EDirection.Down,
        EDirection.Left,
        EDirection.Right
    };

    private (float min, float max) primaryMoveTime = (2f, 5f);
    private (float min, float max) secondaryMoveTime = (1f, 3f);
    private (float min, float max) idleTime = (5f, 12f);
    [SerializeField]
    private float chanceForSecondaryMovement = 0.25f;

    [SerializeField]
    private float moveSpeed = 1f;

    /*
        Re assigned variables (dont want to re alloc space every time)
    */
    List<EDirection> availableDirections;
    private EDirection primaryDirection;
    private EDirection secondaryDirection;
    private float moveTime;

    // Movement -------------------------------------------------------------------------------------------------

    // Agro -----------------------------------------------------------------------------------------------------

    private Transform target;
    private bool isMad = false;
    private bool isShooting = false;
    [SerializeField]
    private float shootingRange = 6f; // range at which the porcupine will start shooting at the target
    [SerializeField]
    private float shootingRangeBuffer = 1.5f; // buffer that the porcupine will stay shooting in
    [SerializeField]
    private float shootCooldown = 3f; // time between shots

    [SerializeField]
    private float timeUntilBored = 5f; // maximum amount of time that should be spent chasing target (reset when damage is taken)
    private float currentTimeUntilBored; // current time from last damage taken
    [SerializeField]
    private float madMoveSpeedMultiplier = 1.2f; // will move quicker when angry

    [SerializeField]
    private float alertRadius = 10f;

    [SerializeField]
    private GameObject projectilePfb;

    private EDirection currentDirectionTowardsTarget;

    /*
        Re assigned vars
    */

    private float distanceToTarget;
    private Vector2 directionToTarget;
    private float shootTimer;

    // Agro -----------------------------------------------------------------------------------------------------
    
    private float currentHealth;

    private bool processingHit = false;
    private Color originalColour;

    private enum EState
    {
        Wander,
        Chase
    }

    private EState currentState;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        originalColour = spriteRenderer.color;

        StartCoroutine(Wander());
    }

    void Update()
    {
        if(isMad && currentState == EState.Wander)
        {
            BecomeMad();
        }

        gameObject.GetComponent<SortingOrder>().UpdateSortingOrder();
    }

    private IEnumerator Wander()
    {
        /* pick a direction,
        // move in that direction for some random amount of time in the range of primaryMoveTime
        // check for secondary movement
        // if secondary movement:
        //      pick a different direction from previous
        //      move in that direction for some random amount of time in the range of secondaryMoveTime
        // idle for some random amount of time in the range of idleTime
        */

        if(currentState != EState.Wander)
        {
            currentState = EState.Wander;
        }

        // only wander when the animal is not mad
        while(!isMad)
        {
            //Debug.Log($"isMad anim: {anim.GetBool("isMad")}");

            // reset the list of available directions
            availableDirections = new List<EDirection>(directions);

            // pick a random direction from the list of available directions, remove the selected direction from the list
            primaryDirection = availableDirections[Random.Range(0, availableDirections.Count)];
            availableDirections.Remove(primaryDirection);

            // pick a random amount of time to move for
            moveTime = Random.Range(primaryMoveTime.min, primaryMoveTime.max);

            //Debug.Log($"Starting primary movement in direction: {primaryDirection} for {moveTime}s.");

            // move in direction for time
            yield return StartCoroutine(Move(primaryDirection, moveTime));

            // decide if there will be a secondary movement
            if(Random.Range(0f, 1f) < chanceForSecondaryMovement && availableDirections.Count > 0)
            {
                secondaryDirection = availableDirections[Random.Range(0, availableDirections.Count)];
                moveTime = Random.Range(secondaryMoveTime.min, secondaryMoveTime.max);

                //Debug.Log($"Starting secondary movement in direction: {secondaryDirection} for {moveTime}s.");

                yield return StartCoroutine(Move(secondaryDirection, moveTime));
            }
        
            //Debug.Log("Starting idle.");

            // idle for random time in range
            anim.SetTrigger("switchIdle");
            yield return new WaitForSeconds(Random.Range(idleTime.min, idleTime.max));
        }
    }

    // THIS IS NOT USED WHEN THE PORCUPINE IS MAD
    private IEnumerator Move(EDirection dir, float moveTime)
    {
        /*
        set the correct animation trigger
        move in direction for time
        */
        anim.SetTrigger(animDirectionTriggers[dir]);

        float elapsedTime = 0f;

        while(elapsedTime < moveTime)
        {
            
            //transform.position += (Vector3)directionToVector[dir] * moveSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + directionToVector[dir] * moveSpeed * Time.fixedDeltaTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

    private void BecomeMad()
    {
        //Debug.Log("mad as fuck now fr");
        StopAllCoroutines();
        if(processingHit)
        {
            StartCoroutine(HitEffect());
        }
        currentState = EState.Chase;
        //Debug.Log("Setting mad");
        anim.SetBool("isMad", true);
        anim.SetTrigger("switchIdle");

        currentTimeUntilBored = timeUntilBored;
        StartCoroutine(Chase());
    }

    private void StopMad()
    {
        //Debug.Log("we chilling");
        StopAllCoroutines();
        if(processingHit)
        {
            StartCoroutine(HitEffect());
        }

        anim.SetBool("isMad", false);
        isMad = false;
        target = null;

        currentState = EState.Wander;

        //anim.SetTrigger("switchIdle");

        StartCoroutine(Wander());
    }

    /*
    The porcupine should chase the targeted player when provoked.
    as long as it's not bored, and the target exists:

    if the porcupine is not currently shooting, and has entered the shooting range, start shooting
    if the porcupine is currently shooting, and is within the shooting range + buffer, continue shooting
    if the porcupine is currently shooting, and is outside the shooting range + buffer, stop shooting

    */
    private IEnumerator Chase()
    {
        while(currentTimeUntilBored > 0 && target != null)
        {
            currentTimeUntilBored -= Time.deltaTime;

            distanceToTarget = Vector2.Distance(transform.position, target.position);

            // not yet shooting, entered the range, start shooting
            if(distanceToTarget <= shootingRange)
            {
                if(!isShooting)
                {
                    isShooting = true;
                    anim.SetTrigger("switchIdle");
                }
                TryShoot();
                // shoot
            }
            // is shooting, within range + buffer
            else if(distanceToTarget <= shootingRange + shootingRangeBuffer && isShooting)
            {
                TryShoot();
            }
            // not shooting and outside range or shooting and outside range + buffer
            else
            {
                if(isShooting)
                {
                    // stop shooting
                    isShooting = false;
                }

                directionToTarget = (target.position - transform.position).normalized;

                // if state has changed
                if(GetClosestDirection(directionToTarget) != currentDirectionTowardsTarget)
                {
                    currentDirectionTowardsTarget = GetClosestDirection(directionToTarget);
                    anim.SetTrigger(animDirectionTriggers[GetClosestDirection(directionToTarget)]);
                }
                
                //transform.position += (Vector3)directionToTarget * (moveSpeed * madMoveSpeedMultiplier) * Time.deltaTime;
                rb.MovePosition(rb.position + directionToTarget * (moveSpeed * madMoveSpeedMultiplier) * Time.fixedDeltaTime);
            }

            yield return null;
        }

        StopMad();
    }

    private void TryShoot()
    {
        //Debug.Log("we trying");
        shootTimer += Time.deltaTime;

        if(shootTimer >= shootCooldown)
        {
            Shoot(target);
            shootTimer = 0f;
        }
    }

    private void Shoot(Transform target)
    {
        //Debug.Log("Shooting");
        GameObject proj = Instantiate(projectilePfb, transform.position, Quaternion.identity);
        Vector2 shootDir = (target.position - transform.position).normalized;

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        rb.velocity = shootDir * 10f;
    }

    public override void Hit(float damage, Transform attacker)
    {
        target = attacker.parent.transform;
        if(!isMad)
        {
            isMad = true;
        }
        if(damage > 0f)
        {
            TakeDamage(damage);
            Alert();
        }
        anim.SetTrigger("switchIdle");
    }

    private void TakeDamage(float damage)
    {
        StartCoroutine(HitEffect());
        currentHealth -= damage;
        //Debug.Log($"{gameObject.name} took " + damage + " damage.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if(GameLoop.Instance != null)
        {
            GameLoop.Instance.OnPorcupineDeath();
        }
        Destroy(gameObject);
    }

    private IEnumerator HitEffect()
    {
        spriteRenderer.color = hitColour;
        processingHit = true;
        yield return new WaitForSeconds(hitDuration);
        spriteRenderer.color = originalColour;
        processingHit = false;
    }

    public bool IsMad()
    {
        return isMad;
    }

    // this could be in the Animal class and applied to every animal
    // alert nearby porcupines and make them mad at the target
    private void Alert()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, alertRadius);
        foreach(var collider in colliders)
        {
            TPorcupine porcupine = collider.GetComponent<TPorcupine>();
            // only apply to porcupines that are not this one, and are not already mad
            if(porcupine != null && porcupine != this && !porcupine.IsMad())
            {
                porcupine.Hit(0, target);
                //Debug.Log("a new porcupine is mad");
            }
        }
    }

    private EDirection GetClosestDirection(Vector2 direction)
    {
        // We just need to compare the axis with the most magnitude
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) // More horizontal movement
        {
            return direction.x < 0 ? EDirection.Left : EDirection.Right;
        }
        else // More vertical movement
        {
            return direction.y < 0 ? EDirection.Down : EDirection.Up;
        }
    }
}