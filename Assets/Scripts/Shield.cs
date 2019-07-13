using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shield : MonoBehaviour {

    public enum ShieldMode
    {
        off=0,
        auto,
        on
    }

    public ShieldMode mode;
    public bool shieldEnabled = false;
    public float shieldReducer;
    public float shieldRechargeDrag;

    private Energy energy;

    private TextMeshPro _guiText;

    private bool FindVisible()
    {
        // scan the enemies.  As soon as we find a visible one, we're done.
        GameObject[] _enemies = GameObject.FindGameObjectsWithTag("EnemyShip");
        Visibility _v;
        foreach (GameObject _enemy in _enemies)
        {
            _v = _enemy.GetComponent<Visibility>();
            if (_v.isVisible()) return true;
        }
        return false;
    }

    public void Start()
    {
        energy = GetComponent<Energy>();
        _guiText = GameObject.FindGameObjectWithTag("ShieldModeText").GetComponent<TextMeshPro>();
    }


    public void Update()
    {

        switch (mode)
        {
            case ShieldMode.off:
                {
                    _guiText.text = "OFF";
                    break;
                }

            case ShieldMode.auto:
                {
                    _guiText.text = "AUTO";
                    break;
                }

            case ShieldMode.on:
                {
                    _guiText.text = "ON";
                    break;
                }

            default:
                {
                    _guiText.text = "BROKEN";
                    break;
                }
        }
        if (energy.damaged) shieldEnabled = false;
        else
        {
            if (ShieldMode.off == mode) shieldEnabled = false;
            if (ShieldMode.on == mode) shieldEnabled = true;
            if (ShieldMode.auto == mode) shieldEnabled = FindVisible();
        }
        if (shieldEnabled)
        {
            // running the shields slows recharge
            energy.DoDrain(energy.rechargePerFrame * shieldRechargeDrag);
        }
    }

}
