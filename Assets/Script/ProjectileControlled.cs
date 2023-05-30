using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileControlled : MonoBehaviour
{

    public UnityEvent<ProjectileControlled, Collider2D> onTriggerEnter, onCollisionEnter;
    public UnityEvent<ProjectileControlled> onDestroy;
    public float TimeToLive = 5f;
    public Vector2 direction = Vector2.right;

    public void UpdateLifetime()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive <= 0)
        { Destroy(gameObject); }
    }
    Vector3 eul;
    public void RotateTowards(Vector2 position, float max_angle, out float angle_diff)
    {
        angle_diff = Vector2.SignedAngle(transform.right, position - (Vector2)transform.position);
        float adiff = angle_diff;
        if (adiff > 0)
        {
            eul = transform.eulerAngles;
            eul.z += Mathf.Min(max_angle, adiff);
            transform.eulerAngles = eul;
            return; 
        }
        if (adiff < 0)
        {
            eul = transform.eulerAngles;
            eul.z -= Mathf.Min(max_angle, -adiff);
            transform.eulerAngles = eul;
            return; 
        }
    }
    public void MoveForward(float distance)
    {
        transform.position += transform.right * distance;
    }
    public void MoveTowardsDirection(float distance)
    {
        transform.position += new Vector3(direction.x * distance, direction.y * distance, 0f);
    }
    public void Move(Vector2 distance)
    {
        transform.position += new Vector3(distance.x, distance.y, 0);
    }

    private void OnDestroy()
    {
        onDestroy.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTriggerEnter.Invoke(this, collision);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        onCollisionEnter.Invoke(this, collision.collider);
    }

}
