// This script should be attached to ONE gameObject that is always present
// Its necessary for handling orphan bullets who have no BulletPattern father and is required for all order and harmony in
// the Bullet Universe, hence the name - BulletManager

using UnityEngine;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour
{

    [SerializeField]
    public List<BulletList> bullets = new List<BulletList>();

    public Bullet[] bulletPrefab;

    public Transform player;
    public Transform MainCam;

    public int bulletCount = 0;
    public float rank = 0f;
    public float bulletLifespan = 5f;


    //DO NOT change this value unless you want every time-related pattern action to be affected accordingly
    //The value is multipled by  every user-inputted time variable so that the frame numbers will be converted to smaller
    //second based times that the Unity functions (yield.WaitForSWeconds) will accept.
    [HideInInspector]
    public float timePerFrame = 0.01666f;
    //original value = 0.01666; (approximately one 60th of a second)

    public static BulletManager instance;


    void Awake()
    {
        instance = this;

        for (var i = 0; i < bulletPrefab.Length; i++)
        {
            bullets.Add(new BulletList());
        }
    }


}


[System.Serializable]
public class BulletList
{
    [SerializeField]
    public List<Bullet> bl = new List<Bullet>();
}