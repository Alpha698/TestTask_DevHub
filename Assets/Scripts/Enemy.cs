using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public static event Action EnemyAttack;

    [SerializeField]
    private float moveSpeed = 1f;

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ParticleSystem hitEffect;

    private List<Transform> pathPoints;
    private int currentPointIndex = 0;
    private bool isDead = false;
    private bool isAttacking = false;

    private void Update()
    {
        if (isDead || isAttacking || pathPoints == null || pathPoints.Count == 0) return;

        Transform target = pathPoints[currentPointIndex];

        // Movement on the plane (XZ), with fixed Y
        Vector3 direction = (target.position - transform.position);
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

        // Move
        transform.position += flatDirection.normalized * moveSpeed * Time.deltaTime;

        // Rotation
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Checking if the point has been reached
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            if (currentPointIndex < pathPoints.Count - 1)
            {
                currentPointIndex++;
            }
            else
            {
                Attack();
            }
        }
    }

    public void SetPath(List<Transform> path)
    {
        pathPoints = path;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CannonBall"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // Stop moving
        pathPoints = null;

        hitEffect.Play();

        // Play animation
        animator.SetTrigger("Death");

        // Remove the object after a delay
        Destroy(gameObject, 3f);
    }

    private void Attack()
    {
        if (isAttacking) return;

        animator.SetTrigger("Idle");

        StartCoroutine(AttackDelay());

    }

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(2f);

        isAttacking = true;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(1f);
        EnemyAttack?.Invoke();

        animator.SetTrigger("Idle");

    }
}