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

public class BulletPattern : MonoBehaviour
{

    [HideInInspector]
    GameObject go;
    [HideInInspector]
    Transform tform;

    public FireTag[] fireTags;
    public BulletTag[] bulletTags;

    float sequenceSpeed = 0f;

    bool started = false;
    public float waitBeforeRepeating = 5f;


    //These belong in the Editor class, as they control whether or not the GUI Foldouts are open or closed
    //However by putting them here they will actually SAVE whether or not they are open after deselecting the object
    //Bear in mind this does increase mememory usage somewhat (per BulletPattern object) so remove these on release version if necesaary
    public bool ftFoldout = false;
    public List<bool> ftFoldouts = new List<bool>();
    public bool btFoldout = false;
    public List<bool> btFoldouts = new List<bool>();

    public List<ActionFoldouts> ftaFoldouts = new List<ActionFoldouts>();
    public List<ActionFoldouts> btaFoldouts = new List<ActionFoldouts>();


    public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence };

    //setting these saves a little time on referencing objects later
    void Awake()
    {
        go = gameObject;
        tform = transform;
    }

    //get the ball rolling
    void Start()
    {
        InitiateFire();
    }

    //resume if the script was disabled then enabled, the bool prevents 2 calls to InstantiateFire on first startup
    void OnEnable()
    {
        if (started)
            InitiateFire();
    }

    //start the first FireTag in the array of tags, then repeat until disabled or destroyed
    IEnumerator InitiateFire()
    {
        if (!started)
            yield return new WaitForSeconds(1.0f);

        started = true;

        while (true)
        {
            yield return RunFire(0);
            yield return new WaitForSeconds(waitBeforeRepeating * BulletManager.use.timePerFrame);
        }
    }


    //Gets a bullet and returns it. We look through BulletManager's bullet pool to see if a bullet
    // is available, so we can avoid creating abullet if possible. Infinitely creating and
    // destroying objects is bad for performance.
    Bullet GetInstance(List<Bullet> arr , Transform tform , Bullet prefab)
    {
        for (var i = 0; i < arr.Count; i++)
        {
            var tempGo = (arr[i] as Component).gameObject;

            if (!tempGo.active)
            {
                tempGo.SetActiveRecursively(true);
                return arr[i];
            }
        }

        var temp = Instantiate(prefab, tform.position, tform.rotation) as Bullet;
        arr.Add(temp);
        return temp;
    }

    //creates a bullet and initiales its parameters
    public void Fire(Transform t , BPAction a, float param , PrevRotWrapper prw)
    {
        //find the correct bulletTag that has info for this bullet
        var bt = bulletTags[a.bulletTagIndex - 1];
        //get the bullet
        Bullet temp = GetInstance(BulletManager.use.bullets[bt.prefabIndex].bl, t, BulletManager.use.bulletPrefab[bt.prefabIndex]);

        if (prw.prevRotationNull)
        {
            prw.prevRotationNull = false;
            prw.previousRotation = temp.tform.localRotation;
        }
        //set positions equal to its creator, which could be a Firetag or Bullet
        temp.tform.position = t.position;
        temp.tform.rotation = t.rotation;
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
                ang += BulletManager.use.rank * a.angle.z;
        }
        //actually point the bullet in the right direction
        switch ((DirectionType)a.direction)
        {
            case (DirectionType.TargetPlayer):
                var originalRot = t.rotation;
                var dotHeading = Vector3.Dot(temp.tform.up, BulletManager.use.player.position - temp.tform.position);

                int dir;
                if (dotHeading > 0)
                    dir = -1;
                else
                    dir = 1;
                var angleDif = Vector3.Angle(temp.tform.forward, BulletManager.use.player.position - temp.tform.position);
                temp.tform.rotation = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right);
                break;

            case (DirectionType.Absolute):
                temp.tform.localRotation = Quaternion.Euler(-(ang - 270), 270, 0);
                break;

            case (DirectionType.Relative):
                temp.tform.localRotation = t.localRotation * Quaternion.AngleAxis(-ang, Vector3.right);
                break;

            case (DirectionType.Sequence):
                temp.tform.localRotation = prw.previousRotation * Quaternion.AngleAxis(-ang, Vector3.right);
                break;
        }
        //record this rotation for next Sequence Direction
        prw.previousRotation = temp.tform.localRotation;
        //set the speed, either from creator's speed data
        if (a.overwriteBulletSpeed)
        {
            float spd;
            if (a.randomSpeed)
                spd = Random.Range(a.speed.x, a.speed.y);
            else
                spd = a.speed.x;
            if (a.rankSpeed)
                spd += BulletManager.use.rank * a.speed.z;

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
                temp.speed += BulletManager.use.rank * bt.speed.z;
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
    class IndexWrapper
    {
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
                            waitT += BulletManager.use.rank * ft.actions[iw.idx].waitTime.z;
                        waitT *= BulletManager.use.timePerFrame;
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
                            yield return RunFire(idx);
                        break;

                    case (FireActionType.StartRepeat):
                        yield return RunNestedFire(i, iw);
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
            repeatC += ft.actions[iw.idx].repeatCount.y * BulletManager.use.rank;
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
                            waitT += BulletManager.use.rank * ft.actions[iw.idx].waitTime.z;
                        waitT *= BulletManager.use.timePerFrame;
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
                            yield return RunFire(idx);
                        break;

                    case (FireActionType.StartRepeat):
                        yield return RunNestedFire(i, iw);
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
//[System.Serializable]
public class FireTag
{
    public float param = 0f;
    public PrevRotWrapper prw = new PrevRotWrapper();
    public FireAction[] actions;
}
// "BulletPatternAction", this is all the action related variables that both FireTag and Bullet actions have in common
public class BPAction
{
    public Vector3 waitTime;
    public bool randomWait = false;
    public bool rankWait = false;

    public DirectionType direction;
    public Vector3 angle;
    public bool randomAngle = false;
    public bool rankAngle = false;

    public bool overwriteBulletSpeed = false;
    public Vector3 speed;
    public bool randomSpeed = false;
    public bool rankSpeed = false;
    public bool useSequenceSpeed = false;

    public int bulletTagIndex = 0;
    public bool useParam = false;

    public int fireTagIndex = 0;
    public Vector2 repeatCount;
    public bool rankRepeat = false;

    public bool passParam = false;
    public bool passPassedParam = false;
    public Vector2 paramRange;
}
//only thing specific to FireTagActions is the name of actual action type, bullet actions have their own unique types
public class FireAction : BPAction
{
    public FireActionType type = FireActionType.Wait;
}

public enum FireActionType { Wait, Fire, CallFireTag, StartRepeat, EndRepeat };

//Not to be confused with an actual Bullet object. Like a fireTag, it is not physical but contains 
//information important for creating and controlling bulets
//[System.Serializable]
public class BulletTag
{
    public Vector3 speed;
    public bool randomSpeed = false;
    public bool rankSpeed = false;
    public int prefabIndex = 0;

    public BulletAction[] actions;
}
//wrapper for passing by reference, so we can track each tag's previous fire rotation
public class PrevRotWrapper
{
    public Quaternion previousRotation;
    public bool prevRotationNull = true;
}



public class ActionFoldouts
{
    public bool main = false;
    public List<bool> sub = new List<bool>();
}

public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence };

