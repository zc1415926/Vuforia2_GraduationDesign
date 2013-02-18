#pragma strict

// Objects to drag in
public var motor : MovementMotor;
public var joystickPrefab : GameObject;

private var joystickLeft : Joystick;
private var joystickRight : Joystick;

private var mainCameraTransform : Transform;

// Private memeber data
private var mainCamera : Camera;

private var joystickRightGO : GameObject;

private var screenMovementSpace : Quaternion;
private var screenMovementForward : Vector3;
private var screenMovementRight : Vector3;


function Awake()
{
	motor.movementDirection = Vector2.zero;
	motor.facingDirection = Vector2.zero;
	
	// Set main camera
	mainCamera = Camera.main;
	mainCameraTransform = mainCamera.transform;
	
#if UNITY_IPHONE || UNITY_ANDROID

	if(joystickPrefab)
	{
		var joystickLeftGO : GameObject = Instantiate(joystickPrefab) as GameObject;
		joystickLeftGO.name = "Joystick Left";
		joystickLeft = joystickLeftGO.GetComponent.<Joystick>();
		
		joystickRightGO = Instantiate(joystickPrefab) as GameObject;
		joystickRightGO.name = "Joystick Right";
		joystickRight = joystickRightGO.GetComponent.<Joystick>();
		
		
	}

#else

#endif
	
}

function Start () 
{
#if UNITY_IPHONE || UNITY_ANDROID
	
	var guiTex : GUITexture = joystickRightGO.GetComponent.<GUITexture>();
	guiTex.pixelInset.x = Screen.width - guiTex.pixelInset.x - guiTex.pixelInset.width;
#endif
	
	screenMovementSpace = Quaternion.Euler (0, mainCameraTransform.eulerAngles.y, 0);
	screenMovementForward = screenMovementSpace * Vector3.forward;
	screenMovementRight = screenMovementSpace * Vector3.right;	
}

function OnDisable () {
	if (joystickLeft) 
		joystickLeft.enabled = false;
	
	if (joystickRight)
		joystickRight.enabled = false;
}

function OnEnable () {
	if (joystickLeft) 
		joystickLeft.enabled = true;
	
	if (joystickRight)
		joystickRight.enabled = true;
}

function Update () 
{
		// HANDLE CHARACTER MOVEMENT DIRECTION
	#if UNITY_IPHONE || UNITY_ANDROID
		motor.movementDirection = joystickLeft.position.x * screenMovementRight + joystickLeft.position.y * screenMovementForward;
	#else
		motor.movementDirection = Input.GetAxis ("Horizontal") * screenMovementRight + Input.GetAxis ("Vertical") * screenMovementForward;
	#endif
	
	// Make sure the direction vector doesn't exceed a length of 1
	// so the character can't move faster diagonally than horizontally or vertically
	if (motor.movementDirection.sqrMagnitude > 1)
		motor.movementDirection.Normalize();

}