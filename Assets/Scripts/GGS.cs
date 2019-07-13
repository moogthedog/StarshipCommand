using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GGS{
    public static int score;
    public static bool capsuleLaunched;
    public static bool capsuleSurvived;
    public static bool playerDestroyed;
    public static int newScore;
    public static int levelNumber=1;
    public static float SoundVolume = 1.0f;

    public const string SCENESUFFIX = "320";

    public static void NewGame()
    {
        score = 0;
        capsuleLaunched = false;
        capsuleSurvived = true;
        playerDestroyed = false;
        levelNumber = 1;
        newScore = 0;
    }

    public static void NewLevel()
    {
        capsuleLaunched = false;
        capsuleSurvived = true;
        playerDestroyed = false;
        newScore = 0;
        levelNumber++;
    }

    public static void AddScore(int parScore)
    {
        score += parScore;
        newScore += parScore;
    }
}
