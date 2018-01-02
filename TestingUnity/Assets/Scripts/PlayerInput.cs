using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof (Player))]
public class PlayerInput : MonoBehaviour {
    Player player;
    Controller2D controller;
	// Use this for initialization
	void Start () {
        player = GetComponent<Player>();
        controller = GetComponent<Controller2D>();
    }
	
	// Update is called once per frame
	void Update () {
        //Setting it so when your sliding you can't do anything, this is a bug fix for being able to jump up slopes, will fix this later
        if (!controller.collisions.descendingSlope)
        {


            Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            player.SetDirectionalInput(directionalInput);
            
            
            if (Input.GetKeyDown(KeyCode.W) && !controller.collisions.slideingDownMaxSlope)
            {
                player.OnJumpInputDown();
            }
            if (Input.GetKeyUp(KeyCode.W) && !controller.collisions.slideingDownMaxSlope)
            {
                player.OnJumpInputUp();
            }
        }
    }
}
