// This one is the kid brother everyone takes for granted, 'nuff said

#pragma strict
#pragma downcast

// we hide this stuff from the inspector because no one wants to look at it
// *insert extremely unfunny occupational joke here*
@HideInInspector() 
var go : GameObject;
@HideInInspector() 
var tform: Transform;
@HideInInspector() 
var rb : Rigidbody;

var speed = 5.0;
var verticalSpeed = 0.0;
var useVertical = false;
var lifetime = 0.0;
var prw = new PrevRotWrapper();
var actions : BulletAction[];
var param = 0.0;
var actionIndex = 0;

var master : BulletPattern;

function Awake()
{
	go = gameObject;
	tform = transform;
	rb = GetComponent.<Rigidbody>();
	
	//make sure this bullet knows whos his daddy
	//actually its so 100 bullet objects dont clutter the hierarchy window
	tform.parent = BulletManager.use.transform;
}

function FixedUpdate()
{
	var targetVelocity = tform.forward * speed;
	// dont bother with vertical speed unless its been tampered with (see ChangeSpeedVertical..or was it SpeedChangeVertical?)
	if(useVertical)
		targetVelocity += BulletManager.use.MainCam.up * verticalSpeed;
	var velocityChange = (targetVelocity - rb.velocity);
	rb.AddForce(velocityChange,ForceMode.VelocityChange);
	
	//if you uncomment this stuff the bullet willl die automatically die after awhile
	// look at me tempting you like this 
	
	//lifetime += Time.deltaTime;
	//if(lifetime > BulletManager.use.bulletLifespan)
	//	Deactivate();
	
	
}

// Same deal as the redundant twin functions from BulletPattern that was for FireTagActions
function RunActions() : IEnumerator
{
	for(actionIndex = 0;actionIndex < actions.length; actionIndex++)
	{
		switch(actions[actionIndex].type)
		{
			case(0):			
				if(actions[actionIndex].randomWait)
					var waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
				else
					waitT = actions[actionIndex].waitTime.x;
				if(actions[actionIndex].rankWait)
					waitT += BulletManager.use.rank * actions[actionIndex].waitTime.z;
				waitT *= BulletManager.use.timePerFrame;
				yield WaitForSeconds(waitT);
				break;
			case(1):
				if(actions[actionIndex].waitForChange)
					yield ChangeDirection(actionIndex);
				else
					ChangeDirection(actionIndex);
				break;
			case(2):
				if(actions[actionIndex].waitForChange)
					yield ChangeSpeed(actionIndex, false);
				else
					ChangeSpeed(actionIndex, false);
				break;
			case(3):
				yield RunNestedActions();
				break;
			case(5):
				if(master != null)
					master.Fire(tform, actions[actionIndex], param, prw);
				break;
			case(6):
				if(actions[actionIndex].waitForChange)
					yield ChangeSpeed(actionIndex, true);
				else
					ChangeSpeed(actionIndex, true);
				break;
			case(7):
				Deactivate();
				break;
		}
		
	}
}
//felt bad there was no green text here
function RunNestedActions() : IEnumerator
{
	var startIndex = actionIndex;
	var endIndex = 0;
	actionIndex++;
	
	var repeatC = actions[startIndex].repeatCount.x;
	if(actions[startIndex].rankRepeat)
		repeatC += actions[startIndex].repeatCount.y * BulletManager.use.rank;
	repeatC = Mathf.Floor(repeatC);
	
	for(var y = 0; y < repeatC; y++)
	{
		while(actions[actionIndex].type != 4)
		{
			switch(actions[actionIndex].type)
			{
				case(0):
					if(actions[actionIndex].randomWait)
						var waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
					else
						waitT = actions[actionIndex].waitTime.x;
					if(actions[actionIndex].rankWait)
						waitT += BulletManager.use.rank * actions[actionIndex].waitTime.z;
					waitT *= BulletManager.use.timePerFrame;
					yield WaitForSeconds(waitT);
					break;
				case(1):
					if(actions[actionIndex].waitForChange)
						yield ChangeDirection(actionIndex);
					else
						ChangeDirection(actionIndex);
					break;
				case(2):
					if(actions[actionIndex].waitForChange)
						yield ChangeSpeed(actionIndex, false);
					else
						ChangeSpeed(actionIndex, false);
					break;
				case(3):
					yield RunNestedActions();
					break;
				case(5):
					if(master != null)
						master.Fire(tform, actions[actionIndex], param, prw);
					break;
				case(6):
					if(actions[actionIndex].waitForChange)
						yield ChangeSpeed(actionIndex, true);
					else
						ChangeSpeed(actionIndex, true);
					break;
				case(7):
					Deactivate();
					break;
			}
			
			actionIndex++;
		
		}
		
		endIndex = actionIndex;
		actionIndex = startIndex+1;
	}
	
	actionIndex = endIndex;
}

//activate a bullet, wow what would you do without these comments?
function Activate()
{
	BulletManager.use.bulletCount++;
	lifetime = 0.0;
	verticalSpeed = 0.0;
	useVertical = false;
	prw.prevRotationNull = true;
	RunActions();
}
//we dont actually destroy bullets, were all about recycling
function Deactivate()
{
	if(go.active)
	{
		BulletManager.use.bulletCount--;
		go.SetActiveRecursively(false);
	}
}

//im feeling adventurous so im going to try putting comments INSIDE the function, you ready?
function ChangeDirection(i)
{
	var t = 0.0;
	
	//determine how long this operation will take
	if(actions[i].randomWait)
		var d = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
	else
		d = actions[i].waitTime.x;
	if(actions[i].rankWait)
		d += BulletManager.use.rank * actions[i].waitTime.z;
		
	d *= BulletManager.use.timePerFrame;
	
	var originalRot = tform.localRotation;
	
	// determine offset
	if(actions[i].randomAngle)
		var ang = Random.Range(actions[i].angle.x, actions[i].angle.y);
	else
		ang = actions[i].angle.x;
	if(actions[i].rankAngle)
		ang += BulletManager.use.rank * actions[i].angle.z;
	
	//and set rotation depending on angle
	switch(actions[i].direction)
	{
		case (DirectionType.TargetPlayer):
			var dotHeading = Vector3.Dot( tform.up, BulletManager.use.player.position - tform.position );		
			if(dotHeading > 0)
				var dir = -1;
			else
				dir = 1;
			var angleDif = Vector3.Angle(tform.forward, BulletManager.use.player.position - tform.position);
			var newRot = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right); 
			break;
			
		case (DirectionType.Absolute):
			newRot = Quaternion.Euler(-(ang - 270), 270, 0);
			break;
			
		case (DirectionType.Relative):
			newRot = originalRot * Quaternion.AngleAxis(-ang, Vector3.right);
			break;
			
	}
	
	//Sequence has its own thing going on, continually turning a set amount until time is up
	if(actions[i].direction == DirectionType.Sequence)
	{
		newRot = Quaternion.AngleAxis (-ang, Vector3.right); 
				
		while(t < d)
		{
			tform.localRotation *= newRot;
			t += Time.deltaTime;
			yield WaitForFixedUpdate();
		}
	}
	//all the others just linearly progress to destination rotation
	else if(d > 0)
	{
		while(t < d)
		{
			tform.localRotation = Quaternion.Slerp(originalRot, newRot, t/d);
			t += Time.deltaTime;
			yield WaitForFixedUpdate();
		}
		
		tform.localRotation = newRot;
	}
}

//its basically the same as the above except without rotations
function ChangeSpeed(i, isVertical)
{
	var t = 0.0;
	var s = 0.0;
	
	if(isVertical)
		useVertical = true;
	
	if(actions[i].randomWait)
		var d = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
	else
		d = actions[i].waitTime.x;
	if(actions[i].rankWait)
		d += BulletManager.use.rank * actions[i].waitTime.z;
	d *= BulletManager.use.timePerFrame;	
	
	var originalSpeed = speed;
	
	if(actions[i].randomSpeed)
		var newSpeed = Random.Range(actions[i].speed.x, actions[i].speed.y);
	else
		newSpeed = actions[i].speed.x;
	if(actions[i].rankSpeed)
		d += BulletManager.use.rank * actions[i].speed.z;
	
	if(d > 0)
	{
		while(t < d)
		{
			s = Mathf.Lerp(originalSpeed, newSpeed, t/d);
			if(isVertical) verticalSpeed = s;
			else speed = s;
			t += Time.deltaTime;
			
			yield WaitForFixedUpdate();
		}
	}
	
	if(isVertical) verticalSpeed = newSpeed;
	else speed = newSpeed;
}

//if a bullet leaves the boundary then make sure it actually left the screen, not enetered it
// if it did, then deactivate
// this process MAY be too intensive for iPhone, but then it does keep bullet number under control
function OnTriggerExit(other : Collider)
{
	if(other.CompareTag("Boundary"))
	{	
		if(!CamColliders.use.IsInsideBox(tform.position))
			Deactivate();
	}
}

//exclusive bullet action variables
class BulletAction extends BPAction
{
	var type : BulletActionType = BulletActionType.Wait; 
	var waitForChange = false;
}

enum BulletActionType { Wait, ChangeDirection, ChangeSpeed, StartRepeat, EndRepeat, Fire, 
	VerticalChangeSpeed, Deactivate };

