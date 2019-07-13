using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerJumping : MonoBehaviour {

    public GameObject player;
    public float maxDistance;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // if the object has got too far from the player, jump it in front of them.
        float _distance = Vector3.Distance(transform.position, player.transform.position);
        if (_distance>maxDistance)
        {
            float _playerAngle = player.transform.rotation.eulerAngles.z;
            Vector3 _playerPosition = player.transform.position;
            // create a new position just inside the required distance, in front of the player
            Vector2 _newPosition = Helper.MakeDistanceCoords(maxDistance * 0.9f, _playerAngle, _playerAngle);
            transform.position = new Vector3(_playerPosition.x-_newPosition.x, _newPosition.y+_playerPosition.y, 0);
        }
		
	}
}
