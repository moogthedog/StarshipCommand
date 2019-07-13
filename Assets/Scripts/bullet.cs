using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour {

    public float fireEnergy;
    public float payloadEnergy;
    public GameObject parent;
    public float frameRate;
    public float speed;

    private Rigidbody2D _rb;
    // Use this for initialization


    void Awake () {
        _rb = GetComponent<Rigidbody2D>();
	}

    private void Start()
    {
        // Add our speed to the initial velocity;
        _rb.velocity += (Vector2)transform.up * speed;
    }

    /// <summary>
    /// Add the parent's speed to that already defined for the bullet
    /// </summary>
    /// <param name="parentAngle">Angle of the parent</param>
    /// <param name="parentFrameSpeed">Frame speed of the parent</param>
    public void SetSpeed(float parentAngle, float parentFrameSpeed)
    {
        Vector2 _parentVelocity = Helper.MakeDistanceCoords(parentFrameSpeed, parentAngle, parentAngle);
        _parentVelocity *= frameRate;

        _rb.velocity += new Vector2(-_parentVelocity.x,_parentVelocity.y);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
