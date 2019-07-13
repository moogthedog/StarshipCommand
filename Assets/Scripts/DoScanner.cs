using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoScanner : MonoBehaviour {

    private Texture2D t;
    public float scannerRange;
    public int scannerWidth;
    public int scannerHeight;
    public bool bigblobs;
    public GameObject player;
    private SpriteRenderer _sr;
    public Color32 backColor;

    private Energy _e;
    private Shield _s;
    private TextMeshPro _guiText;
    

	// Use this for initialization
	void Start () {
        t = new Texture2D(scannerWidth,scannerHeight);
        t.filterMode = FilterMode.Point;
        player = GameObject.FindGameObjectWithTag("PlayerShip");
        _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = Sprite.Create(t, new Rect(0, 0, scannerWidth, scannerHeight), new Vector2(0.5f,0.5f), 16);
        _s = player.GetComponent<Shield>();
        _e = player.GetComponent<Energy>();
        GameObject[] _shieldText = GameObject.FindGameObjectsWithTag("ShieldText");
        _guiText = _shieldText[0].GetComponent<TextMeshPro>();
        _guiText.enabled = false;
    }

    private void Update()
    {
        
        // Shields?
        if (_s.shieldEnabled)
        {
            if (bigblobs)
            {
                // SR scanner - just clear;
                RenderClear();
            }
            else
            {
                // LR Scanner - clear and display text;
                RenderClear();
                _guiText.enabled = true;
            }
        }
        else
        {
            // no shields=no text.  Damage?
            _guiText.enabled = false;
            if (_e.damaged)
            {
                // Damaged.
                if (bigblobs)
                {
                    // SR Scanner - just clear;
                    RenderClear();
                }
                else
                {
                    // LR Scanner - static;
                    RenderStatic();
                }
            }
            else
            {
                // No shields, no damage.
                RenderScanner();
            }
        }

    }

    public void RenderScanner()
    {
        GameObject[] _e = GameObject.FindGameObjectsWithTag("EnemyShip");

        // Clear
        Color32[] resetColorArray = t.GetPixels32();
        for (int _i = 0; _i < resetColorArray.Length; _i++)
        {
            resetColorArray[_i] = backColor;
        }
        t.SetPixels32(resetColorArray);

        // plot the player on the LR scanner.
        if (!bigblobs) t.SetPixel(scannerWidth / 2, scannerHeight / 2, Color.white);

        foreach (GameObject _o in _e)
        {
            Vector2 _ppos = new Vector2(player.transform.position.x, player.transform.position.y);
            Vector2 _opos = new Vector2(_o.transform.position.x, _o.transform.position.y);
            Vector2 _diff = _opos - _ppos;
            // find the angle 
            float _theta = Vector2.SignedAngle(new Vector2(0,1),_diff)-player.transform.rotation.eulerAngles.z;
            float _r = Vector2.Distance(_ppos, _opos);

            // scale distance to scanner size
            _r /= scannerRange;

            // coords relative to the player position and rotation...
            float _x = -Mathf.Sin(_theta*Mathf.Deg2Rad)*_r;
            float _y = Mathf.Cos(_theta*Mathf.Deg2Rad)*_r;

            // eliminate anything that wouldn't be displayed
            if (_x < -1) continue;
            if (_x > 1) continue;
            if (_y < -1) continue;
            if (_y > 1) continue;

            // scale up for the display
            _x *= scannerWidth / 2;
            _y *= scannerHeight / 2;

            // and then move the player to the centre of the scanner
            _x += scannerWidth / 2;
            _y += scannerHeight / 2;

            // and floor it for pixels
            int _ix = (int)Mathf.Floor(_x);
            int _iy = (int)Mathf.Floor(_y);



            Color32 _pixel = new Color32(255, 255, 255, 255);
            // and plot a pixel (or a blob)
            t.SetPixel(_ix, _iy, Color.white);
            if (bigblobs)
            {
                t.SetPixel(_ix + 1, _iy, Color.white);
                t.SetPixel(_ix, _iy+1, Color.white);
                t.SetPixel(_ix + 1, _iy+1, Color.white);
            }
            
        }
        t.Apply();
    }

    void RenderStatic()
    {
        Color32[] resetColorArray = t.GetPixels32();
        Color32 _white = new Color32(255, 255, 255, 255);
        for (int _i = 0; _i < resetColorArray.Length; _i++)
        {
            if (Random.Range(0, 100) < 50)
            {
                resetColorArray[_i] = _white;
            }
            else
            {
                resetColorArray[_i] = backColor;
            }
        }
        t.SetPixels32(resetColorArray);
        t.Apply();
    }

    void RenderClear()
    {
        Color32[] resetColorArray = t.GetPixels32();
        Color32 _white = new Color32(255, 255, 255, 255);
        for (int _i = 0; _i < resetColorArray.Length; _i++)
        {
           resetColorArray[_i] = backColor;
        }
        t.SetPixels32(resetColorArray);
        t.Apply();
    }
}
