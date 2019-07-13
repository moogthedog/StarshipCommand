using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Game : MonoBehaviour {

    public int minEnemies;
    public GameObject[] possibleEnemies;
    public float[] enemyDistribution;
    public float spawnDistance;
    public GameObject player;
    public TextMeshPro scoreText;

    private bool gameEnded = false;
    private float gameEndTime;

    private PlayerControl playerControl;

    public void PlayerDestroyed()
    {
        // cue the end-level scripts.
        // but we can't actually destroy the player object - too much rests on it.
        // only do it once...
        if (!playerControl.destroyed)
        {
            GGS.playerDestroyed = true;
            playerControl.DestroyPlayer();
            Instantiate(playerControl.playerExplosion, player.transform.position, player.transform.rotation);
            gameEnded = true;
            gameEndTime = Time.time + 8;
        }
    }
   
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("PlayerShip");
        playerControl = player.GetComponent<PlayerControl>();
        GameObject.Find("CommandNo").GetComponent<TextMeshPro>().text = GGS.levelNumber.ToString();
        // an extra simultaneous enemy evey 2 levels
        // 1-3, 2-4, 3-4, 4-5, 6-5, 7-6, 8-6
        minEnemies = 3 + (Mathf.FloorToInt(GGS.levelNumber/2));
        // at higher levels, you get more big ships.
        // 1-100%, 2-90%, 3-80%, 4 and on-75%
        enemyDistribution[0] = Mathf.Clamp(100 - ((GGS.levelNumber - 1) * 10), 75, 100);
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            GGS.SoundVolume = 1.0f;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            GGS.SoundVolume = 0.0f;
        }

        AudioListener.volume = GGS.SoundVolume;

        // Enumerate the enemy armada
        GameObject[] _enemyList = GameObject.FindGameObjectsWithTag("EnemyShip");
        int _enemyCount = _enemyList.Length;

        // bring the enemy armada up to strength
        for (int _j = _enemyCount; _j < minEnemies;_j++)
        {
            Vector2 _newPos = Helper.MakeDistanceCoords(spawnDistance, 0, 359);
            _newPos.x += player.transform.position.x;
            _newPos.y += player.transform.position.y;
            GameObject _newEnemyType = Helper.DistributionChooser(possibleEnemies, enemyDistribution);
            GameObject _newEnemy = Instantiate(_newEnemyType, new Vector3(_newPos.x, _newPos.y, 0), Quaternion.Euler(0, 0, 0));
            _newEnemy.GetComponent<EnemyController>().bigBulletChance = Mathf.Clamp((GGS.levelNumber * 2) + 1, 0, 15); ;
        }
        scoreText.text = GGS.score.ToString();
        if ((gameEnded) && (Time.time > gameEndTime))
        {
            SceneManager.LoadScene("PostGame"+GGS.SCENESUFFIX);
        }
	}
   
}
