using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloak : MonoBehaviour {

    public float cloakBattery;
    public bool canCloak;
    public bool amCloaked;
    public float rechargeRate;
    public float expenditure;

    private EnemyController ec;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        ec = GetComponent<EnemyController>();
        spriteRenderer = ec.spriteRendererObject.GetComponent<SpriteRenderer>(); ;
    }

    // Use this for initialization
    void Start () {
        cloakBattery = 1;
        canCloak = true;
        amCloaked = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (amCloaked)
        {
            cloakBattery -= expenditure;
        }
        else
        {
            cloakBattery += rechargeRate;
        }

        if (cloakBattery <= 0) canCloak = false;
        if (!canCloak) amCloaked = false;
        if (cloakBattery >= 1) canCloak = true;

        cloakBattery = Mathf.Clamp(cloakBattery, 0, 1);

        spriteRenderer.enabled = !amCloaked;
    }
}
