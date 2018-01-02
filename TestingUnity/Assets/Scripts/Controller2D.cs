using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{

    public collisionInfo collisions;

    public float maxSlopeAngle = 50;
    Vector2 playerInput;
    //this lets us call the start function in raycast controller as well as here
    public override void Start()
    {
        base.Start();

    }
    //This just means that the moving platform doesn't have to worry about the input method used in the Move function below and will use this one instead
    public void Move(Vector2 MoveAmount, bool standingOnPlatform)
    {
        Move(MoveAmount, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 MoveAmount, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.MoveAmountOld = MoveAmount;
        playerInput = input;
        if (MoveAmount.y < 0)
        {
            descendSlope(ref MoveAmount);
        }
        //This is making sure that the Horizontal collisons and vertical collisions only change if the object is moving
        if (MoveAmount.x != 0)
        {
            HorizontalCollisions(ref MoveAmount);
        }
        if (MoveAmount.y != 0)
        {
            VerticalCollisions(ref MoveAmount);
        }

        transform.Translate(MoveAmount);
        if (standingOnPlatform)
        {
            collisions.below = true;
        }
    }

    void HorizontalCollisions(ref Vector2 MoveAmount)
    {
        //Fisrst set the direction to be the MoveAmount of the player
        float directionX = Mathf.Sign(MoveAmount.x);
        //Get the ray length and add on the skin width, abs is used to make sure that the raylength is always positive
        float rayLength = Mathf.Abs(MoveAmount.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            //if the direction is -1 then the raycast will come from the bottom left otehr wise it will come from the bottom right. 
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            //This gets the origin of the ray
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            //if the ray hits an object to the right of it with a collision mask that we set then it will trigger. 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);
            //When it hits it gets the distance to the obejcet - the skin width and the direction it is faceing. This should cause it to stop. 
            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }
                //get the angle of the surface the we hit. 
                //when the raycast hits a surface it stores the normal, then we have two directions so we can get the angle between them
                //global up and the normal 
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && i <= maxSlopeAngle)
                {
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        MoveAmount = collisions.MoveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        MoveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    climbSlope(ref MoveAmount, slopeAngle);
                    MoveAmount.x += distanceToSlopeStart * directionX;
                }
                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    MoveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;
                    //update MoveAmount on y axis
                    if (collisions.climbingSlope)
                    {
                        MoveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(MoveAmount.x);
                    }
                    //This checks if the collision is left or right then sets the appropriate one in the struct to true
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 MoveAmount)
    {
        float directionY = Mathf.Sign(MoveAmount.y);
        float rayLength = Mathf.Abs(MoveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + MoveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Through")
                {
                    //What this says is if we are underneath and the distance from the ray is 0 then we will ignore the collisions
                    if (directionY == 1 || hit.distance == 0)
                    {

                        continue;
                    }
                    if (collisions.fallingThroughPlatform)
                    {
                        continue;
                    }
                    if (playerInput.y == -1)
                    {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .1f);
                        continue;
                    }
                }
                MoveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
                if (collisions.climbingSlope)
                {
                    //using tan (theta) = y/x but we want x so we get x = y/tan(theta).

                    MoveAmount.x = MoveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(MoveAmount.x);
                }
                //Doing the same thing as above but with below and above using y instead of X
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }

        }
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(MoveAmount.x);
            rayLength = Mathf.Abs(MoveAmount.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * MoveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    MoveAmount.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }
    void climbSlope(ref Vector2 MoveAmount, float slopeAngle)
    {
        //treat the MoveAmount on the x axis as the total distance up the slope that we want to move. 
        //using target move distance + slope angle figure out what the new velocities should be 
        //Math behind it sin (theta) = y/d but we want the y so we get y = d * sin(theta)
        float moveDistance = Mathf.Abs(MoveAmount.x);
        float climbMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (MoveAmount.y <= climbMoveAmountY)
        {
            MoveAmount.y = climbMoveAmountY;
            MoveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(MoveAmount.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }
    void descendSlope(ref Vector2 MoveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(MoveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(MoveAmount.y) + skinWidth, collisionMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref MoveAmount);
        SlideDownMaxSlope(maxSlopeHitRight, ref MoveAmount);


        float directionX = Mathf.Sign(MoveAmount.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        if (!collisions.slideingDownMaxSlope)
        {


            if (hit)
            {
                //get the angle between the normal of the object we are colliding with and up 
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(MoveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(MoveAmount.x);
                            float descendMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            MoveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(MoveAmount.x);
                            MoveAmount.y -= descendMoveAmountY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                        }
                    }
                }

            }
        }
    }
    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                //use Tan(theta) = y -hitdistance / x Want x so s = y-hitdistance/tan(theta)
                moveAmount.x = Mathf.Sign(hit.normal.x) *(Mathf.Abs(moveAmount.y) - hit.distance)/ Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                collisions.slopeAngle = slopeAngle;
                collisions.slideingDownMaxSlope = true;
            }
        }
    }
    void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;
    }

    public struct collisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slideingDownMaxSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 MoveAmountOld;
        public bool fallingThroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slideingDownMaxSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }


}