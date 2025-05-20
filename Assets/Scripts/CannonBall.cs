using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [SerializeField] private Collider ballCollider;
    [SerializeField] private Rigidbody ballRigidbody;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ballCollider.enabled = false;
            StartCoroutine(DestroyCoroutine());
        }
    }

    //Destroy Ball
    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }
}