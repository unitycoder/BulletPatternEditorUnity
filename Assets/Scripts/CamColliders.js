//#pragma strict
//#pragma implicit
//#pragma downcast

static var tform : Transform;
static var use : CamColliders;

var bottom : Transform;
var top : Transform;
var right : Transform;
var left : Transform;

var collideMask : LayerMask;

var bottomPos : float;
var topPos : float;
var rightPos : float;
var leftPos : float;

function Awake()
{
	use = this;
	tform = transform;
	
	bottomPos = bottom.localPosition.y;
	topPos = top.localPosition.y;
	rightPos = right.localPosition.x;
	leftPos = left.localPosition.x;
	
	
}

function IsInsideBox(pos : Vector3) : boolean
{		
	if(Physics.Linecast(pos, tform.position, collideMask))
		return false;
	else 
		return true;
}

function FindPointInBox(pos : Vector3) : Vector3
{
	var rayDir = (pos - tform.position).normalized;
	var ray = new Ray (tform.position, rayDir);
	var hit : RaycastHit;
		
	if(Physics.Raycast (ray, hit, 100, collideMask))
	{
		return hit.point + (-3 * rayDir);
		
	}
}






