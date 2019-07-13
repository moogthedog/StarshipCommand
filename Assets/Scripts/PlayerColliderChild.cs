using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderChild : MonoBehaviour {

    public PlayerControl playerControl;

    private void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("PlayerShip").GetComponent<PlayerControl>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // pass the collision up to our parent controller
        playerControl.OnTriggerEnter2D(collision);
    }
}
