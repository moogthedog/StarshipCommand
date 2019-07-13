using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagedMob : MonoBehaviour {

    public float speed;
    public float angle;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public Vector2 velocity()
    {
        return Helper.RBVelocity(angle, speed);
    }
}
