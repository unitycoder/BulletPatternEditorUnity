// This script should be attached to ONE gameObject that is always present
// Its necessary for handling orphan bullets who have no BulletPattern father and is required for all order and harmony in
// the Bullet Universe, hence the name - BulletManager

import System.Collections.Generic;
#pragma strict
#pragma downcast

var bullets = new List.<BulletList>();

var bulletPrefab : Bullet[];

var player : Transform;
var MainCam : Transform;

var bulletCount = 0;
var rank = 0.0;
var bulletLifespan = 5.0;

//DO NOT change this value unless you want every time-related pattern action to be affected accordingly
//The value is multipled by  every user-inputted time variable so that the frame numbers will be converted to smaller
//second based times that the Unity functions (yield.WaitForSWeconds) will accept.
@HideInInspector() 
var timePerFrame = 0.01666;
//original value = 0.01666; (approximately one 60th of a second)

static var use : BulletManager;

function Awake()
{
	use = this;
	
	for(var i = 0; i < bulletPrefab.length; i++)
	{
		bullets.Add(new BulletList());
	}
}

class BulletList
{
	var bl = new List.<Bullet>();
}