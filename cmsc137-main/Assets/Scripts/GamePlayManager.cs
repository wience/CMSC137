// /*
// Created by Darsan
// */

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GamePlayManager : MonoBehaviour
{
    public event Action GameOver;

    [SerializeField] private ShapePool _pool;
    [SerializeField] private Board _board;
    [SerializeField] private Shape _shapePrefab;
    [SerializeField] private ShapeProvider _shapeProvider;
    [SerializeField] private AudioClip _returnShapeClip, _overClip;


    public bool Dragging => DraggingShape != null;

    public bool Active { get; set; }

    private Vector2 _lastDragPoint;

    // ReSharper disable once PossibleNullReferenceException
    public Vector2 MouseWorldPoint => Camera.main.ScreenToWorldPoint(Input.mousePosition);
    public Shape DraggingShape { get; set; }
    public Vector2 DraggingPoint { get; private set; }
    public bool IsOver { get; private set; }

    public Board Board => _board;


    private void Awake()
    {
        AddRandomShapes();
    }

    public void AddRandomShapes()
    {
        var count = _pool.Holders.Count(h => h.Shape == null);
        for (var i = 0; i < count; i++)
        {
            var shape = Instantiate(_shapePrefab);
            shape.Init(new Shape.InitParams
            {
                Spacing = _board.Spacing,
                TileSize = _board.TileSize,
                Color = GetRandomShapeColor(),
                Coordinates = GetRandomShapeCoordinates()
            });
            _pool.AddShape(shape);
        }
    }


    private Vector2Int[] GetRandomShapeCoordinates()
    {
        return _shapeProvider.GetRandomShape();
    }

    private int GetRandomShapeColor()
    {
        return UnityEngine.Random.Range(0, PieceSprites.DEFAULT.Count());
    }

    private void Update()
    {
        if (!Active || IsOver)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            var col = Physics2D.OverlapPoint(MouseWorldPoint);
            var shape = col != null ? col.GetComponent<Shape>() : null;
            if (shape == null)
            {
                return;
            }

            DraggingShape = shape;
            ShapeOnPointerDown();
            _lastDragPoint = MouseWorldPoint;
        }

        if (Dragging)
        {
            var mouseWorldPoint = MouseWorldPoint;
            ShapeOnPointerDrag(mouseWorldPoint - _lastDragPoint);
            _lastDragPoint = mouseWorldPoint;
        }

        if (Input.GetMouseButtonUp(0) && Dragging)
        {
            ShapeOnPointerUp();
            DraggingShape = null;
        }
    }


    private void ShapeOnPointerDown()
    {
        DraggingShape.IsFront = true;
        DraggingPoint = MouseWorldPoint;
        _board.OnStartDragShape(DraggingShape);
        StartCoroutine(ShapePointerDownAnim());
    }

    private IEnumerator ShapePointerDownAnim()
    {
        var startScale = DraggingShape.transform.localScale;
        var pieceStartScale = DraggingShape.PieceScale;
        var elapsedOffset = 0f;
        var offset = 2f;
        yield return SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
        {
            DraggingShape.transform.localScale = Vector3.Lerp(startScale, Vector3.one, n);
            DraggingShape.PieceScale = Mathf.Lerp(pieceStartScale, 0.9f, n);
            elapsedOffset = Mathf.MoveTowards(elapsedOffset, offset, n);
            DraggingShape.transform.position = DraggingPoint + elapsedOffset * Vector2.up;
        }, speed: 13f);
    }

    private void ShapeOnPointerUp()
    {
        StartCoroutine(HandleShapeOnPointerUp());
    }

    private IEnumerator HandleShapeOnPointerUp()
    {
        if (_board.CanPlaceDraggingShape())
        {
            _pool.RemoveShape(DraggingShape);

            if (!_pool.Shapes.Any())
            {
                AddRandomShapes();
            }

            yield return _board.HandlePlaceDragShape();
            DraggingShape = null;

            if (!_pool.Shapes.Any(_board.CanPlaceShapeAnyWhere))
            {
                OverTheGame();
            }
        }
        else
        {
            DraggingShape.IsFront = false;
            _board.ClearDragShape();
            PlaySoundClipIfCan(_returnShapeClip);
            _pool.ReturnShapeWithAnim(DraggingShape);
            DraggingShape = null;
        }
    }
    
    private void PlaySoundClipIfCan(AudioClip clip,float volume=0.95f)
    {
        if (!AudioManager.IsSoundEnable||clip==null)
        {
            return;
        }
        
        AudioSource.PlayClipAtPoint(clip,Camera.main.transform.position,volume);
    }

    private void OverTheGame()
    {
        PlaySoundClipIfCan(_overClip);
        _pool.Shapes.ForEach(s => s.Opacity = 0.5f);
        IsOver = true;
        GameOver?.Invoke();
    }


    private void ShapeOnPointerDrag(Vector2 delta)
    {
        DraggingShape.transform.position += (Vector3) delta;
        DraggingPoint = MouseWorldPoint;
        _board.OnDragShape();
    }
}