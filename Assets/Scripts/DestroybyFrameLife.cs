using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroybyFrameLife : MonoBehaviour {

    public int lifetimeFrames;
    public int life;
    public GameObject gameController;
    public GameObject bulletDeath;
    // Use this for initialization
  
	void Start () {
        life = 0;
	}
	
	// Update is called once per frame
	void Update () {
        life++;
        if (life>=lifetimeFrames)
        {
            //gameController.bulletDeath(gameObject);
            if ((tag=="EnemyBullet") || (tag=="PlayerBullet"))
            {
                Instantiate(bulletDeath, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }
	}
}
