﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region movement_variables
    public float movespeed;
    float x_input;
    float y_input;
    #endregion

    #region attack_variables
    public float damage;
    public float attackSpeed;
    float attackTimer;
    public float hitboxTiming;
    public float endAnimationTiming;
    bool isAttacking;
    Vector2 currDirection;
    #endregion

    #region health_variables
    public float maxHealth;
    float currHealth;
    public Slider hpSlider;
    #endregion

    #region physics_components
    Rigidbody2D playerRB;
    #endregion

    #region animation_components
    Animator anim;
    #endregion

    #region Unity_functions
    // Called once on creation
    private void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>();

        attackTimer = 0;

        currHealth = maxHealth;

        hpSlider.value = currHealth / maxHealth;
    }

    // Called once per frame
    private void Update()
    {

        if (isAttacking)
        {
            return;
        }

        // Access our input values
        x_input = Input.GetAxisRaw("Horizontal");
        y_input = Input.GetAxisRaw("Vertical");

        Move();

        if (Input.GetKeyDown(KeyCode.J) && attackTimer <= 0)
        {
            Attack();
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Interact();
        }
    }
    #endregion

    #region movement_functions
    // Moves the player based on WASD inputs and 'movespeed'
    private void Move()
    {
        anim.SetBool("Moving", true);

        // If the player is pressing 'D'
        if (x_input > 0)
        {
            playerRB.velocity = Vector2.right * movespeed;
            currDirection = Vector2.right;
        }
        // If the player is pressing 'A'
        else if (x_input < 0)
        {
            playerRB.velocity = Vector2.left * movespeed;
            currDirection = Vector2.left;
        }
        // If the player is pressing 'W'
        else if (y_input > 0)
        {
            playerRB.velocity = Vector2.up * movespeed;
            currDirection = Vector2.up;
        }
        // If the player is pressing 'S'
        else if (y_input < 0)
        {
            playerRB.velocity = Vector2.down * movespeed;
            currDirection = Vector2.down;
        }
        else
        {
            playerRB.velocity = Vector2.zero;
            anim.SetBool("Moving", false);
        }

        // Set animator directional values
        anim.SetFloat("DirX", currDirection.x);
        anim.SetFloat("DirY", currDirection.y);
    }
    #endregion

    #region attack_functions
    // Attacks in the direction the player is facing
    private void Attack()
    {
        Debug.Log("Attacking Now!");
        Debug.Log(currDirection);

        // Handles all attack animations and calculates hitboxes
        StartCoroutine(AttackRoutine());

        attackTimer = attackSpeed;
    }

    // Handle animations and hitboxes for the attack mechanism
    IEnumerator AttackRoutine()
    {
        // Pause movement and freeze the player for the duration of the attack
        isAttacking = true;
        playerRB.velocity = Vector2.zero;

        // Start animation
        anim.SetTrigger("Attack");

        // Start Sound Effect
        FindObjectOfType<AudioManager>().Play("PlayerAttack");

        // Brief pause before the hitbox is calculated
        yield return new WaitForSeconds(hitboxTiming);
        Debug.Log("Cast Hitbox Now");

        // Create hitbox
        RaycastHit2D[] hits = Physics2D.BoxCastAll(playerRB.position + currDirection, Vector2.one, 0f, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits)
        {
            Debug.Log(hit.transform.name);

            if (hit.transform.CompareTag("Enemy"))
            {
                Debug.Log("Tons of Damage!");
                hit.transform.GetComponent<Enemy>().TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(endAnimationTiming);

        // Re-enables movement for player after attacking
        isAttacking = false;
    }
    #endregion

    #region health_functions
    // Take damage based on a 'value' parameter, which is passed in by caller
    public void TakeDamage(float value)
    {
        // Call Sound Effect
        FindObjectOfType<AudioManager>().Play("PlayerHurt");

        // Decrement current health
        currHealth -= value;
        Debug.Log("Player health is now " + currHealth.ToString());

        // Change UI
        hpSlider.value = currHealth / maxHealth;

        // Check for death
        if (currHealth <= 0)
        {
            Die();
        }
    }

    // Heals player HP based on 'value' parameter, which is passed in by caller
    public void Heal(float value)
    {
        // Increment current health
        currHealth += value;
        currHealth = Mathf.Min(currHealth, maxHealth);
        Debug.Log("Player health is now " + currHealth.ToString());

        // Change UI
        hpSlider.value = currHealth / maxHealth;
    }

    // Destroys Player object and triggers end scene
    private void Die()
    {
        // Call death sound effect
        FindObjectOfType<AudioManager>().Play("PlayerDeath");

        // Destroy gameObject
        Destroy(this.gameObject);

        // Trigger anything we need to end the game, find game manager and lose game

        GameObject gm = GameObject.FindWithTag("GameController");
        gm.GetComponent<GameManager>().LoseGame();
    }
    #endregion

    #region interact_functions
    private void Interact()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(playerRB.position + currDirection, new Vector2(0.5f, 0.5f), 0f, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Chest"))
            {
                hit.transform.GetComponent<Chest>().Interact();
            }
        }
    }
    #endregion
}
