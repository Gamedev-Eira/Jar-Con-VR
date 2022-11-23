using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.SceneManagement;
using System;

//E: pretty much all the code here is pre-written and from the libary, including comments. I don't understand 99% of ths code
//E: I have made a few very minor changes, which I will point out via comment
//E: Any comments I make will begin with 'E:'

public class JoyconManager: MonoBehaviour
{
    // Settings accessible via Unity
    public bool EnableIMU = true;
    public bool EnableLocalize = true;

	// Different operating systems either do or don't like the trailing zero
	private const ushort vendor_id = 0x57e;
	private const ushort vendor_id_ = 0x057e;
	private const ushort product_l = 0x2006;
	private const ushort product_r = 0x2007;

    public List<Joycon> j; // Array of all connected Joy-Cons
    static JoyconManager instance;

    public static JoyconManager Instance
    {
        get { return instance; }
    }

    //E: Awake() runs when JoyconManager is being loaded, which happens when a scene it's attached to is loading
    //E: it initalises a list of all the Joycons currently connected, initalises some data (EG, whether it's a left or right joycon), and puts it in a list
    //E: that list will be sent to JoyconDemo, which is called from JoyconManager
    
    //E: However, I added code to Awake that takes the user to a UI scene if the wrong amount of Joycons is found to be connected
    //E: JoyconManager is connected to 2 Scenes - Menu_AllSet (which doesn't make use of JoyconDemo, so doesn't use the Joycon for anything) and JarScene.

    //E: Menu_AllSet acts like a middle-man between scenes - if it's loaded and the user has too many or no Joycons connected, it immedietly kicks them to different UI...
    //E: ...that explains the issue. These UI menus can then reload Menu_AllSet, which reloads Awake() and resets the Joycon List
    //E: And if the issue is still unfixed, they are kicked out again. Since this happens before the scene is loaded, it will look to the user like they've just been denied access.
    
    //E: If the issue is fixed, or there wasn't one to begin with, Menu_AllSet will allow you to load the JarScene
    //E: And since JarScene loads JoyconManager (and therefore Awake() as well), if an issue occurs between getting to Menu_AllSet and loading JarScene, the user will still get taken to another...
    //E: UI element instead of JarScene, and the loop described here starts again

    void Awake()
    {
        //if (instance != null) Destroy(gameObject); E: don't want this line, but didn't outright delete it incase it breaks something
        instance = this;
        int i = 0;

        j = new List<Joycon>();
        bool isLeft = false;
        HIDapi.hid_init();

        IntPtr ptr = HIDapi.hid_enumerate(vendor_id, 0x0);
        IntPtr top_ptr = ptr;

        if (ptr == IntPtr.Zero)
        {
            ptr = HIDapi.hid_enumerate(vendor_id_, 0x0);
            if (ptr == IntPtr.Zero)
            {
                HIDapi.hid_free_enumeration(ptr);
                Debug.Log("No Joy-Cons found!");
            }
        }

        hid_device_info enumerate;
        while (ptr != IntPtr.Zero)
        {
            enumerate = (hid_device_info)Marshal.PtrToStructure(ptr, typeof(hid_device_info));
            Debug.Log(enumerate.product_id);
            if (enumerate.product_id == product_l || enumerate.product_id == product_r)
            {
                if (enumerate.product_id == product_l)
                {
                    isLeft = true;
                    Debug.Log("Left Joy-Con connected.");
                }
                else if (enumerate.product_id == product_r)
                {
                    isLeft = false;
                    Debug.Log("Right Joy-Con connected.");
                }//end if

                else
                {
                    Debug.Log("Non Joy-Con input device skipped.");
                }
                IntPtr handle = HIDapi.hid_open_path(enumerate.path);
                HIDapi.hid_set_nonblocking(handle, 1);
                j.Add(new Joycon(handle, EnableIMU, EnableLocalize & EnableIMU, 0.05f, isLeft));
                ++i;
            }
            ptr = enumerate.next;
        }
        HIDapi.hid_free_enumeration(top_ptr);

        //E: here's the code I added - after the list of Joycons has been made it checks whether the list is 0 objects long (no joycons connected) or more than 1 long (more than 1 joycon connected)
        //E: it loads the apropriate UI scene if either of these criteria are met, effectively breaking here and disallowing Start() or Update() to run
		if(j.Count < 1) {
            Debug.Log("No Joy-Cons found!");
            SceneManager.LoadScene("Menu_NoJoyCon", LoadSceneMode.Single); //E: Immediatly takes the user to the "No Joycon" menu if they fail to connect a joycon
        } else if(j.Count > 1) {
            Debug.Log("More than 1 Joy-Con found!");
            SceneManager.LoadScene("Menu_TooManyJoyCon", LoadSceneMode.Single); //E: Immediatly takes the user to the "Too Many Joycon" menu if they connect more than 1 Joycon
        }//end else
    }

    void Start()
    {
		for (int i = 0; i < j.Count; ++i)
		{
			Debug.Log (i);
			Joycon jc = j [i];
			byte LEDs = 0x0;
			LEDs |= (byte)(0x1 << i);
			jc.Attach (leds_: LEDs);
			jc.Begin ();
		}
    }

    void Update()
    {
        for (int i = 0; i < j.Count; ++i)
        {
            j[i].Update();
        }
    }

    void OnApplicationQuit()
    {
		for (int i = 0; i < j.Count; ++i)
		{
			j[i].Detach();
		}
    }
}
