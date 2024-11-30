// /*
// Created by Darsan
// */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxGrid
{
    public float TileSize { get; }
    public float Spacing { get; set; }


    public BoxGrid(float tileSize, float spacing)
    {
        TileSize = tileSize;
        Spacing = spacing;
    }


    public Vector2Int InverseCoordinate(Vector2Int baseCoordinate, Vector2Int relCoordinate)
    {
        return relCoordinate * -1;
    }


    public static Vector2Int RelativeCoordinate(Vector2Int baseCoordinate, Vector2Int relCoordinate,
        Vector2Int newBaseCoordinate)
    {
        return baseCoordinate + relCoordinate - newBaseCoordinate;
    }

    public Vector2 GetRelativePositionForCoordinate(Vector2Int coordinate)
    {

        return new Vector2(coordinate.x*(TileSize+Spacing), coordinate.y * (TileSize + Spacing));
    }


    public Vector2 GetRelativePositionForCoordinate(Vector2 coordinate)
    {
       
        return new Vector2(coordinate.x * (TileSize + Spacing), coordinate.y * (TileSize + Spacing));
    }


    public Vector2Int GetNearestCoordinateForRelativePosition(Vector2 pos)
    {
       ;
        return new Vector2Int(Mathf.RoundToInt(pos.x/(TileSize+Spacing)), Mathf.RoundToInt(pos.y/(TileSize+Spacing)));
    }



    public static IEnumerable<Vector2Int> GetAdjacentCoordinates(IEnumerable<Vector2Int> coordinates)
    {
        var list = coordinates.ToList();
        return list.SelectMany(i => GetAdjacentCoordinates(i).Except(list)).Distinct();
    }

    public static IEnumerable<Vector2Int> GetAdjacentCoordinates(Vector2Int vec)
    {
        return new[]
        {
            vec + new Vector2Int(0, 1), vec + new Vector2Int(1, 0), vec + new Vector2Int(0, -1),
            vec + new Vector2Int(-1, 0)
        };
    }





    

    public static Vector2 GetCenterOfGravityCoordinate(IEnumerable<Vector2Int> coordinates)
    {
        var list = coordinates.ToList();
        return list.Aggregate(Vector2.zero, (total, vec) => total + vec) / list.Count;
    }



  
}