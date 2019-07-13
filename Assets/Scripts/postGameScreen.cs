using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class postGameScreen : MonoBehaviour {

    public bool newGame;
    public string sceneToLoad = "TitlePage";

    // debugging
    public int score;
    public int newScore;
    public bool capsuleSurvived;
    public bool capsuleLaunched;
    public int levelNumber;

    public GameObject[] shipGraphics;

    

    private TextMeshPro titleText;
    private TextMeshPro flavourText;
    private TextMeshPro scoreText;
    private TextMeshPro commandResponse;
    private TextMeshPro returnText;

    private int levelThreshold;

    private string[] BadTexts = { "Furious", "Unhappy", "Displeased", "Disappointed" };
    private string[] GoodTexts = { "Satisfied", "Happy", "Pleased", "Delighted" };

    public int LevelThreshold(int levelNum)
    {
        return (levelNum-1) * 10 + 100;
    }

    public bool LevelPassed()
    {
        float _threshold = LevelThreshold(GGS.levelNumber);

        // If we've passed the threshold, we've passed the level.
        if (GGS.newScore >= _threshold) return true;

        // Use the first quarter of a sine curve to scale the rest
        float _mult = Mathf.PI / 2 / _threshold;
        float _rad = GGS.newScore * _mult;
        float _perc = Mathf.Sin(_rad) * 100;
        return Random.Range(0, 100) < _perc;
    }

    public void GenerateText()
    {
        string connectorString;
        int commandFlavour;
        string commandFlavourText;
        bool playerSurvived = true;

        titleText.text = "STARSHIP COMMAND " + GGS.levelNumber.ToString();

        // game end status
        if (GGS.capsuleLaunched)
        {
            if (GGS.capsuleSurvived)
            {
                flavourText.text = "An escape capsule was launched and returned safely from the combat zone";
            }
            else
            {
                flavourText.text = "An escape capsule was launched, but collided with an enemy starship";
                playerSurvived = false;
            }
        }
        else
        {
            flavourText.text = "NO escape capsule was launched before the starship exploded";
            playerSurvived = false;
        }

        scoreText.text = "Your official combat experience rating is now recorded as ";
        if (GGS.levelNumber == 1)
        {
            scoreText.text += GGS.newScore.ToString();
        }
        else
        {
            scoreText.text += GGS.score.ToString() + "\n having just gained " + GGS.newScore.ToString();
        }

        if (playerSurvived)
        {
            // 100 points baseline, plus 10 for each level after the first
            levelThreshold = LevelThreshold(GGS.levelNumber);
            if (GGS.newScore < levelThreshold)
            {
                // Use a montecarlo method to decide if the failing player gets another go
                if (LevelPassed())
                {
                    connectorString = "but";
                    newGame = false;
                }
                else
                {
                    connectorString = "and";
                    newGame = true;
                }
                // because we know 0<=GGS.newScore<levelThreshold, this must return 0-3
                commandFlavour = Mathf.FloorToInt(GGS.newScore / (levelThreshold / 4));
                commandFlavourText = BadTexts[commandFlavour];
            }
            else
            {
                connectorString = "and";
                newGame = false;
                commandFlavour = (int)Mathf.Clamp(Mathf.Floor((GGS.newScore - levelThreshold) / (levelThreshold / 4)), 0, 3);
                commandFlavourText = GoodTexts[commandFlavour];
            }

            commandResponse.text = "After your performance on this command the Star-Fleet authorities are said to be '" + commandFlavourText + "' " + connectorString + " ";
            if (newGame)
            {
                commandResponse.text += "they retire you from active service.";
            }
            else
            {
                commandResponse.text += "they allow you the commmand of another starship.";
            }
            commandResponse.enabled = true;
        }
        else
        {
            commandResponse.enabled = false;
            newGame = true;
        }

        if (newGame)
        {
            if (playerSurvived)
            {
                returnText.text = "Press <RETURN> to reapply";
            }
            else
            {
                returnText.text = "Press <RETURN> to avenge them";
            }
        }
        else
        {
            returnText.text = "Press <RETURN> to start the next command";
        }
    }

    public void Start()
    {
        titleText = GameObject.Find("TitleText").GetComponent<TextMeshPro>();
        flavourText = GameObject.Find("FlavourText").GetComponent<TextMeshPro>();
        scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshPro>();
        commandResponse = GameObject.Find("CommandResponse").GetComponent<TextMeshPro>();
        returnText = GameObject.Find("ReturnText").GetComponent<TextMeshPro>();

        capsuleLaunched = GGS.capsuleLaunched;
        capsuleSurvived = GGS.capsuleSurvived;
        newScore = GGS.newScore;
        score = GGS.score;
        levelNumber = GGS.levelNumber;

        for (float _pos=-9; _pos<=9; _pos=_pos+2)
        {
            Instantiate(shipGraphics[(GGS.levelNumber - 1) % 8], new Vector3(_pos, 5.5f, 1), Quaternion.Euler(new Vector3(0, 0, 0)));
        }

        GenerateText();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (newGame)
            {
                sceneToLoad = "TitlePage"+GGS.SCENESUFFIX;
            }
            else
            {
                sceneToLoad = "SampleScene"+GGS.SCENESUFFIX;
                GGS.NewLevel();
            }
            SceneManager.LoadScene(sceneToLoad);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            GGS.newScore = LevelThreshold(GGS.levelNumber);
            GGS.capsuleLaunched = true;
            GGS.capsuleSurvived = true;
            GenerateText();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            DoDebug();
        }
    }

    private void DoDebug()
    {
        GGS.score = score;
        GGS.capsuleLaunched = capsuleLaunched;
        GGS.capsuleSurvived = capsuleSurvived;
        GGS.newScore = newScore;
        GGS.levelNumber = levelNumber;
        GenerateText();
    }





}
