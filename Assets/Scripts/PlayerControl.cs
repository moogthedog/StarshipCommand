using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour {

    // publics
    public float maxSpeed;
    public float maxTurn;
    public float forwardImpulse;
    public float rotationImpulse;
    public float turn;
    //public float speed;
    public float turnDampingBaseline;
    public float speedDampingBaseline;
    public float turnEnergyCost;
    public float impulseEnergyCost;
    public float fireRate;
    public Transform bulletSpawnPoint;
    public Transform capsuleLeftSpawn;
    public Transform capsuleRightSpawn;
    public GameObject playerBulletPrefab;
    public GameObject playerExplosion;
    public GameObject enemyExplosion;
    public GameObject capsule;
    public GameObject compass;
    public GameObject bulletDeath;
    public AudioSource bulletSound;
    public AudioSource hitSound;
    public AudioSource playerExplodeSound;
    public AudioSource driveSound;
    public AudioSource enemyExplodeSound;
    public AudioSource capsuleDootSound;


    public bool destroyed;
    public bool cheating = false;
    public float destructionTimer=-1;

    public float maxDriveVolDriveLevel;
    public float maxDriveVol;
    public float lowTonePitch;
    public float highTonePitch;
    public float capsuleLaunchTime;
    public float capsuleDootTime=0;


    public const bool showTargettingSolution = false;


    // debug

    public GameObject trailObject;
    public GameObject[] trail = new GameObject[Targetting.playerLeadTime];
    public GameObject[] shipRenderer;
    public GameObject currentShip;
       
       
    public GameObject stars;
 


    // privates
    public float turnDamping;
    public float speedDamping;
    public float nextFire = 0;
    public float SPEED_MULTIPLIER;

    private Shield shield;
    private Energy energy;
    private SpriteRenderer spriteRenderer;
    private Game gameScript;
    private GameObject xBar;
    private GameObject yBar;

    public ManagedMob mm;

    // Use this for initialization
    void Start () {
        //trail = new GameObject[maxTrail];
        Vector3 _pos = transform.position;
        GameObject _s=Instantiate(stars, _pos, transform.rotation);
        _s.GetComponent<FollowPlayerJumping>().player = gameObject;
        _pos.y += 9;
        _s=Instantiate(stars, _pos, transform.rotation);
        _s.GetComponent<FollowPlayerJumping>().player = gameObject;
        energy = GetComponent<Energy>();
        shield=GetComponent<Shield>();
       
        mm = GetComponent<ManagedMob>();
        for (int _count=0; _count<Targetting.playerLeadTime; _count++)
        {
            if (showTargettingSolution) trail[_count] = Instantiate(trailObject, transform.position, transform.rotation);
        }
        gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        currentShip=Instantiate(shipRenderer[(GGS.levelNumber-1) % 8],transform);
        spriteRenderer = currentShip.GetComponent<SpriteRenderer>();
        xBar = GameObject.Find("XBarParent");
        yBar = GameObject.Find("YBar");
    }

    public void DestroyPlayer()
    {
        // we can't actually destroy the player...
        playerExplodeSound.Play();
        spriteRenderer.enabled = false;
        destroyed = true;
        // stop the scanners
        energy.damageRepair = Time.time + 1000;
        energy.damaged = true;
        // stop recharging;
        energy.rechargePerFrame = 0;
    }

    public void DoDriveSound()
    {
        // we want the full drive to be responsible for about 3/4 of the tone, and the rotation motors to be the other 1/4
        float _mainDrivePerc = mm.speed / maxSpeed;
        float _turnDrivePerc = Mathf.Abs(turn) / maxTurn;
        float _driveTone = (_mainDrivePerc * 0.75f) + (_turnDrivePerc * 0.25f);

        if ((_driveTone==0) || (destroyed))
        {
            if (driveSound.isPlaying)
            {
                driveSound.Stop();
            }
        }
        else
        {
            float _drivePitch = lowTonePitch + (_driveTone * (highTonePitch - lowTonePitch));
            float _driveVol = Mathf.Clamp(_driveTone / maxDriveVolDriveLevel, 0, 1) * maxDriveVol;
            driveSound.pitch = _drivePitch;
            driveSound.volume = _driveVol;
            if (!driveSound.isPlaying)
            {
                driveSound.Play();
            }
        }
    }

    public void DoCapsuleDoots()
    {
        if (Time.time>capsuleDootTime)
        {
            capsuleDootSound.pitch -= 0.005f;
            capsuleDootSound.volume -= 0.02f;
            capsuleDootSound.Play();
            capsuleDootTime = Time.time + 0.15f;
        }
    }
       

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (destroyed) return;

        GameObject _other = collision.gameObject;
        string _otherTag = _other.tag;

        if (_otherTag == "EnemyShip")
        {
            enemyExplodeSound.Play();
            // add the score manually, as the energy code only does it on 'natural' object destruction;
            GGS.AddScore(2);
            // ship collision with shields off is a bad thing.
            energy.DoDamage(150,0,true);
            GameObject _explo=Instantiate(enemyExplosion, _other.transform.position, _other.transform.rotation);
            Rigidbody2D _explorb = _explo.GetComponent<Rigidbody2D>();
            ManagedMob _enemyManager = _other.GetComponent<ManagedMob>();
            _explorb.velocity = _enemyManager.velocity();

            Destroy(_other);
        }

        if (_otherTag=="EnemyBullet")
        {
            hitSound.Play();
            energy.DoDamage(_other.GetComponent<bullet>().payloadEnergy,0,true);
            Instantiate(bulletDeath, _other.transform.position, _other.transform.rotation);
            Destroy(_other);
        }
    }
    // Update is called once per frame
    void Update () {

        // cheat during development
        if ((!GGS.capsuleLaunched) && (cheating))
        {
            energy.energy = Mathf.Clamp(energy.energy, 100, 400);
        }

        DoDriveSound();

        compass.transform.position = transform.position;

        bool _turning = false;
        bool _accelerating = false;
        float speed = mm.speed;

        for (int _count=0; _count<Targetting.playerLeadTime; _count++)
        {
            if (showTargettingSolution) trail[_count].transform.position = new Vector2(Targetting.playerLead[_count].x, Targetting.playerLead[_count].y);
        }

        if ((!destroyed) && (!GGS.capsuleLaunched))
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                cheating = !cheating;
                GameObject.FindGameObjectWithTag("CheatingText").GetComponent<TextMeshPro>().enabled = cheating;
            }

            if (Input.GetKey(KeyCode.M))
            {
                if (speed < maxSpeed) energy.DoDrain(impulseEnergyCost);
                speed += forwardImpulse;
                _accelerating = true;
            }

            if (Input.GetKey(KeyCode.Comma))
            {
                if (speed > 0) energy.DoDrain(impulseEnergyCost);
                speed -= forwardImpulse;
            }

            if (Input.GetKey(KeyCode.X))
            {
                if (turn > -maxTurn) energy.DoDrain(turnEnergyCost);
                turn -= rotationImpulse;
                _turning = true;
            }

            if (Input.GetKey(KeyCode.Z))
            {
                if (turn < maxTurn) energy.DoDrain(turnEnergyCost);
                turn += rotationImpulse;
                _turning = true;
            }

            if (Input.GetKey(KeyCode.N))
            {
                if (nextFire < Time.time)
                {
                    bulletSound.Play();
                    GameObject _newBullet = Instantiate(playerBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                    bullet _b = _newBullet.GetComponent<bullet>();
                    _b.parent = gameObject;
                    _b.SetSpeed(transform.rotation.eulerAngles.z, speed);
                    nextFire = Time.time + fireRate;
                    energy.DoDrain(_b.fireEnergy);
                }
            }

            if (!GGS.capsuleLaunched)
            {
                if (Input.GetKey(KeyCode.F))
                {
                    GGS.capsuleLaunched = true;
                    GameObject _capsule = Instantiate(capsule, capsuleLeftSpawn.position, capsuleLeftSpawn.rotation);
                    bullet _b = _capsule.GetComponent<bullet>();
                    _b.parent = gameObject;
                }
                else if (Input.GetKey(KeyCode.G))
                {
                    GGS.capsuleLaunched = true;
                    GameObject _capsule = Instantiate(capsule, capsuleRightSpawn.position, capsuleRightSpawn.rotation);
                    bullet _b = _capsule.GetComponent<bullet>();
                    _b.parent = gameObject;
                }

                if (GGS.capsuleLaunched)
                {
                    capsuleDootSound.pitch = 1;
                    capsuleDootSound.volume = 1;
                    GGS.capsuleSurvived = true;
                    destructionTimer = Time.time + 5;
                    GameObject.FindGameObjectWithTag("EscapeCapsuleText").GetComponent<TextMeshPro>().enabled = true;
                }
            }




            if (Input.GetKeyDown(KeyCode.C))
            {
                shield.mode = Shield.ShieldMode.off;
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                shield.mode = Shield.ShieldMode.auto;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                shield.mode = Shield.ShieldMode.on;
            }

            if (Input.GetKeyDown(KeyCode.Period))
            {
                Instantiate(playerExplosion, gameObject.transform.position, gameObject.transform.rotation);
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (turnDamping == 0)
                {
                    turnDamping = turnDampingBaseline;
                }
                else
                {
                    turnDamping = 0;
                }
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (speedDamping == 0)
                {
                    speedDamping = speedDampingBaseline;
                }
                else
                {
                    speedDamping = 0;
                }
            }
        }

        if (!destroyed)
        { 
            speed = Mathf.Clamp(speed, 0, maxSpeed);
            // speed is always positive (or zero)
            if ((speedDamping > 0) && (!_accelerating))
            {
                if (speed > speedDamping)
                {
                    energy.DoDrain(impulseEnergyCost);
                    speed -= speedDamping;
                }
                else
                {
                    speed = 0;
                }
            }
            turn = Mathf.Clamp(turn, -maxTurn, maxTurn);

            if ((turnDamping > 0) && (!_turning))
            {
                // turn is positive or negative, so needs a bit more work.
                if (Mathf.Abs(turn) > turnDamping)
                {
                    energy.DoDrain(turnEnergyCost);
                    turn -= turnDamping * Mathf.Sign(turn);
                }
                else
                {
                    turn = 0;
                }
            }
            if ((GGS.capsuleLaunched) && (GGS.capsuleSurvived))
            {

                DoCapsuleDoots();

            }
        }
        else
        {

        }

        if (destructionTimer > 0)
        {
            if (Time.time > destructionTimer)
            {
                // KERBLAMMO
                energy.DoDrain(2000);
            }
        }

        mm.speed = speed;
        mm.angle += turn;

        float _xbp = (-1 / maxTurn) * turn;
        float _ybp = (1 / maxSpeed) * mm.speed;
        xBar.transform.localScale = new Vector3(_xbp, 1, 1);
        yBar.transform.localScale = new Vector3(1, _ybp, 1);

        Vector2 _newPosition = Helper.MakeDistanceCoords(speed, mm.angle,mm.angle);
        
        transform.position += new Vector3(-_newPosition.x, _newPosition.y, 0);

	}
}
