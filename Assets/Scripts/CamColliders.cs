using UnityEngine;

public class CamColliders : MonoBehaviour
{

    static Transform tform;
    public static CamColliders use;

    public Transform bottom;
    public Transform top;
    public Transform right;
    public Transform left;

    public LayerMask collideMask;

    float bottomPos;
    float topPos;
    float rightPos;
    float leftPos;

    void Awake()
    {
        use = this;
        tform = transform;

        bottomPos = bottom.localPosition.y;
        topPos = top.localPosition.y;
        rightPos = right.localPosition.x;
        leftPos = left.localPosition.x;
    }

    public bool IsInsideBox(Vector3 pos)
    {
        if (Physics.Linecast(pos, tform.position, collideMask))
        {
            return false;
        } else {
            return true;
        }
    }

    Vector3 FindPointInBox(Vector3 pos)
    {
        var rayDir = (pos - tform.position).normalized;
        Ray ray = new Ray(tform.position, rayDir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, collideMask))
        {
            return hit.point + (-3 * rayDir);
        }

        return Vector3.zero;

    }

}
