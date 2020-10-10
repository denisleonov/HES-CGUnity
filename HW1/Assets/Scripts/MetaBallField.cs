using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MetaBallField
{

    public Transform[] Balls = new Transform[0];
    public float BallRadius = 1;

    private Vector3[] _ballPositions;
    
    /// <summary>
    /// Call Field.Update to react to ball position and parameters in run-time.
    /// </summary>
    public void Update()
    {
        _ballPositions = Balls.Select(x => x.position).ToArray();
    }
    
    /// <summary>
    /// Calculate scalar field value at point
    /// </summary>
    public float F(Vector3 position)
    {
        float f = 0;
        // Naive implementation, just runs for all balls regardless the distance.
        // A better option would be to construct a sparse grid specifically around 
        foreach (var center in _ballPositions)
        {
                Vector3 c = center + new Vector3(
                        Mathf.Sin(Time.time + center.z),
                        Mathf.Sin(Time.time + center.y),
                        Mathf.Sin(Time.time + center.x)
                );
           
            f += 1 / Vector3.SqrMagnitude(c - position);
        }

        f *= BallRadius * BallRadius;

        return f - 1;
    }
}