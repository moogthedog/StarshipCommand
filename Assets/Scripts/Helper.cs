using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour {

    public static float frameRate=60;

    public static Vector2 rotateVec2(Vector2 source, float angleDegrees)
    {
        float _mag = source.magnitude;
        // the relative vector doesn't actually matter - it's just a reference...
        float _ang = Vector2.SignedAngle(new Vector2(0, 1), source)+angleDegrees;
        Vector2 retval= MakeDistanceCoords(source.magnitude, _ang, _ang);
        return new Vector2(-retval.x, retval.y);
    }

    public static T DistributionChooser<T>(T[] sourceArray, float[] distributionArray)
    {
        if (sourceArray.Length != distributionArray.Length) return default(T);
        int _len = distributionArray.Length;
        float _r = Random.Range(0, 100);
        
        // process the array in order
        for (int _count=0; _count<_len; _count++)
        {
            if (_r < distributionArray[_count]) return sourceArray[_count];
        }
        return sourceArray[_len - 1];
    }

    public static Vector2 MakeDistanceCoords(float distance, float minAngle, float maxAngle)
    {
        // pick a random angle in the range specified
        float _targetAngle = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;
        // work out the coordinates at the distance specified
        float _x = Mathf.Sin(_targetAngle) * distance;
        float _y = Mathf.Cos(_targetAngle) * distance;

        // and spit'em out.
        return new Vector2(_x, _y);
    }

    /// <summary>
    /// Clamp a Vector2 to a magnitude range, keeping the angle
    /// </summary>
    /// <param name="source">The Vec2 to adjust</param>
    /// <param name="minMagnitude">The minimum magnitude allowed</param>
    /// <param name="maxMagnitude">The maximum magnitude allowed</param>
    /// <returns>The clamped Vec2</returns>
    public static Vector2 VectorClamp(Vector2 source, float minMagnitude, float maxMagnitude)
    {
        float _mag = source.magnitude;
        if ((_mag >= minMagnitude) && (_mag <= maxMagnitude)) return source;
        float _ang = Vector2.SignedAngle(new Vector2(0, 1), source);
        if (_mag>maxMagnitude) return MakeDistanceCoords(maxMagnitude, _ang, _ang);
        return MakeDistanceCoords(minMagnitude, _ang, _ang);
    }

    public static Vector2 RBVelocity(float angle, float frameSpeed)
    {
        Vector2 _newVelocity = MakeDistanceCoords(frameSpeed * frameRate, angle, angle);
        return new Vector2(-_newVelocity.x, _newVelocity.y);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
