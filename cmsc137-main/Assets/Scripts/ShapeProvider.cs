using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class ShapeProvider : ScriptableObject
{
    [SerializeField] private List<ShapeAndProbability> _initialShapeAndProbabilities = new List<ShapeAndProbability>();

    private readonly List<ShapeAndProbability> _shapeAndProbabilities = new List<ShapeAndProbability>();
    private float _totalProbability;

    public bool Initialized => _shapeAndProbabilities.Count > 0;

    public void Init()
    {
        if (Initialized)
            return;

        _shapeAndProbabilities.AddRange(_initialShapeAndProbabilities.Select(s => new List<ShapeAndProbability>
        {
            s, GetRotatedShape(s, 90f), GetRotatedShape(s, 180f), GetRotatedShape(s, 270f)
        }).SelectMany(s => s));
        _totalProbability = _shapeAndProbabilities.Sum(s => s.probability);
    }

    private static ShapeAndProbability GetRotatedShape(ShapeAndProbability s, float angle)
    {
        return new ShapeAndProbability
        {
            shape = s.shape.Select(vec => GetRotatedVector(vec, angle).RoundToInt()).ToArray(),
            probability = s.probability
        };
    }
    public Vector2Int[] GetRandomShape()
    {
        if(!Initialized)
            Init();
        
        var probability = Random.Range(0, _totalProbability);
        var elapsedProbability = 0f;
        foreach (var shapeAndProbability in this._shapeAndProbabilities)
        {
            elapsedProbability += shapeAndProbability.probability;
            if (probability <= elapsedProbability)
                return shapeAndProbability.shape;
        }

        return new Vector2Int[0];
    }
    public static Vector2 GetRotatedVector(Vector2 vec, float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.forward) * vec;
    }


    [Serializable]
    public struct ShapeAndProbability
    {
        public Vector2Int[] shape;
        public float probability;
    }
}