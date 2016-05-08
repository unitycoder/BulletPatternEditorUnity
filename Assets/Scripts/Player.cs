using UnityEngine;

public class Player : MonoBehaviour
{

    public GameObject go;
    public Transform tform;
    public Rigidbody2D rb;
    public float speed = 24f;

    int hitCount = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        go = gameObject;
        tform = transform;
    }

    void FixedUpdate()
    {
        var targetVelocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),0);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        var velocityChange = (targetVelocity - (Vector3)rb.velocity);

        rb.AddForce(velocityChange, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            hitCount++;
            Debug.Log("Hit " + hitCount + " times!");
        }
    }

}
