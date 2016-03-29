using UnityEngine;

public class Player : MonoBehaviour
{

    public GameObject go;
    public Transform tform;
    public Rigidbody rb;
    public float speed = 24f;

    int hitCount = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        go = gameObject;
        tform = transform;
    }

    void FixedUpdate()
    {
        var targetVelocity = new Vector3(0, Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        var velocityChange = (targetVelocity - rb.velocity);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            hitCount++;
            Debug.Log("Hit " + hitCount + " times!");
        }
    }

}
