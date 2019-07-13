using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameToPhysics : MonoBehaviour {
    
    int frameCount = 0;
    int fixedCount = 0;
    float nextTime = 0;
    public int frameSecCount = 0;
    public int fixedSecCount = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time>nextTime)
        {
            frameSecCount = frameCount;
            fixedSecCount = fixedCount;
            frameCount = 0;
            fixedCount = 0;
            nextTime = Time.time + 1;
        }
        frameCount++;
	}

    private void FixedUpdate()
    {
        fixedCount++;
    }
}
