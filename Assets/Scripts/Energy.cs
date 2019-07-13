using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// The class for dealing with energy and ship damage

public class Energy : MonoBehaviour {

    public float initialEnergy;
    public float maxEnergy;
    public float rechargePerFrame;
    public float energy;
    public float damageChance;
    public bool damaged = false;
    public float damageRepair=0;
    public GameObject explosion;
    private AudioSource enemyExplosionSound;
    public TextMeshPro energyText;
    public AudioSource alarmBeepSound;
    float nextAlarm=0;

    public bool isPlayer;
    public GameObject[] energyBars;

    public Game gameController;
    
    private Shield shield;

    private float displayEnergy = 0;
    private ManagedMob parentMM;
    private Game gameScript;

    // Use this for initialization
    void Start () {
        energy = initialEnergy;
        shield = GetComponent<Shield>();
        parentMM = GetComponent<ManagedMob>();
        gameScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        enemyExplosionSound = GameObject.Find("EnemyExplodeSound").GetComponent<AudioSource>();
	}

    // lose energy without reference to anything else
    // used by the shields and engines
    public void DoDrain(float drain)
    {
        energy -= drain;
    }

    public void DoDamage(float damage, int score, bool critical = false)
    {
        // Some damage will *really* hurt us, regardless of shielding
        // (not incremental, so if we're already damaged, we stop checking)
        // (Do this first - if the damage wounds us, we want the effect to start NOW)
        if (critical)
        {
            if ((Random.Range(0, 100) < damageChance) && (!damaged))
            {
                // set us as damaged, and find out how long Scotty's going to take to fix it.
                damaged = true;
                damageRepair = Time.time + Random.Range(5, 10);
            }
        }

        // if we've got a shield...
        if (shield!=null)
        {
            //... and it's switched on, and we're not hurt
            if ((shield.shieldEnabled) && (!damaged))
            {
                // ameliorate some of the damage;
                damage *= shield.shieldReducer;
            }

        }
        energy -= damage;
        if (energy <= 0) GGS.AddScore(score);
    }

    private void DisplayEnergy()
    {
    
        // use a basic easing algorithm to smooth the energy display a bit.
        displayEnergy = displayEnergy - ((displayEnergy - energy) * 0.5f);

        // How much is a bar worth?
        float _barEnergy = maxEnergy / 4;
        // How far through a bar are we?
        float _barValue = displayEnergy % _barEnergy;
        // which bar is affected?
        float _bar = displayEnergy / _barEnergy;

        for (int _count=3; _count>=0; _count--)
        {
            if ((int)_bar>_count)
            {
                energyBars[_count].transform.localScale = new Vector3(1, 1, 1);
                continue;
            }
            if (Mathf.Floor(_bar)<_count)
            {
                energyBars[_count].transform.localScale = new Vector3(0, 1, 1);
                continue;
            }
            float _perc = _barValue / _barEnergy;
            energyBars[_count].transform.localScale = new Vector3(_perc, 1, 1);

        }
    }
	
	// Update is called once per frame
	void Update () {

        if (energy<=0)
        {
            if (!isPlayer)
            {
                enemyExplosionSound.Play();
                // Explosion appears at our location
                GameObject _explo=Instantiate(explosion, transform.position, transform.rotation);
                // if we have an RB, propogate our velocity to the explosion too
                if (null != parentMM)
                {
                    Rigidbody2D _explorb = _explo.GetComponent<Rigidbody2D>();
                    _explorb.velocity = parentMM.velocity();
                }
                Destroy(gameObject);
            }
            else
            {
                gameController.PlayerDestroyed();
            }
        }

        if ((isPlayer) && (!GGS.playerDestroyed))
        {
            if (energy<maxEnergy/4)
            {
                if (Time.time>nextAlarm)
                {
                    energyText.enabled = !energyText.enabled;
                    alarmBeepSound.Play();
                    nextAlarm = Time.time + Mathf.Lerp(0.25f, 1, (energy / (maxEnergy / 4)));
                }
            }
            else
            {
                nextAlarm = 0;
                energyText.enabled = true;
            }
        }

        if (damageRepair<Time.time)
        {
            damaged = false;
        }

        // do recharge last, or we'd never hit zero;
        energy += rechargePerFrame;
        energy = Mathf.Clamp(energy, 0, maxEnergy);

        if (isPlayer) DisplayEnergy();
	}
}
