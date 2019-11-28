using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public LayerMask whatIsGround;
    public Vector2 size;
    public float heighit;
    // Start is called before the first frame update

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, size * transform.parent.localScale.x);
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(transform.position, size * transform.parent.localScale.x, 0f, whatIsGround);
    }
}
