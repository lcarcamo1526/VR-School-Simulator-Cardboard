using UnityEngine;
using System.Collections;

public class DemoPlayAnimation : MonoBehaviour {

    private Animator myAnim;
    private bool doorOpen;

	// Use this for initialization
	void Start () {
        myAnim = GetComponent<Animator>();
	}

    public void OpenCloseDoor() {
        doorOpen = !doorOpen;
        myAnim.SetBool("DoorOpen", doorOpen);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
