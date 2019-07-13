using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visibility : MonoBehaviour {

    private Camera viewCamera=null;
    private Collider2D parentCollider;

    public void Start()
    {
        GameObject[] _o = GameObject.FindGameObjectsWithTag("MainCamera");
        if (_o.Length > 0) viewCamera = _o[0].GetComponent<Camera>();
        parentCollider = GetComponent<Collider2D>();
    }

    public bool isVisible()
    {
        if (null == viewCamera) return false;

        Vector3 _pos = viewCamera.WorldToViewportPoint(transform.position);
        
        if (_pos.x < 0) return false;
        if (_pos.x > 1) return false;
        if (_pos.y < 0) return false;
        if (_pos.y > 1) return false;
        return true;
    }

    public void Update()
    {
        // things that are off the screen don't collide with anything else;
        if (null!=parentCollider)
        {
            parentCollider.enabled = isVisible();
        }
    }
}
