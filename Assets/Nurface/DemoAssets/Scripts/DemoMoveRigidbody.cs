using UnityEngine;
using System.Collections;

public class DemoMoveRigidbody : MonoBehaviour {

    private Rigidbody myRB;

	// Use this for initialization
	void Start () {
        myRB = GetComponent<Rigidbody>();
	}
	
	
	public void PushBall() {
        myRB.AddForce(Camera.main.transform.forward * 1000f);
	}
}
