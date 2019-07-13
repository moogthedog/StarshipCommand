using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    public float fireRate;
    public float nextFire = 0;
    public GameObject enemybullet;
    public GameObject enemyExplosion;
    public GameObject spriteRendererObject;
    public GameObject crosshairObject;
    public GameObject bulletDeath;
    public Transform rendererTransform;
    private Game gameScript;
    public Transform bulletSpawnPoint;
    public float bulletSpeed;
    public float bigBulletChance;
    public bool bigBullets;
    
    public float enemyCollisionDamage;

    public float maxSpeed;
    public float maxTurnSpeed;
    public float speedDelta;
    public float turnDelta;
    //public float speed;
    //public float angle;
    
    public float angleToPlayer;
    public Vector2 vectorToPlayer;
    public Vector2 playerAngleVector;
    public float angleToEnemyDestination;
    // how wide is the firing cone that'll make this enemy captain go 'Noooope' and fuck off or cloak.
    public float chutzpah;
    public bool runningAway = false;
    public float runningAwayStartTime = 0;
    public float runningAwaySlowTime = 2;
    public bool nearnessLatch = false;
    public float safetyRange;
    public float nearnessRange;
    public float tooClose;
    public float breakOff;
    public float cloakProbability;

    public string behaviourModel;

    public Vector2 desiredRelativeLocation;
    public float locationAccuracyOuter;
    public float locationAccuracyInner;

    public AudioSource enemyHitSound;
    public AudioSource enemyBounceSound;
    public AudioSource enemyShootSound;

         
    private Energy _e;
    private Rigidbody2D _rb;
    private Visibility _v;
    private Targetting _t;
    private GameObject crosshair;
    private ManagedMob MM;
    private ManagedMob playerMM;
    private PlayerControl playerControl;
    private Cloak cloak;



	// Use this for initialization
	void Start () {
         _e= GetComponent<Energy>();
        _rb = GetComponent<Rigidbody2D>();
        _v = GetComponent<Visibility>();
        _t = GetComponent<Targetting>();


        gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        playerMM = gameScript.player.GetComponent<ManagedMob>();
        playerControl = gameScript.player.GetComponent<PlayerControl>();

        // Shotgun or rifle?
        _t.accuracy = Random.Range(0.6f, 2);
        
        // Gung-ho with his bullets?
        fireRate = Random.Range(0.05f, 1);

        // Once he's tracking us, how close will he get?
        tooClose = Random.Range(3.0f, 4.0f);

        // a chutzpah <=0 means the captain will happily sit right in the player's firing line
        chutzpah = Random.Range(-10, 10);
        rendererTransform = transform.Find("Renderer");
        MM = GetComponent<ManagedMob>();
        crosshair = Instantiate(crosshairObject, transform.position, transform.rotation);

        cloak = GetComponent<Cloak>();
        ChooseNewDestination();

        enemyShootSound = GameObject.Find("EnemyShootSound").GetComponent<AudioSource>();
        enemyBounceSound = GameObject.Find("EnemyBumpSound").GetComponent <AudioSource>();
        enemyHitSound = GameObject.Find("EnemyHurtSound").GetComponent<AudioSource>();

        if (Random.Range(0, 100) < bigBulletChance) bigBullets = true;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject _other = collision.gameObject;
        string _otherTag = _other.tag;
        float _damage = 0;

        if (_otherTag == "EnemyShip")
        {
            enemyBounceSound.Play();
            // They will have received the same event, so just damage ourselves
            _damage = enemyCollisionDamage;
           // velocity is immediately halted
            _rb.velocity = new Vector2(0, 0);
            // as is rotation
            _rb.angularVelocity = 0;
        }
        _e.DoDamage(_damage,4);

    }

    //  All entities deal with their own damage, not anyone else's
    void OnTriggerEnter2D(Collider2D data)
    {
        GameObject _other = data.gameObject;
        string _otherTag = _other.tag;
        float _damage = 0;
        int score=0;

        if ((_otherTag=="EnemyBullet") || (_otherTag=="PlayerBullet") || (_otherTag=="Capsule"))
        {
            bullet _otherBullet = _other.GetComponent<bullet>();
            if (_otherBullet.parent==gameObject)
            {
                // we can't be hit by our own bullets
                return;
            }
            _damage = _otherBullet.payloadEnergy;
            //gameController.DestroyInstance(_other);

            // add an asplosion if the other thing was a capsule, otherwise do the standard bullet death sprite
            if (_otherTag=="Capsule")
            {
                GameObject _explo = Instantiate(enemyExplosion, _other.transform.position, _other.transform.rotation);
                Rigidbody2D _explorb = _explo.GetComponent<Rigidbody2D>();
                _explorb.velocity = _other.GetComponent<Rigidbody2D>().velocity;
                score = 70;
                GGS.capsuleSurvived = false;
            }
            else
            {
                enemyHitSound.Play();
                Instantiate(bulletDeath, _other.transform.position, _other.transform.rotation);
                if (_otherTag == "EnemyBullet")
                {
                    score = 4;
                }
                else
                {
                    if (null==cloak)
                    {
                        score = 8;
                    }
                    else
                    {
                        // for when big ships are implemented
                        score = 12;
                    }
                }
            }
            Destroy(_other);
        }



        // when we collide with the player ship, we're the bullet, so let them deal with it.

        // actioning the damage will destroy us if necessary
        _e.DoDamage(_damage,score);
    }

    private void CreateBullet()
    {
        float _bulletCount;
        if (bigBullets)
        {
            _bulletCount = 4;
        }
        else
        {
            _bulletCount = 1;
        }

        GameObject _b;
        float _bulletAngle = 360 / _bulletCount;
        for (int _count=0; _count<_bulletCount; _count++)
        {
            float _angle = bulletSpawnPoint.transform.rotation.eulerAngles.z;
            _angle += (_count * _bulletAngle);
            Vector2 _offset = Helper.MakeDistanceCoords(0.1f, _angle, _angle);
            _b=Instantiate(enemybullet, bulletSpawnPoint.transform.position+(Vector3)_offset, bulletSpawnPoint.transform.rotation);
            bullet _bb = _b.GetComponent<bullet>();
            _bb.parent = gameObject;
            _bb.SetSpeed(MM.angle, MM.speed);
        }


    }

    /// <summary>
    /// Give the angle relative to our rotation, pointing from us to a destination relative to the player
    /// </summary>
    /// <param name="destination"></param>Position relative to the player
    /// <returns>SignedAngle offset from our rotation to the destination</returns>
    private float AngleToDestination(Vector2 destination)
    {
        Vector2 _destinationPoint = (Vector2)gameScript.player.transform.position + destination;
        Vector2 _vecToDestination =  _destinationPoint-(Vector2)transform.position;
        _vecToDestination = new Vector2(-_vecToDestination.x, _vecToDestination.y);
        float _myAngle = transform.rotation.eulerAngles.z;
        Vector2 _myAngleUnitVector = Helper.MakeDistanceCoords(1, _myAngle, _myAngle);
        return Vector2.SignedAngle(_myAngleUnitVector, _vecToDestination);
    }
    
    private void SlowDown()
    {
        MM.speed = Mathf.Clamp(MM.speed - speedDelta, 0, maxSpeed);
    }

    private void SpeedUp()
    {
        MM.speed=Mathf.Clamp(MM.speed+speedDelta,0,maxSpeed);
    }

    private bool CrappingOurselves()
    {
        // the direction that the player's facing
        float _playerAngle = gameScript.player.transform.rotation.eulerAngles.z+180;
        // the player's direction as a unit vector
        Vector2 _playerDirection = Helper.MakeDistanceCoords(1, _playerAngle, _playerAngle);
        playerAngleVector = _playerDirection;
        // our vector from the player's position
        Vector2 _offset = gameScript.player.transform.position-transform.position;
        _offset = Helper.VectorClamp(_offset, 1, 1);
        
        vectorToPlayer = _offset;
        // if the angle between those two vectors is bigger than our 'courage', run away
        // (note we're not using SignedAngle, so it goes both clockwise and widdershins)
        // (also, a negative chutzpah will never trigger this test)
        float _ang = Vector2.Angle( _offset,_playerDirection);
        angleToPlayer = _ang;
        if (_ang<=chutzpah)
        {
            return true;
        }
        return false;
    }


    private void ChooseNewDestination()
    {
        //desiredRelativeLocation = Helper.MakeDistanceCoords(Random.Range(3,6), 0, 359);
        desiredRelativeLocation = new Vector2(0, 0);
    }


    // Update is called once per frame
    void Update () {

        crosshair.transform.position = gameScript.player.transform.position + (Vector3) desiredRelativeLocation;
        crosshair.GetComponent<DestroybyFrameLife>().life = 0;

        float _turnAngle = 0;
        // cloakable ship
        if (null!=cloak)
        {
            // first off, if we're off the screen, turn off the cloak.
            if (!_v.isVisible())
            {
                cloak.amCloaked = false;
            }

            // if we're running away, and can cloak, cloak
            if (runningAway)
            {
                if (cloak.canCloak) cloak.amCloaked = true;
            }
            else
            {
                if (chutzpah<=0)
                {
                    // we'll never run away.
                    if (cloak.canCloak)
                    {
                        if (!cloak.amCloaked)
                        {
                            if (_v.isVisible())
                            {
                                // remember this is happening 60 times a second...
                                if (Random.Range(0, 1000) < 10)
                                {
                                    cloak.amCloaked = true;
                                }

                            }
                        }
                    }
                }
                else
                {
                    if (_v.isVisible())
                    {
                        if (!cloak.amCloaked)
                        {
                            if (CrappingOurselves())
                            {
                                if (Random.Range(0, 100) < cloakProbability)
                                {
                                    cloak.amCloaked = true;
                                }
                                else
                                {
                                    runningAway = true;
                                    runningAwayStartTime = Time.time;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // normal ship
            if (!runningAway)
            {
                // if we're not already running away, see if we want to
                if (_v.isVisible())
                {
                    if (CrappingOurselves())
                    {
                        runningAway = true;
                        runningAwayStartTime = Time.time;
                    }
                }
            }
        }

        if (runningAway)
        {
            behaviourModel = "Running Away";
            nearnessLatch = false;
            // just turn away from the player
            float _ang = AngleToDestination(new Vector2(0, 0));
            angleToEnemyDestination = 0;
            float _dir = Mathf.Sign(_ang);
            _turnAngle = _dir * turnDelta;
            float _desiredSpeed;
            if (Time.time-runningAwayStartTime>runningAwaySlowTime)
            {
                _desiredSpeed = maxSpeed;
            }
            else
            {
                _desiredSpeed = playerControl.maxSpeed * 0.8f;
            }

            

            if (MM.speed < _desiredSpeed) SpeedUp();
            if (MM.speed > _desiredSpeed) SlowDown();

            if (Vector2.Distance(transform.position, gameScript.player.transform.position) > safetyRange)
            {
                runningAway = false;
                // the captain gets emboldened each time he escapes
                chutzpah -= 1;
            }
        }
        else
        {
            
            float _ang = AngleToDestination(desiredRelativeLocation);
            angleToEnemyDestination = _ang;
            float _dir = Mathf.Sign(_ang);

            // if it's a small offset angle, pretend it's nothing.
            // it'll either get bigger, or it won't matter.
            //if (Mathf.Abs(_ang - MM.angle) < turnDelta) _dir = 0;
            float _dis = Vector2.Distance(transform.position, crosshair.transform.position);
            // choose a behaviour dependent on how far we are from our target destination

            if ((_dis < locationAccuracyInner) || (nearnessLatch))
            {
                behaviourModel += "Nearness Latch";
                // very near
                // once we reach this point, latch to this behaviour until we get a LOT further away from the player
                // (or too close - we're not actually kamikaze)
                float _playerDis = Vector2.Distance(transform.position, gameScript.player.transform.position);
                if ((_playerDis < tooClose))
                {
                    runningAway = true;
                    runningAwayStartTime = Time.time;
                }
                else
                {
                    nearnessLatch = true;
                }

                // match player speed, plus a little bit.
                if (MM.speed > playerMM.speed * 1.2f) SlowDown(); else SpeedUp();
                
                // turn towards the player to see if we can get a firing solution
                _ang = AngleToDestination(new Vector2(0, 0));
                _dir = Mathf.Sign(_ang);
                _turnAngle = -_dir * turnDelta;

            }
            else if (_dis < locationAccuracyOuter)
            {
                behaviourModel += "Getting Close";
                // getting close
                // as we approach, use linear interpolation to steadily reduce our speed
                float _desiredSpeedPerc = locationAccuracyOuter - locationAccuracyInner;
                _desiredSpeedPerc = (_dis - locationAccuracyInner) / _desiredSpeedPerc;
                // remember the player may be moving, so add their speed into the mix
                // they may be pointing in a different direction, but that adds to the fun
                float desiredSpeed = (maxSpeed * _desiredSpeedPerc) + (playerMM.speed*1.1f);
                // adjust to fit
                if (MM.speed > desiredSpeed) SlowDown();
                if (MM.speed < desiredSpeed) SpeedUp();
                // don't forget to keep turning towards the target
                _turnAngle = -_dir * turnDelta;
            }
            else
            {
                // miles off
                behaviourModel += "Miles off";
                breakOff = 0;
                if (Mathf.Abs(_ang) > 45)
                {
                    // our destination is behind us
                    // slow to increase our effective turning speed
                    SlowDown();
                }
                else
                {
                    // full whack
                    SpeedUp();
                }
                _turnAngle=-_dir * turnDelta;
            }
        }

        //runningAway = CrappingOurselves();

        MM.angle += _turnAngle;
        Vector2 _newPosition = Helper.MakeDistanceCoords(MM.speed, MM.angle, MM.angle);
        transform.position += new Vector3(-_newPosition.x,_newPosition.y,0);

        // can we fire?
        bool _canFire = true;
        // not if our gun hasn't cooled down
        if (Time.time < nextFire) _canFire = false;
        // not if we're not on screen
        if ((_canFire)&&(!_v.isVisible())) _canFire = false;
        // not if the player's been destroyed
        if ((_canFire)&&(gameScript.player.GetComponent<PlayerControl>().destroyed)) _canFire = false;

		if (_canFire)
        {
            // see if the enemy's in our sights!
            if (_t.HaveFiringSolution())
            {
                // FIRE!
                enemyShootSound.Play();
                CreateBullet();
                nextFire = Time.time + fireRate;
            }
        }
	}
}
