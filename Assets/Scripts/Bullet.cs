using UnityEngine;
using System.Collections;

[System.Serializable]
public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public GameObject go;
    [HideInInspector]
    public Transform tform;
    [HideInInspector]
    public Rigidbody2D rb;

    [SerializeField]
    public float speed = 5f;
    [SerializeField]
    public float verticalSpeed = 0f;
    [SerializeField]
    public bool useVertical = false;
    [SerializeField]
    public float lifetime = 0f;
    [SerializeField]
    public PrevRotWrapper prw = new PrevRotWrapper();
    [SerializeField]
    public BulletAction[] actions;
    [SerializeField]
    public float param = 0f;
    [SerializeField]
    public int actionIndex = 0;

    [SerializeField]
    public BulletPattern master;

    void Awake()
    {
        go = gameObject;
        tform = transform;
        rb = GetComponent<Rigidbody2D>();

        //make sure this bullet knows whos his daddy
        //actually its so 100 bullet objects dont clutter the hierarchy window

        tform.parent = BulletManager.instance.transform;
    }



    void FixedUpdate()
    {
        var targetVelocity = tform.forward * speed;
        // dont bother with vertical speed unless its been tampered with (see ChangeSpeedVertical..or was it SpeedChangeVertical?)
        if (useVertical)
        {
            targetVelocity += BulletManager.instance.MainCam.up * verticalSpeed;
        }
        var velocityChange = (targetVelocity - (Vector3)rb.velocity);
        rb.AddForce(velocityChange, ForceMode2D.Force);

        //if you uncomment this stuff the bullet willl die automatically die after awhile
        // look at me tempting you like this 

        //lifetime += Time.deltaTime;
        //if(lifetime > BulletManager.instance.bulletLifespan)
        //	Deactivate();


    }


    IEnumerator RunActions()
    {
        for (actionIndex = 0; actionIndex < actions.Length; actionIndex++)
        {
            switch (actions[actionIndex].type)
            {
                case (BulletActionType.Wait):
                    float waitT;
                    if (actions[actionIndex].randomWait)
                        waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
                    else
                        waitT = actions[actionIndex].waitTime.x;
                    if (actions[actionIndex].rankWait)
                        waitT += BulletManager.instance.rank * actions[actionIndex].waitTime.z;
                    waitT *= BulletManager.instance.timePerFrame;
                    yield return new WaitForSeconds(waitT);
                    break;
                case (BulletActionType.ChangeDirection):
                    if (actions[actionIndex].waitForChange)
                        yield return StartCoroutine(ChangeDirection(actionIndex));
                    else
                        StartCoroutine(ChangeDirection(actionIndex));
                    break;
                case (BulletActionType.ChangeSpeed):
                    if (actions[actionIndex].waitForChange)
                        yield return StartCoroutine(ChangeSpeed(actionIndex, false));
                    else
                        StartCoroutine(ChangeSpeed(actionIndex, false));
                    break;
                case (BulletActionType.StartRepeat):
                    yield return StartCoroutine(RunNestedActions());
                    break;
                case (BulletActionType.Fire):
                    if (master != null)
                        master.Fire(tform, actions[actionIndex], param, prw);
                    break;
                case (BulletActionType.VerticalChangeSpeed):
                    if (actions[actionIndex].waitForChange)
                        yield return StartCoroutine(ChangeSpeed(actionIndex, true));
                    else
                        StartCoroutine(ChangeSpeed(actionIndex, true));
                    break;
                case (BulletActionType.Deactivate):
                    Deactivate();
                    break;
            }
        }
    }

    IEnumerator RunNestedActions()
    {
        var startIndex = actionIndex;
        var endIndex = 0;
        actionIndex++;

        float repeatC = actions[startIndex].repeatCount.x;
        if (actions[startIndex].rankRepeat)
            repeatC += actions[startIndex].repeatCount.y * BulletManager.instance.rank;
        repeatC = Mathf.Floor(repeatC);

        for (var y = 0; y < repeatC; y++)
        {
            while (actions[actionIndex].type != BulletActionType.EndRepeat)
            {
                switch (actions[actionIndex].type)
                {
                    case (BulletActionType.Wait):
                        float waitT;
                        if (actions[actionIndex].randomWait)
                            waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
                        else
                            waitT = actions[actionIndex].waitTime.x;
                        if (actions[actionIndex].rankWait)
                            waitT += BulletManager.instance.rank * actions[actionIndex].waitTime.z;
                        waitT *= BulletManager.instance.timePerFrame;
                        yield return new WaitForSeconds(waitT);
                        break;
                    case (BulletActionType.ChangeDirection):
                        if (actions[actionIndex].waitForChange)
                            yield return ChangeDirection(actionIndex);
                        else
                            StartCoroutine(ChangeDirection(actionIndex));
                        break;
                    case (BulletActionType.ChangeSpeed):
                        if (actions[actionIndex].waitForChange)
                            yield return ChangeSpeed(actionIndex, false);
                        else
                            StartCoroutine(ChangeSpeed(actionIndex, false));
                        break;
                    case (BulletActionType.EndRepeat):
                        yield return RunNestedActions();
                        break;
                    case (BulletActionType.Fire):
                        if (master != null)
                            master.Fire(tform, actions[actionIndex], param, prw);
                        break;
                    case (BulletActionType.VerticalChangeSpeed):
                        if (actions[actionIndex].waitForChange)
                            yield return ChangeSpeed(actionIndex, true);
                        else
                            StartCoroutine(ChangeSpeed(actionIndex, true));
                        break;
                    case (BulletActionType.Deactivate):
                        Deactivate();
                        break;
                }

                actionIndex++;

            }

            endIndex = actionIndex;
            actionIndex = startIndex + 1;
        }

        actionIndex = endIndex;
    }

    //activate a bullet, wow what would you do without these comments?
    public void Activate()
    {
        BulletManager.instance.bulletCount++;
        lifetime = 0f;
        verticalSpeed = 0f;
        useVertical = false;
        prw.prevRotationNull = true;
        StartCoroutine(RunActions());
    }
    //we dont actually destroy bullets, were all about recycling
    void Deactivate()
    {
        if (go.activeSelf)
        {
            BulletManager.instance.bulletCount--;
            go.SetActive(false);
        }
    }

    //im feeling adventurous so im going to try putting comments INSIDE the function, you ready?
    IEnumerator ChangeDirection(int i)
    {
        var t = 0f;

        //determine how long this operation will take
        float d;
        if (actions[i].randomWait)
            d = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
        else
            d = actions[i].waitTime.x;
        if (actions[i].rankWait)
            d += BulletManager.instance.rank * actions[i].waitTime.z;

        d *= BulletManager.instance.timePerFrame;

        var originalRot = tform.localRotation;

        // determine offset
        float ang;
        if (actions[i].randomAngle)
            ang = Random.Range(actions[i].angle.x, actions[i].angle.y);
        else
            ang = actions[i].angle.x;
        if (actions[i].rankAngle)
            ang += BulletManager.instance.rank * actions[i].angle.z;

        Quaternion newRot = Quaternion.identity;

        //and set rotation depending on angle
        switch (actions[i].direction)
        {
            case (DirectionType.TargetPlayer):
                var dotHeading = Vector3.Dot(tform.up, BulletManager.instance.player.position - tform.position);
                int dir;
                if (dotHeading > 0)
                    dir = -1;
                else
                    dir = 1;
                var angleDif = Vector3.Angle(tform.forward, BulletManager.instance.player.position - tform.position);
                newRot = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right);
                break;

            case (DirectionType.Absolute):
                newRot = Quaternion.Euler(-(ang - 270), 270, 0);
                break;

            case (DirectionType.Relative):
                newRot = originalRot * Quaternion.AngleAxis(-ang, Vector3.right);
                break;

        }

        //Sequence has its own thing going on, continually turning a set amount until time is up
        if (actions[i].direction == DirectionType.Sequence)
        {
            newRot = Quaternion.AngleAxis(-ang, Vector3.right);

            while (t < d)
            {
                tform.localRotation *= newRot;
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
        //all the others just linearly progress to destination rotation
        else if (d > 0)
        {
            while (t < d)
            {
                tform.localRotation = Quaternion.Slerp(originalRot, newRot, t / d);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            tform.localRotation = newRot;
        }
    }

    //its basically the same as the above except without rotations
    IEnumerator ChangeSpeed(int i, bool isVertical)
    {

        var t = 0f;
        var s = 0f;

        if (isVertical)
            useVertical = true;

        float d;
        if (actions[i].randomWait)
            d = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
        else
            d = actions[i].waitTime.x;
        if (actions[i].rankWait)
            d += BulletManager.instance.rank * actions[i].waitTime.z;
        d *= BulletManager.instance.timePerFrame;

        var originalSpeed = speed;

        float newSpeed;
        if (actions[i].randomSpeed)
            newSpeed = Random.Range(actions[i].speed.x, actions[i].speed.y);
        else
            newSpeed = actions[i].speed.x;
        if (actions[i].rankSpeed)
            d += BulletManager.instance.rank * actions[i].speed.z;

        if (d > 0)
        {
            while (t < d)
            {
                s = Mathf.Lerp(originalSpeed, newSpeed, t / d);
                if (isVertical) verticalSpeed = s;
                else speed = s;
                t += Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }
        }

        if (isVertical) verticalSpeed = newSpeed;
        else speed = newSpeed;
    }

    //if a bullet leaves the boundary then make sure it actually left the screen, not enetered it
    // if it did, then deactivate
    // this process MAY be too intensive for iPhone, but then it does keep bullet number under control
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boundary"))
        {
            if (CamColliders.instance.isOutSideBox(tform.position))
            {
                Deactivate();
            }

        }
    }

   

}







//exclusive bullet action variables
[System.Serializable]
public class BulletAction : BPAction
{
    [SerializeField]
    public BulletActionType type = BulletActionType.Wait;
    [SerializeField]
    public bool waitForChange = false;
}

[System.Serializable]
public enum BulletActionType
{
    Wait,
    ChangeDirection,
    ChangeSpeed,
    StartRepeat,
    EndRepeat,
    Fire,
    VerticalChangeSpeed,
    Deactivate
};


