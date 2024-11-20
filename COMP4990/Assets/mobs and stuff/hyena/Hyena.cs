using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyena : Animal
{
    private enum EState
    {
        Idle,
        Walking
    }

    private List<EState> states;
    private EState currentState;
    private EState previousState;

    [SerializeField]
    private float chanceToSwitch = 0.2f; // 0 to 1 1 being 100%
    private float currentChance;

    [SerializeField]
    private float chanceIncreasePerSecond = 0.01f; // 1% increased chance to swap each second

    [SerializeField]
    private float checkTime = 1f;

    private bool isMad;

    private Animator anim;

    private Vector3 direction;

    public float moveSpeed = 1f;
    private float currentHealth;

    // for changing colours on hit to red
    SpriteRenderer spriteRenderer;
    Color originalColour;

    private Transform target; // target to move towards and attack when angry (player)
    [SerializeField]
    private float timeUntilBored = 30f; // maximum amount of time that should be spent chasing target (reset when damage is taken or dealt)
    private float currentTimeUntilBored; // current time from last damage taken or dealt'
    [SerializeField]
    private float madMoveSpeedMultiplier = 1.2f; // will move quicker when angry

    [SerializeField]
    private float agroRange = 5f; // range that hyenas will be mad from
    private bool isChasing = false;
    private float attackRange = 1f;
    private bool isAttacking = false;
    [SerializeField]
    private float attackCooldown = 1f;

    void Start()
    {
        // health
        currentHealth = maxHealth;

        // change colours on hit
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColour = spriteRenderer.color;

        anim = GetComponent<Animator>();
        currentState = EState.Idle;
        anim.SetTrigger("setIdle");

        StartCoroutine(CheckWanderState());

        currentChance = chanceToSwitch;
        currentTimeUntilBored = timeUntilBored;
    }

    // idgaf about no attacker low key (on the dl)
    public override void Hit(float damage, Transform attacker)
    {
        Debug.Log($"{gameObject.name} was hit by {attacker.name}, taking {damage} damage!");
        TakeDamage(damage);

        // update the targeted transform
        if(target != attacker)
        {
            target = attacker;
            Debug.Log($"{gameObject.name} is now angry at {target.name}!");
        }
        // continue chasing when hit obviously
        currentTimeUntilBored = timeUntilBored;
    }

    private void TakeDamage(float damage)
    {
        StartCoroutine(HitEffect());
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took " + damage + " damage.");

        if (currentHealth <= 0)
        {
            //Die();
        }
    }

    private IEnumerator HitEffect()
    {
        spriteRenderer.color = hitColour;
        yield return new WaitForSeconds(hitDuration);
        spriteRenderer.color = originalColour;
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestPlayer();
        
        if(isChasing && !isAttacking)
        {
            ChaseTarget();
        }
        else
        {
            Wander();
        }
    }

    private void Wander()
    {
        // chilling
    }

    private void FindClosestPlayer()
    {
        if(!isChasing)
        {
            target = null;
        }
        float closestDistance = agroRange;

        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                target = player.transform;
                if(!isChasing)
                {
                    anim.SetTrigger("setWalking");
                    isChasing = true;
                }
            }
        }

        if(target && closestDistance < agroRange)
        {
            currentTimeUntilBored = timeUntilBored;
        }
    }

    private void ChaseTarget()
    {
        if(currentTimeUntilBored <= 0 || target == null)
        {
            isChasing = false;
            anim.SetTrigger("setIdle");
            return;
        }

        currentTimeUntilBored -= Time.deltaTime;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        if(distanceToTarget <= attackRange && !isAttacking)
        {
            StartCoroutine(Attack());
        }
        else if(distanceToTarget > attackRange)
        {
            isAttacking = false;

            Vector2 direction = (target.position - transform.position).normalized;
            spriteRenderer.flipX = direction.x < 0 ? false : true;

            transform.position += (Vector3)direction * moveSpeed * madMoveSpeedMultiplier * Time.deltaTime;
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;

        while(Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            anim.SetTrigger("attack");
            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
    }

    private IEnumerator CheckWanderState()
    {
        while(true)
        {
            if(!isChasing)
            {
                currentState = (UnityEngine.Random.value > 0.5f) ? EState.Idle : EState.Walking;
                if(currentState != previousState)
                {
                    if(currentState == EState.Idle)
                    {
                        anim.SetTrigger("setIdle");
                    }
                    else
                    {
                        anim.SetTrigger("setWalking");
                    }
                    previousState = currentState;
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));
        }
    }
}
