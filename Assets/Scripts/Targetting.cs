using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetting : MonoBehaviour {
    public GameObject player;

    private PlayerControl pc;
    private ManagedMob playerMob;
    private ManagedMob myMM;
    private EnemyController ec;

    public float frameRate;
    public float fixedRate;
    public float accuracy;

    // player lead array - declared as static, as it should be available to everyone 
    // (and only be calculated once per frame)
    public const int playerLeadTime = 120;
    public static Vector2[] playerLead = new Vector2[playerLeadTime];
    private static bool playerLeadUpdated = false;

    private GameObject[] trail = new GameObject[playerLeadTime];
    public GameObject trailObject;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("PlayerShip");
        pc = player.GetComponent<PlayerControl>();
        playerMob = player.GetComponent<ManagedMob>();
        myMM = GetComponent<ManagedMob>();
        ec = GetComponent<EnemyController>();
        for (int _count = 0; _count < playerLeadTime; _count++)
        {
            //trail[_count] = Instantiate(trailObject, transform.position, transform.rotation);
        }
	}

    private void UpdatePlayerLead()
    {
        // update once per frame
        if (playerLeadUpdated) return;

        float _playerAngle = player.transform.rotation.eulerAngles.z;
        float _playerFrameTurnRate = pc.turn;
        
        // where will the player be in one frame, if he flies straight forward
        Vector2 _playerFrameVector = new Vector2(Mathf.Sin(_playerAngle * Mathf.Deg2Rad) * playerMob.speed, Mathf.Cos(_playerAngle * Mathf.Deg2Rad) * playerMob.speed);
                
        // start position
        Vector2 _playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);
        playerLead[0] = _playerPosition;
        float _ang;
        for (int _frameCount = 1; _frameCount < playerLeadTime; _frameCount++)
        {
            _ang = -_playerAngle-(_frameCount * _playerFrameTurnRate);
            playerLead[_frameCount] = playerLead[_frameCount - 1] + Helper.MakeDistanceCoords(playerMob.speed, _ang, _ang);
        }

        playerLeadUpdated = true;
    }

    public void LateUpdate()
    {
        // reset the playerleadupdate flag at the end of the frame
        playerLeadUpdated = false;
    }

    public bool HaveFiringSolution()
    {
        // update the player lead data (only happens once per frame)
        // (all targetting solutions access the same copy)
        UpdatePlayerLead();

        // What's *my* offset per frame?
        Vector2 _enemyFrameVector = myMM.velocity() / fixedRate * frameRate;

        // and my bullet's?
        // (we can't use bullet.velocity, as we don't have a instance to check, so work it out the hard way)
        float _enemyAngle = transform.rotation.eulerAngles.z;
        Vector2 _enemyFixedBulletVector = new Vector2(-Mathf.Sin(_enemyAngle * Mathf.Deg2Rad) * ec.bulletSpeed, Mathf.Cos(_enemyAngle * Mathf.Deg2Rad) * ec.bulletSpeed);
        // velicoty is distance per second?
        Vector2 _enemyFrameBulletVector = _enemyFixedBulletVector / fixedRate;// * frameRate;

        // An instantiated bullet would have my speed on top of its own
        _enemyFrameBulletVector += _enemyFrameVector;

        // start positions
        Vector2 _playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);
        Vector2 _enemyPosition = new Vector2(transform.position.x, transform.position.y);

        // so, now we find a suitable intersection point
        int _bulletLife = playerLeadTime;
        float _oldIntersection = 2000;
        float _intersection;
        bool _intersectionFound = false;
        Vector2 _player;
        Vector2 _bullet;

        // age the bullet and player vectors until they either collide or start moving apart
        for (int _frameCount = 1; _frameCount < _bulletLife; _frameCount++)
        {
            _player = playerLead[_frameCount];
            _bullet = _enemyPosition + (_enemyFrameBulletVector * _frameCount);
            // _bullet v2 is implicitly coerced to a transform v3 at z=0;
            //trail[_frameCount].transform.position = _bullet;
            _intersection = (_player - _bullet).magnitude;

            if (_intersection <= accuracy)
            {
                // WE GOT ONE! [/janice]
                _intersectionFound = true;
                break; // out of FOR loop
            }

            if (_intersection>_oldIntersection)
            {
                // the bullet and the player positions have started moving apart.  Stop here
                break; // out of FOR loop
            }
            _oldIntersection = _intersection;
        }

        return _intersectionFound;
    }
	
	// Update is called once per frame
	void Update () {
        
	}
}
