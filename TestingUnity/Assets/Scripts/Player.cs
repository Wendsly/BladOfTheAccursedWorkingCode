using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    //Making the variables public so they can be changed on the fly 
    public float moveSpeed = 6;
    float gravity;
    Vector3 velocity;
    public float MaxJumpHeight = 4;//how hight do you want to jump
    public float MinJumpHeight = 1;
    public float timeToJumpApex = .4f;//how long does it take to reach max height
    public float dashSpeed = 5;
    float MaxJumpVelocity;
    float MinJumpVelocity;
    Controller2D controller;
    float velocityXSmoothing;
    public float AccelerationTimeAirborn = .2f;
    public float accelerationTimeGrounded = .2f;
    public Vector3 moveDirection;
    public float MaxDashTime = 1.0f;
    public float dashStoppingSpeed = 0.1f;

    private float currentDashTime;
    Vector2 directionalInput;
    void Start()
    {
        //Make sure we get all of the information in the controller2D script
        controller = GetComponent<Controller2D>();
        //using the Equation deltaMovement = velocityInitial*time + ((acceleration *time^2)/2) to calculate the final velocity
        //so jumpHeight = (gravity*timeToJumpApex^2)/2)
        //for gravity  gravity = 2*jumpHeight/TimeToJumpApex^2
        gravity = -(2 * MaxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        //for jump velocity jumpVelocity = gravity * timeToJumpApex
        MaxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);
        print("Gravity: " + gravity + " Jump Velocity:  " + MaxJumpVelocity);
        //Set the dashtime
        currentDashTime = MaxDashTime;
    }
    void Update()
    {
        CalculateVelocity();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //set up a dash using the shift key. 

        }
        controller.Move(velocity * Time.deltaTime, directionalInput);
        //This is here because the moving platform is also calling the above and below values so we need to make sure that they are present after they have been called as well
        if (controller.collisions.above || controller.collisions.below)
        {
            if (!controller.collisions.slideingDownMaxSlope)
            {
                velocity.y = 0;
            } 
           
        }
    }
    //This is all called from the player input script
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }
    public void OnJumpInputDown()
    {
            velocity.y = MaxJumpVelocity;
    }
    public void OnJumpInputUp()
    {
            if (velocity.y > MinJumpVelocity)
            {
                velocity.y = MinJumpVelocity;
            }
        
    }
    
    //This handles all of the velocity
    void CalculateVelocity()
    {
        //This stops the gravity accumulating
        //Applying the move speed and gravity to the player and also to the move function in Controller 2D
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : AccelerationTimeAirborn);
        velocity.y += gravity * Time.deltaTime;
    }
}