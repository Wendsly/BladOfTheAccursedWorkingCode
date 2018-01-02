using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour {
    //What we are following
    public Transform target;
    //sets the velocity to zero
    Vector3 Velocity = Vector3.zero;

    //Time to follow target
    public float SmoothTime = 15f;

    //Enable and set the max and min Y value 
    public bool YMaxEnabled = false;
    public float YMaxValue = 0;
    public bool YMinEnabled = false;
    public float YMinValue = 0;


    //enable and set the min and max x value
    public bool XMaxEnabled = false;
    public float XMaxValue = 0;

    public bool XMinEnabled = false;
    public float XMinValue = 0;

    void FixedUpdate()
    {
        //target position 
        Vector3 targetPos = target.position;

        //Vertical 
        if (YMinEnabled && YMaxEnabled)
        
            targetPos.y = Mathf.Clamp(target.position.y, YMinValue, YMaxValue);
        else if (YMinEnabled)
        {
            targetPos.y = Mathf.Clamp(target.position.y, YMinValue, target.position.y);
        }
        else if (YMaxEnabled)
        {
            targetPos.y = Mathf.Clamp(target.position.y, target.position.y, YMaxValue);
        }
        //Horizontal
        if (XMinEnabled && XMaxEnabled)

            targetPos.x = Mathf.Clamp(target.position.x, XMinValue, XMaxValue);
        else if (XMinEnabled)
        {
            targetPos.x = Mathf.Clamp(target.position.x, XMinValue, target.position.x);
        }
        else if (XMaxEnabled)
        {
            targetPos.x = Mathf.Clamp(target.position.x, target.position.x, XMaxValue);
        }
        //Align the camera and the target Z pos
        targetPos.z = transform.position.z;

        //gradaully chage the position 
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref Velocity, SmoothTime);
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "boundary")
        {
            XMaxEnabled = true;
            XMaxValue = 5;
            XMinEnabled = true;
            XMinValue = 1;
        }

            

    }
}
