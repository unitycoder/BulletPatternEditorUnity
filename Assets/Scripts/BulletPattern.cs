/* Code located in this and related files in this Unity package authored by Daniel Hendricks, copyright 2011
	Feel more than welcome to edit and use this code as you see fit, but never claim original authorship. Thanks.
	If you have confusion or questions about this code feel free to email me at drayhendrix@gmail.com and I'll do my best 
	to get back to you.
	
	These scripts are free to use, but if you feel generous, you can send a small donation with Paypal
	to "drayhendrix@gmail.com", Thanks.
	
	This is the main script of the BulletPattern package. It handles the logic flow of patterns and creation of bullets.
	
*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class BulletPattern : MonoBehaviour
{
    [SerializeField]
    Transform tform;

    [SerializeField]
    public FireTag[] fireTags;
    [SerializeField]
    public BulletTag[] bulletTags;

    [SerializeField]
    float sequenceSpeed = 0f;

    [SerializeField]
    bool started = false;
    [SerializeField]
    public float waitBeforeRepeating = 5f;


    //These belong in the Editor class, as they control whether or not the GUI Foldouts are open or closed
    //However by putting them here they will actually SAVE whether or not they are open after deselecting the object
    //Bear in mind this does increase mememory usage somewhat (per BulletPattern object) so remove these on release version if necesaary
    [SerializeField]
    public bool ftFoldout = false;
    [SerializeField]
    public List<bool> ftFoldouts = new List<bool>();
    [SerializeField]
    public bool btFoldout = false;
    [SerializeField]
    public List<bool> btFoldouts = new List<bool>();

    [SerializeField]
    public List<ActionFoldouts> ftaFoldouts = new List<ActionFoldouts>();
    [SerializeField]
    public List<ActionFoldouts> btaFoldouts = new List<ActionFoldouts>();


    [SerializeField]
    public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence };

    //setting these saves a little time on referencing objects later
    void Awake()
    {
        tform = transform;
    }

    //get the ball rolling
    void Start()
    {
        StartCoroutine(InitiateFire());
    }

    //resume if the script was disabled then enabled, the bool prevents 2 calls to InstantiateFire on first startup
    void OnEnable()
    {
        if (started)
        {
            StartCoroutine(InitiateFire());
        }

    }

    //start the first FireTag in the array of tags, then repeat until disabled or destroyed
    IEnumerator InitiateFire()
    {
        if (!started)
            yield return new WaitForSeconds(1.0f);

        started = true;

        while (true)
        {
            yield return StartCoroutine(RunFire(0));
            yield return new WaitForSeconds(waitBeforeRepeating * BulletManager.instance.timePerFrame);
        }
    }


    //Gets a bullet and returns it. We look through BulletManager's bullet pool to see if a bullet
    // is available, so we can avoid creating abullet if possible. Infinitely creating and
    // destroying objects is bad for performance.
    Bullet GetInstance(List<Bullet> arr, Transform tform, Bullet prefab)
    {

        for (var i = 0; i < arr.Count; i++)
        {
            var tempGo = (arr[i] as Component).gameObject;

            if (!tempGo.activeSelf)
            {
                tempGo.SetActive(true);
                return arr[i];
            }
        }

        var temp = Instantiate(prefab, tform.position, tform.rotation) as Bullet;
        arr.Add(temp);
        return temp;
    }

    //creates a bullet and initiales its parameters
    public void Fire(Transform t, BPAction a, float param, PrevRotWrapper prw)
    {
        //find the correct bulletTag that has info for this bullet
        var bt = bulletTags[a.bulletTagIndex - 1];

        //Debug.Log("bt:"+bt.actions[0].type);

        //get the bullet
        Bullet temp = GetInstance(BulletManager.instance.bullets[bt.prefabIndex].bl, t, BulletManager.instance.bulletPrefab[bt.prefabIndex]);

        if (prw.prevRotationNull)
        {
            prw.prevRotationNull = false;
            prw.previousRotation = temp.transform.localRotation;
        }

        //set positions equal to its creator, which could be a Firetag or Bullet
        temp.transform.position = t.position;
        temp.transform.rotation = t.rotation;
        //set the abgle offset of new bullet
        float ang;
        if (a.useParam)
            ang = param;
        else
        {
            if (a.randomAngle)
                ang = Random.Range(a.angle.x, a.angle.y);
            else
                ang = a.angle.x;
            if (a.rankAngle)
                ang += BulletManager.instance.rank * a.angle.z;
        }

        //actually point the bullet in the right direction
        switch ((DirectionType)a.direction)
        {
            case (DirectionType.TargetPlayer):
                var originalRot = t.rotation;
                var dotHeading = Vector3.Dot(temp.transform.up, BulletManager.instance.player.position - temp.transform.position);

                int dir;
                if (dotHeading > 0)
                    dir = -1;
                else
                    dir = 1;
                var angleDif = Vector3.Angle(temp.transform.forward, BulletManager.instance.player.position - temp.transform.position);
                temp.transform.rotation = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right);
                break;

            case (DirectionType.Absolute):
                temp.transform.localRotation = Quaternion.Euler(-(ang - 270), 270, 0);
                break;

            case (DirectionType.Relative):
                temp.transform.localRotation = t.localRotation * Quaternion.AngleAxis(-ang, Vector3.right);
                break;

            case (DirectionType.Sequence):
                temp.transform.localRotation = prw.previousRotation * Quaternion.AngleAxis(-ang, Vector3.right);
                break;
        }
        //record this rotation for next Sequence Direction
        prw.previousRotation = temp.transform.localRotation;
        //set the speed, either from creator's speed data
        if (a.overwriteBulletSpeed)
        {
            float spd;
            if (a.randomSpeed)
                spd = Random.Range(a.speed.x, a.speed.y);
            else
                spd = a.speed.x;
            if (a.rankSpeed)
                spd += BulletManager.instance.rank * a.speed.z;

            if (a.useSequenceSpeed)
            {
                sequenceSpeed += spd;
                temp.speed = sequenceSpeed;
            } else
            {
                sequenceSpeed = 0f;
                temp.speed = spd;
            }
        }
        //or bulletTag data
        else
        {
            if (bt.randomSpeed)
                temp.speed = Random.Range(bt.speed.x, bt.speed.y);
            else
                temp.speed = bt.speed.x;
            if (bt.rankSpeed)
                temp.speed += BulletManager.instance.rank * bt.speed.z;
        }

        //set the bullets actions array, so it can perform actions later if it has any
        temp.actions = bt.actions;

        //pass random params to bullet, commented out because it seemed to be causing errors and I never used it anyway

        if (a.passParam)
            temp.param = Random.Range(a.paramRange.x, a.paramRange.y);

        //pass param that the creator received form another FireTag before creating this bullet(see readMe file)
        if (a.passPassedParam)
            temp.param = param;
        //give the bullet a reference to this script 
        temp.master = this;
        //and activate it
        temp.Activate();
    }

    //a wrapper so we can pass an int as reference in the next 2 IEnumerator functions
    [System.Serializable]
    class IndexWrapper
    {
        [SerializeField]
        public int idx = 0;
    }

    //performs logic flow for a FireTag, starting its actions one by one, and even calling other FireTags
    IEnumerator RunFire(int i)
    {
        var ft = fireTags[i];
        var iw = new IndexWrapper();

        if (ft.actions.Length == 0)
            Fire(tform, ft.actions[iw.idx], ft.param, ft.prw);
        else
        {
            for (iw.idx = 0; iw.idx < ft.actions.Length; iw.idx++)
            {
                switch (ft.actions[iw.idx].type)
                {
                    case (FireActionType.Wait):
                        float waitT;
                        if (ft.actions[iw.idx].randomWait)
                            waitT = Random.Range(ft.actions[iw.idx].waitTime.x, ft.actions[iw.idx].waitTime.y);
                        else
                            waitT = ft.actions[iw.idx].waitTime.x;
                        if (ft.actions[iw.idx].rankWait)
                            waitT += BulletManager.instance.rank * ft.actions[iw.idx].waitTime.z;
                        waitT *= BulletManager.instance.timePerFrame;
                        yield return new WaitForSeconds(waitT);
                        break;

                    case (FireActionType.Fire):
                        Fire(tform, ft.actions[iw.idx], ft.param, ft.prw);
                        break;

                    case (FireActionType.CallFireTag):
                        var idx = ft.actions[iw.idx].fireTagIndex - 1;

                        if (ft.actions[iw.idx].passParam)
                            fireTags[idx].param = Random.Range(ft.actions[iw.idx].paramRange.x, ft.actions[iw.idx].paramRange.y);
                        else if (ft.actions[iw.idx].passPassedParam)
                            fireTags[idx].param = ft.param;

                        if (fireTags[idx].actions.Length > 0)
                            yield return StartCoroutine(RunFire(idx));
                        break;

                    case (FireActionType.StartRepeat):
                        yield return StartCoroutine(RunNestedFire(i, iw));
                        break;
                }

            }
        }
    }
    //This is basically the same function as above but for nested FireTag actions starting of with a StartRepeat action
    // this function ends and resumes to teh above function on a EndRepeat action. There is some duplicated code,
    // but I decided to avoid any extra function calls, and completely combining these two functions is too complicated to be worth it
    IEnumerator RunNestedFire(int i, IndexWrapper iw)
    {
        var ft = fireTags[i];
        var startIndex = iw.idx;
        var endIndex = 0;

        var repeatC = ft.actions[iw.idx].repeatCount.x;
        if (ft.actions[iw.idx].rankRepeat)
            repeatC += ft.actions[iw.idx].repeatCount.y * BulletManager.instance.rank;
        repeatC = Mathf.Floor(repeatC);

        iw.idx++;

        for (var y = 0; y < repeatC; y++)
        {
            while (ft.actions[iw.idx].type != FireActionType.EndRepeat)
            {
                switch (ft.actions[iw.idx].type)
                {
                    case (FireActionType.Wait):
                        float waitT;
                        if (ft.actions[iw.idx].randomWait)
                            waitT = Random.Range(ft.actions[iw.idx].waitTime.x, ft.actions[iw.idx].waitTime.y);
                        else
                            waitT = ft.actions[iw.idx].waitTime.x;
                        if (ft.actions[iw.idx].rankWait)
                            waitT += BulletManager.instance.rank * ft.actions[iw.idx].waitTime.z;
                        waitT *= BulletManager.instance.timePerFrame;
                        yield return new WaitForSeconds(waitT);
                        break;

                    case (FireActionType.Fire):
                        Fire(tform, ft.actions[iw.idx], ft.param, ft.prw);
                        break;

                    case (FireActionType.CallFireTag):
                        var idx = ft.actions[iw.idx].fireTagIndex - 1;

                        if (ft.actions[iw.idx].passParam)
                            fireTags[idx].param = Random.Range(ft.actions[iw.idx].paramRange.x, ft.actions[iw.idx].paramRange.y);
                        else if (ft.actions[iw.idx].passPassedParam)
                            fireTags[idx].param = ft.param;

                        if (fireTags[idx].actions.Length > 0)
                            yield return StartCoroutine(RunFire(idx));
                        break;

                    case (FireActionType.StartRepeat):
                        yield return StartCoroutine(RunNestedFire(i, iw));
                        break;
                }

                iw.idx++;

            }

            endIndex = iw.idx;
            iw.idx = startIndex + 1;
        }

        iw.idx = endIndex;
    }

}

//A FireTag is a procedure of sorts that is exposed to the GUI Inspector in Unity, it controls tha BulletPattern logic flow
[System.Serializable]
public class FireTag
{
    [SerializeField]
    public float param = 0f;
    [SerializeField]
    public PrevRotWrapper prw = new PrevRotWrapper();
    [SerializeField]
    public FireAction[] actions;
}
// "BulletPatternAction", this is all the action related variables that both FireTag and Bullet actions have in common
[System.Serializable]
public class BPAction
{
    [SerializeField]
    public Vector3 waitTime;
    [SerializeField]
    public bool randomWait = false;
    [SerializeField]
    public bool rankWait = false;

    [SerializeField]
    public DirectionType direction;
    [SerializeField]
    public Vector3 angle;
    [SerializeField]
    public bool randomAngle = false;
    [SerializeField]
    public bool rankAngle = false;

    [SerializeField]
    public bool overwriteBulletSpeed = false;
    [SerializeField]
    public Vector3 speed;
    [SerializeField]
    public bool randomSpeed = false;
    [SerializeField]
    public bool rankSpeed = false;
    [SerializeField]
    public bool useSequenceSpeed = false;

    [SerializeField]
    public int bulletTagIndex = 0;
    [SerializeField]
    public bool useParam = false;

    [SerializeField]
    public int fireTagIndex = 0;
    [SerializeField]
    public Vector2 repeatCount;
    [SerializeField]
    public bool rankRepeat = false;

    [SerializeField]
    public bool passParam = false;
    [SerializeField]
    public bool passPassedParam = false;
    [SerializeField]
    public Vector2 paramRange;
}
//only thing specific to FireTagActions is the name of actual action type, bullet actions have their own unique types
[System.Serializable]
public class FireAction : BPAction
{
    [SerializeField]
    public FireActionType type = FireActionType.Wait;
}

[SerializeField]
public enum FireActionType { Wait, Fire, CallFireTag, StartRepeat, EndRepeat };

//Not to be confused with an actual Bullet object. Like a fireTag, it is not physical but contains 
//information important for creating and controlling bulets
//[System.Serializable]
[System.Serializable]
public class BulletTag
{
    [SerializeField]
    public Vector3 speed;
    [SerializeField]
    public bool randomSpeed = false;
    [SerializeField]
    public bool rankSpeed = false;
    [SerializeField]
    public int prefabIndex = 0;

    [SerializeField]
    public BulletAction[] actions;
}
//wrapper for passing by reference, so we can track each tag's previous fire rotation
[System.Serializable]
public class PrevRotWrapper
{
    [SerializeField]
    public Quaternion previousRotation;
    [SerializeField]
    public bool prevRotationNull = true;
}


[System.Serializable]
public class ActionFoldouts
{
    [SerializeField]
    public bool main = false;
    [SerializeField]
    public List<bool> sub = new List<bool>();
}

[SerializeField]
public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence };

