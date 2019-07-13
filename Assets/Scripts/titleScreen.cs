using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class titleScreen : MonoBehaviour {

    public GameObject pixel;

    GameObject[][] oval;
    public int ovals;
    public int pointsPerOval; // must be even
    public float size;
    public int processed;
    public float processedAngle;
    
    
    
	// Use this for initialization
	void Start () {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        oval = new GameObject[ovals][];
	    for (int _count=0; _count<ovals; _count++)
        {
            GameObject[] _newOval = new GameObject[pointsPerOval];
            oval[_count] = _newOval;
            float _minorRadius = (float)size / ((float)ovals) * (_count+1);
            float _anglePerPixel = 360 / (float)pointsPerOval;
            float _offset = _anglePerPixel / 2;

            for (int _count2 = 0; _count2 < pointsPerOval; _count2++)
            {
                GameObject _newPixel = Instantiate(pixel, transform.position, transform.rotation);
                float _x = Mathf.Sin(((float)_count2 * _anglePerPixel+_offset)*Mathf.Deg2Rad) * _minorRadius+transform.position.x;
                float _y = Mathf.Cos(((float)_count2 * _anglePerPixel+_offset)*Mathf.Deg2Rad) * size+transform.position.y;
                _newPixel.transform.position = new Vector3(_x, _y, 0);
                _newOval[_count2] = _newPixel;
            }
        }
        processed = 0;
	}
	
	// Update is called once per frame
	void Update () {
		for (int _count=0; _count<ovals; _count++)
        {
            // each frame, we pick two pixels from each oval...
            GameObject _thisPixel = oval[_count][processed];

            _thisPixel.transform.position = Helper.rotateVec2(_thisPixel.transform.position-transform.position, processedAngle)+(Vector2)transform.position;
            _thisPixel = oval[_count][(pointsPerOval - processed)-1];
            _thisPixel.transform.position = Helper.rotateVec2(_thisPixel.transform.position-transform.position, processedAngle)+(Vector2)transform.position;
        }
        processed++;
        if (processed >= (pointsPerOval / 2)) processed = 0;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GGS.NewGame();
            Application.targetFrameRate = 60;
            SceneManager.LoadScene("SampleScene"+GGS.SCENESUFFIX);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}
}
