using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class JoyconDemo : MonoBehaviour
{

    private List<Joycon> joycons;
    private Quaternion lastUpdatedOri = new Quaternion();
    private const double MIN_MOVE = 0.0095;

    // Values made available via Unity
    public int jc_ind = 0;
    public Quaternion orientation;

    void Start()
    {

        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
    }


    // Update is called once per frame
    void Update()
    {    //this loop contains code that will run for each instance of a joycon
         //all the code to update the jar's position is found here, as well as joycon recalibration and other joycon functions

        //since we only ever want one joycon connected, this code will cease to run if no joycons or more than 1 joycon is connected
        if (joycons.Count == 1)
        {

            Joycon j = joycons[jc_ind];  //idk what this does but I'm assuming it's important because the program breaks if I remove it

            orientation = j.GetVector();    //GetVector is a function that returns a Quaternion value of the Joycons orientation
                                            //the Quaternion orientation is assigned this value

            //this line states what order the different axis are stored in - unity defaults to a y,x,z,w order, which doesn't match the joycon input order
            orientation = new Quaternion(orientation.x, orientation.z, orientation.y, orientation.w);

            orientation.x = orientation.x * -1; //These lines invert x and y between positive and negative values, which is done by multiplying it by -1
            orientation.y = orientation.y * -1; //I have no clue why, but after hours of fiddiling, this is what fixed issues I was having with rotation...
                                                //...happening along an incorrect axis (consult github issues for a more thorough explenation)

            // checks for a ZR press, and recalibrates the gyro if it's pressed
            if (j.GetButtonDown(Joycon.Button.SHOULDER_2))
            {
                Debug.Log("Recalibrated!");
                j.Recenter();
            }//end if

            //this code checks how much the joycon has moved since the last time its position was updated visually
            //if the difference along a particular axis is less than MIN_MOVE, the position is invisibally updated but not visually
            if (Math.Abs(orientation.x - lastUpdatedOri.x) > MIN_MOVE)
            {
                updatePosition();
            }
            else if (Math.Abs(orientation.y - lastUpdatedOri.y) > MIN_MOVE)
            {
                updatePosition();
            }
            else if (Math.Abs(orientation.z - lastUpdatedOri.z) > MIN_MOVE)
            {
                updatePosition();
            }
            else if (Math.Abs(orientation.w - lastUpdatedOri.w) > MIN_MOVE)
            {
                updatePosition();
            }//end if

        }//end if

        else if (joycons.Count >= 2)
        {
            Debug.Log("Too many joycons!");
        }

        else if (joycons.Count == 0)
        {
            Debug.Log("No joycons connected!");
        }
    }//end void

    void updatePosition()   //update position updates the visual positioning of the jar
    {
        //Applies the current axis stored in orientation to the gameObject, also needs to be inverted for reasons I don't understand
        gameObject.transform.rotation = Quaternion.Inverse(orientation);

        //the jar is always oriented upside down - this code fixes that. It needs to be at the end or it'll get overwritten by the other rotational code
        gameObject.transform.Rotate(180, 0, 0, Space.World);

        lastUpdatedOri = orientation;   //lastUpdatedOri stores the current visual position of the jar, and is updated everytime the position itself is updated
    }

}//end class