using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoyconDemo : MonoBehaviour {

    //regular variable declorations
    private List<Joycon> joycons;                           //list of Joycon variables, and contains all the detected joycons

    private Quaternion lastUpdatedOri = new Quaternion();   //this Quaternion stores the current visual positionj of the jar...
    private Quaternion lastFramesOri = new Quaternion();    //..while this one tracks the actual position of the jar from the previous itteration of Update()

    private int dcCounter = 0;                              //dcCounter tracks how many itterations of update have had the exact same gyro input in a row

    //constant variable declorations
    private const double MIN_MOVE = 0.0095;                 //this variable holds the minimum difference between the jars current visual position &...
                                                            //...the new gyrocope input for the updatePosition() function to be called

    private const int MAX_DC = 60;                          //this variable tracks the maximum amount of itterations of Update() that can occur with the same...
                                                            //gyro input in a row before the player is exited from the JarScene.
                                                            //(this will be explained more later)

    //public variable declorations
    public int jc_ind = 0;                                  //not too sure what this one does, but if I mess w/ it stuff breaks
    public Quaternion orientation;                          //orientation stores the gyro input from the joycon to be applied to the jar

    void Start() {
        //this gets the list of Joycons from JoyconManager and assigns it to a local variable
        joycons = JoyconManager.Instance.j;
    }//end Start

    void updatePosition() {   //update position updates the visual positioning of the jar

        //Applies the current axis stored in orientation to the gameObject, also needs to be inverted for reasons I don't understand
        gameObject.transform.rotation = Quaternion.Inverse(orientation);

        //the jar is always oriented upside down - this code fixes that. It needs to be at the end or it'll get overwritten by the other rotational code
        gameObject.transform.Rotate(180, 0, 0, Space.World);

        lastUpdatedOri = orientation;   //lastUpdatedOri stores the current visual position of the jar, and is updated everytime the position itself is updated
                                        //since we just updated the position, lastUpdatedOri is as well
    }//end updatePosition

    void Update() {
        Joycon j = joycons[jc_ind];  //This makes a local Joycon variable called j, which it gets from JoyconManager
                                     //I think anyways, I don't fully understand this code, I just know I break everything if I remove it

        // checks for a ZR press, and recalibrates the gyro if it's pressed
        if (j.GetButtonDown(Joycon.Button.SHOULDER_2)) {
            Debug.Log("Recalibrated!");
            j.Recenter();
        }//end if

        orientation = j.GetVector();    //GetVector is a function that returns a Quaternion value of the Joycons orientation
                                        //the Quaternion orientation is assigned this value

        //this line states what order the different axis are stored in - unity defaults to a y,x,z,w order, which doesn't match the joycon input order
        orientation = new Quaternion(orientation.x, orientation.z, orientation.y, orientation.w);

        orientation.x = orientation.x * -1; //These lines invert x and y between positive and negative values, which is done by multiplying it by -1
        orientation.y = orientation.y * -1; //I have no clue why, but after hours of fiddiling, this is what fixed issues I was having with rotation...
                                            //...happening along an incorrect axis (consult github issues for a more thorough explenation)

        if (orientation == lastFramesOri) {     //this if checks whether the current orientation of the Joycon matches the previous itterations
            dcCounter++;                        //if it does, it increments dcCounter by 1
            if (dcCounter == MAX_DC) {              //it then checks if dcCounter is equal to MAX-DC
                SceneManager.LoadScene("Menu_Disconect", LoadSceneMode.Single);     //if it is, the user is then taken out of the jar scene and moved to a UI menu

        //I go into more detail on github, but the reason I'm doing this is basically because the libary code doesn't check for joycon disconnection.
        //But, in the instance that a Joycon does disconnect, the gyro input recieved will cease to update so the program assumes a lack of input...
        //...is due to a DC

        //if the orientation from the last itteration and the current orientation don't match, this else is called instead
        }} else {

            //this code checks how much the joycon has moved since the last time its position was updated visually
            //if the difference along a particular axis is less than MIN_MOVE, the position is invisibally updated but not visually
            //using math.abs returns the absolute value, so it finds the difference w/o potentially returning a negative number, as a negative will always be less than MIN_MOVE
            if (Math.Abs(orientation.x - lastUpdatedOri.x) > MIN_MOVE) {
                updatePosition();
            }  else if (Math.Abs(orientation.y - lastUpdatedOri.y) > MIN_MOVE) {
                updatePosition();
            } else if (Math.Abs(orientation.z - lastUpdatedOri.z) > MIN_MOVE) {
                updatePosition();
            } else if (Math.Abs(orientation.w - lastUpdatedOri.w) > MIN_MOVE) {
                updatePosition();
            }//end if

            dcCounter = 0;      //dcCounter is updated, since this else is only triggered if the Joycon orientation has changed
            lastFramesOri = orientation;    //now at the end of Update
        }//end else
    }//end Update

}//end class