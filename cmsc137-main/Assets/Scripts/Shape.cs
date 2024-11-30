// /*
// Created by Darsan
// */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public partial class Shape : MonoBehaviour, IEnumerable<Piece>, IInitializable<Shape.InitParams>
{


    [SerializeField] private Piece _piecePrefab;

    private readonly List<Piece> _pieces = new List<Piece>();
    private bool _isFront;

    public BoxGrid BoxGrid { get; private set; }
    public Vector2Int Coordinate { get; set; }

    public int Color { get; set; }

    public float Opacity
    {
        set
        {
            _pieces.ForEach(p=>p.ColorValue = p.ColorValue.WithAlpha(0.5f));
        }
    }

    public bool IsFront
    {
        get => _isFront;
        set
        {
            _pieces.ForEach(piece => piece.IsFront = value);
            _isFront = value;
        }
    }

    public float PieceScale
    {
        get => _pieceScale;
        set
        {
            _pieces.ForEach(p=>p.Scale = value);
            _pieceScale = value;
        }
    }

    public IEnumerator<Piece> GetEnumerator()
    {
        return _pieces.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Initialized { get; private set; }

    public Vector2 CenterPoint
    {
        get => transform.TransformPoint(_localCenterOfGravity);
        set => transform.position += (Vector3)(value - CenterPoint);
    }


    private Vector2 _localCenterOfGravity;
    private float _pieceScale = 1f;
    private BoxCollider2D _collider;

    public BoxCollider2D Collider => _collider == null ?(_collider=GetComponent<BoxCollider2D>()) : _collider;

    public void Init(InitParams data)
    {
        if (Initialized)
        {
            return;
        }

        Color = data.Color;
        BoxGrid = new BoxGrid(data.TileSize, data.Spacing);
        
        foreach (var coordinate in data.Coordinates)
        {
            var localPosition = BoxGrid.GetRelativePositionForCoordinate(coordinate);
            var piece = Instantiate(_piecePrefab, transform);
            piece.Shape = this;
            
            piece.Size = data.TileSize;
            piece.Color = data.Color;
            piece.LocalCoordinate = coordinate;
            piece.transform.localPosition = localPosition;
            // ReSharper disable once Unity.InefficientPropertyAccess
            piece.transform.localScale = Vector3.one * PieceScale;
            _pieces.Add(piece);
        }


        
        _localCenterOfGravity = _pieces.Aggregate(Vector2.zero,(total, piece) => total+(Vector2)piece.transform.position)/_pieces.Count;

        var maxX = _pieces.Max(p=>Mathf.Abs(p.transform.localPosition.x-CenterPoint.x))+data.TileSize/2;
        var maxY = _pieces.Max(p=>Mathf.Abs(p.transform.localPosition.y-CenterPoint.y))+data.TileSize/2f;

        Collider.size = new Vector2(Mathf.Max(maxX * 2,3.5f), Mathf.Max(maxY * 2,3.5f));
        Collider.offset = CenterPoint;

        Initialized = true;
    }






    public struct InitParams
    {
        public Vector2Int[] Coordinates { get; set; }
        public int Color { get; set; }
        public float TileSize { get; set; }
        public float Spacing { get; set; }
    }
}

public partial class Shape
{
    public void RemovePiece(Piece piece, bool destroyPiece = true)
    {
        _pieces.Remove(piece);
        if (destroyPiece)
            Destroy(piece.gameObject);
    }

    public IEnumerable<Vector2Int> GetPieceInWorldCoordinates(Vector2Int? targetShapeCoordinate = null)
    {
        return _pieces.Select(piece => BoxGrid.RelativeCoordinate(targetShapeCoordinate ?? Coordinate
            , piece.LocalCoordinate, Vector2Int.zero));
    }

    public Vector2 GrapShapeCoordinate
    {
        get
        {
            var centerOfGravity = BoxGrid.GetCenterOfGravityCoordinate(GetPieceInWorldCoordinates(Vector2Int.zero));
            var nearestPiece = _pieces.First();
            var nearestDistance = (nearestPiece.LocalCoordinate - centerOfGravity).sqrMagnitude;

            foreach (var piece in _pieces)
            {
                var distance = (piece.LocalCoordinate - centerOfGravity).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPiece = piece;
                }
            }
            return nearestPiece.LocalCoordinate;
        }
    }

}




