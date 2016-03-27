//Basic, barebones class for handling Player movement; this definitely isn't what you payed $50 for

var go : GameObject;
var tform: Transform;
var rb : Rigidbody;

var speed = 24.0;

private var hitCount = 0;

function Awake()
{
	go = gameObject;
	tform = transform;
	rb = GetComponent.<Rigidbody>();
}

function Update () 
{
}

function FixedUpdate () 
{
	var targetVelocity = Vector3(0,Input.GetAxis("Vertical"),  Input.GetAxis("Horizontal"));
	targetVelocity = tform .TransformDirection(targetVelocity);
	targetVelocity *= speed ;
   
	var velocityChange = (targetVelocity - rb.velocity);
	
	rb.AddForce(velocityChange, ForceMode.VelocityChange);
}

function OnTriggerEnter(other : Collider)
{
	hitCount++;
	if(other.CompareTag("Bullet"))
	{	
		Debug.Log("Hit " + hitCount + " times!");
	}
}
