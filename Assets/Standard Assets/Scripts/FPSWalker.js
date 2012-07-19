Screen.showCursor = false;
var speed = 6.0; 
var jumpSpeed = 8.0; 
var gravity = 100.0;


private var moveDirection = Vector3.zero; 
private var grounded : boolean = false; 

function FixedUpdate() { 



   if (!grounded) { 
      moveDirection.x = Input.GetAxis("Horizontal")*speed; 
      moveDirection.z = Input.GetAxis("Vertical")*speed; 
   } 
   // If grounded, zero out y component and allow jumping 
   else { 
      moveDirection = Vector3(Input.GetAxis("Horizontal")*speed, 0.0, Input.GetAxis("Vertical")*speed); 
      if (Input.GetButton ("Jump")) { 
         moveDirection.y = jumpSpeed; 
      } 
   } 
   moveDirection = transform.TransformDirection(moveDirection); 

   // Apply gravity 
   moveDirection.y -= gravity * Time.deltaTime; 
    
   // Move the controller 
   grounded = (GetComponent(CharacterController).Move(moveDirection * Time.deltaTime) & CollisionFlags.CollidedBelow) != 0; 
} 

@script RequireComponent(CharacterController)