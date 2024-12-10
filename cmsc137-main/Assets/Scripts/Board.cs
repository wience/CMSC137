using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Board : MonoBehaviour,IInitializable
{
    public event Action<int> Scored; 

    [SerializeField] protected BoardTile boardTilePrefab;
    [SerializeField] protected float _width;
    [SerializeField] protected int _rows;
    [SerializeField] protected float _spacing=0.05f;
    [SerializeField] protected bool debugCoordinate;
    [SerializeField] protected Piece piecePrefab;
    [SerializeField] protected DiamondBreakEffect _diamondBreakEffectPrefab;
    [SerializeField] protected AudioClip placeShapeClip, matchLineClip;


    private readonly List<BoardTile> _boardTiles = new List<BoardTile>();

    public IEnumerable<BoardTile> BoardTiles => _boardTiles;

    public float TileSize { get; private set; }

    public BoxGrid BoxGrid { get; private set; }
    public bool Intractable { get; set; } = true;

    public float Spacing => _spacing;
    

    protected readonly List<Piece> decoratePieces = new List<Piece>();
    private Vector2Int? _lastUpdatedMatchCoordinate;

    public BoardTile this[Vector2Int coordinate]
    {
        get
        {
            // var coordinateFromBase = coordinate+new Vector2Int(_rows/2,_rows/2);
            // return _boardTiles[coordinateFromBase.x*_rows+coordinateFromBase.y];
            return _boardTiles.FirstOrDefault(tile => tile.LocalCoordinate == coordinate);
        }
    }

    private Vector2Int? _matchCoordinateForDragShape;
    private readonly List<Vector2Int[]> _matchLines=new List<Vector2Int[]>();
    private readonly Dictionary<Piece,int> _modifiedColorPieces = new Dictionary<Piece, int>();
    public Shape DraggingShape { get;private set; }



    public bool Initialized { get; protected set; }
    


    public void Init()
    {
        if (Initialized)
        {
            return;
        }
        UpdateBoard();

        Initialized = true;
    }

    protected virtual void Awake()
    {
        Init();
    }


    public void AddPiece(Piece piece, Vector2Int to)
    {
        if (piece.Holder != null)
        {
            throw new InvalidOperationException();
        }

        var boardTile = this[to];
        if (boardTile == null)
            return;
        piece.EnableGlow();
        if (boardTile.Tile != null)
        {
            // Logic to handle the case where there is a tile on the boardTile
        }

        piece.transform.position = boardTile.transform.position;
        boardTile.Tile = piece;
        // piece.transform.parent = boardTile.transform;
         piece.LocalCoordinate = Vector2Int.zero;
    }


    public virtual void RemovePiece(Piece piece)
    {
        if (piece.Holder == null)
        {
            return;
        }

        ((BoardTile) piece.Holder).Tile = null;
    }


    public void AddShape(Shape shape, Vector2Int coordinate)
    {
        if (!CanPlaceShape(shape, coordinate))
        {
            Debug.LogError("Overlap Shape");
            return;
        }
        shape.transform.parent = transform;
        shape.Coordinate = coordinate;
        shape.transform.position = this[coordinate].transform.position;
       
        shape.ForEach(piece =>
            AddPiece(piece, BoxGrid.RelativeCoordinate(coordinate, piece.LocalCoordinate, Vector2Int.zero)));
        
        OnAddShape(shape);
    }

    public IEnumerator AddShapeWithAnim(Shape shape,Vector2Int coordinate)
    {
        if (!CanPlaceShape(shape, coordinate))
        {
            Debug.LogError("Overlap Shape");
            yield break;
        }

        var targetBoardTiles =
            shape.Select(p => this[BoxGrid.RelativeCoordinate(coordinate, p.LocalCoordinate, Vector2Int.zero)]).ToArray();
        shape.ForEach(p=>
        {
            var c = BoxGrid.RelativeCoordinate(coordinate, p.LocalCoordinate - shape.First().LocalCoordinate,
                    Vector2Int.zero);
           
        });
        var pieces = shape.ToArray();
        pieces.ForEach(p=>p.transform.parent = null);
        var piecesStartPoints = pieces.Select(p => p.transform.position).ToArray();
        var startPieceSize = pieces.First().Scale;
        var targetPieceSize = 1f;    
        Destroy(shape.gameObject);


        var enumerators = targetBoardTiles.Select((tile, j) =>  SimpleCoroutine.MoveTowardsEnumerator(speed:15f, onCallOnFrame: n =>
        {
            pieces[j].transform.position =
                Vector3.Lerp(piecesStartPoints[j], targetBoardTiles[j].transform.position, n);
            pieces[j].Scale= Mathf.Lerp(startPieceSize, targetPieceSize, n);
            pieces[j].IsFront = false;
        })).ToArray();

        yield return SimpleCoroutine.MergeParallel(enumerators, this);
        targetBoardTiles.ForEach((i, tile) =>
        {
            AddPiece(pieces[i],tile.LocalCoordinate);
            pieces[i].EnableGlow();
        });

    }


    protected virtual void OnAddShape(Shape shape)
    {

    }


    private void PlaySoundClipIfCan(AudioClip clip,float volume=0.95f)
    {
        if (!AudioManager.IsSoundEnable||clip==null)
        {
            return;
        }
        
        AudioSource.PlayClipAtPoint(clip,Camera.main.transform.position,volume);
    }



    public void OnDragShape()
    { 
        CalculateMatchCoordinateForDraggingShape();
        CalculateMatchLinesForDraggingShape();
        UpdateDecorativePieceForDraggingShape();
        UpdateMatchLineEffectForDraggingShape();
    }

    public void OnStartDragShape(Shape shape)
    {
        DraggingShape = shape;
    }

    public void ClearDragShape()
    {
        DraggingShape = null;

    }

    
    

    public IEnumerator HandlePlaceDragShape()
    {
        if (!CanPlaceDraggingShape())
        {
            yield break;
        }
        
        _modifiedColorPieces.Clear();
        ClearDecorativePieceEffect();
        Scored?.Invoke(DraggingShape.Count());
        PlaySoundClipIfCan(placeShapeClip);
        yield return AddShapeWithAnim(DraggingShape, _matchCoordinateForDragShape.Value);
        
        DraggingShape = null;
        _matchCoordinateForDragShape = null;

        if (_matchLines.Count > 0)
        {
            Scored?.Invoke(_matchLines.Count*_matchLines.First().Length);
            yield return CompleteMatchLines();
        }
        
        _matchLines.Clear();
 
    }

    public bool CanPlaceShapeAnyWhere(Shape shape)
    {
        return BoardTiles.Any(tile => CanPlaceShape(shape, tile.LocalCoordinate));
    }

    public bool CanPlaceDraggingShape()
    {
        return _matchCoordinateForDragShape != null;
    }
    



    private IEnumerator CompleteMatchLines()
    {
        var enumerators = _matchLines.Select(CompleteLine).ToList();
        yield return SimpleCoroutine.MergeParallel(enumerators, this);
    }

    private IEnumerator CompleteLine(Vector2Int[] line)
    {
        PlaySoundClipIfCan(matchLineClip);
        foreach (var c in line)
        {
            if (this[c].Tile!=null)
            {
                var piece = (Piece)this[c].Tile;
                RemovePiece(piece);
                var diamondBreakEffect = Instantiate(_diamondBreakEffectPrefab,piece.transform.position,Quaternion.identity);
                diamondBreakEffect.Color = piece.Color;
                Destroy(diamondBreakEffect.gameObject,5f);
                DestroyImmediate(piece.gameObject);
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
    


    private void UpdateDecorativePieceForDraggingShape()
    {
        var matchCoordinate = _matchCoordinateForDragShape;
        var shapePieceCount = DraggingShape.Count();
        if (decoratePieces.Count < shapePieceCount)
        {
            AddDecoratePieces(shapePieceCount - decoratePieces.Count);
        }

        if (matchCoordinate == null)
        {
            ClearDecorativePieceEffect();
        }
        else
        {
            for (var i = 0; i < decoratePieces.Count; i++)
            {
                if (i < shapePieceCount)
                {
                    decoratePieces[i].gameObject.SetActive(true);
                    decoratePieces[i].Size = TileSize;
                    decoratePieces[i].Color = DraggingShape.Color;
                    var coordinate = BoxGrid.RelativeCoordinate(matchCoordinate.Value,
                        DraggingShape.ElementAt(i).LocalCoordinate,
                        Vector2Int.zero);
                    decoratePieces[i].transform.position =
                        this[coordinate].transform.position;
                    decoratePieces[i].LocalCoordinate = coordinate;
                }
                else
                {
                    decoratePieces[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void ClearDecorativePieceEffect()
    {
        decoratePieces.ForEach(d=>d.gameObject.SetActive(false));
    }

    private void UpdateMatchLineEffectForDraggingShape()
    {
        _modifiedColorPieces.ForEach(p=>p.Key.Color = p.Value);
        _modifiedColorPieces.Clear();

        foreach (var piece in from line in _matchLines
            from c in line
            select this[c]
            into boardTile
            select boardTile.Tile != null ? (Piece) boardTile.Tile : null
            into piece
            where piece != null && piece.Color != DraggingShape.Color
            select piece)
        {
            _modifiedColorPieces.Add(piece,piece.Color);
            piece.Color = DraggingShape.Color;
        }

       
        
       
    }

    private void AddDecoratePieces(int count)
    {
        
        for (var i = 0; i < count; i++)
        {
            var piece = Instantiate(piecePrefab);
            piece.IsDecorator = true;
            decoratePieces.Add(piece);
        }
    }


    private void CalculateMatchCoordinateForDraggingShape()
    {
        _matchCoordinateForDragShape = DraggingShape==null ?null: GetMatchCoordinateForShape(DraggingShape);
    }

    private void CalculateMatchLinesForDraggingShape()
    {
        _matchLines.Clear();
        if (_matchCoordinateForDragShape==null)
        {
           
            return;
        }

        var matchBoardTiles = DraggingShape.Select(p=>BoxGrid.RelativeCoordinate(_matchCoordinateForDragShape.Value,p.LocalCoordinate,Vector2Int.zero)).Select(c=>this[c]).ToList();
        var verticalXs = matchBoardTiles.GroupBy(b => b.LocalCoordinate.x).Select(g => g.Key);
        var horizontalYs = matchBoardTiles.GroupBy(b => b.LocalCoordinate.y).Select(g => g.Key);

        var matchVerticalLines = verticalXs
            .Select(i => GetVerticalLineBoardTiles(i).ToArray())
            .Where(l=>l.Except(matchBoardTiles).All(b=>b.Tile!=null))
            .Select(l=>l.Select(b=>b.LocalCoordinate).ToArray()).ToList();
        
        var matchHorizontalLines = horizontalYs
            .Select(y => GetHorizontalLineBoardTiles(y).ToList())
            .Where(l => l.Except(matchBoardTiles).All(b => b.Tile != null))
            .Select(l=>l.Select(b=>b.LocalCoordinate).ToArray()).ToList();
        
        _matchLines.AddRange(matchVerticalLines);
        _matchLines.AddRange(matchHorizontalLines);
    }

    // public static bool ContainsTile(IEnumerable<BoardTile> boardTiles)
    // {
    //     return 
    // }


    public IEnumerable<BoardTile> GetVerticalLineBoardTiles(int x)
    {
        return Enumerable.Range(0, _rows).Select(n => this[new Vector2Int(x, n)]).Reverse();
    }

    public IEnumerable<BoardTile> GetHorizontalLineBoardTiles(int y)
    {
        return Enumerable.Range(0, _rows).Select(n => this[new Vector2Int(n, y)]);
    }
    
    public Vector2Int? GetNearestMatchCoordinateForShape(Shape shape,out float distance)
    {
        var piece = shape.First();

        var nearestCoordinate = GetNearestCoordinate(piece.transform.position);

        if (nearestCoordinate.x >= _rows || nearestCoordinate.x < 0 || nearestCoordinate.y >= _rows ||
            nearestCoordinate.y < 0)
        {
            distance = 0;
            return null;
        }


  

        var baseCoordinate = BoxGrid.RelativeCoordinate(nearestCoordinate,
            BoxGrid.InverseCoordinate(Vector2Int.zero, piece.LocalCoordinate), Vector2Int.zero);
        var coordinate = !CanPlaceShape(shape, baseCoordinate) ? null : (Vector2Int?)baseCoordinate;

        distance =  coordinate!=null?  (this[nearestCoordinate].transform.position - piece.transform.position).magnitude:-1f;
        return coordinate;

    }

    public Vector2Int? GetMatchCoordinateForShape(Shape shape)
    {
        var coordinate = GetNearestMatchCoordinateForShape(shape,out var distance);
        return distance <= TileSize * .95f / 2f ? coordinate : null;
    }

    public Vector2Int GetNearestCoordinate(Vector2 position)
    {
        return BoxGrid.GetNearestCoordinateForRelativePosition(position - ((Vector2) transform.position - new Vector2(1,1)*_width/2+new Vector2(1,1)*TileSize/2));
    }




    public bool CanPlaceShape(Shape shape, Vector2Int coordinate)
    {
        return shape.All(piece =>
        {
            var c = BoxGrid.RelativeCoordinate(coordinate, piece.LocalCoordinate, Vector2Int.zero);
            return this[c] != null && this[c].Tile == null;
        });
    }



    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position,new Vector3(_width,_width,0.1f));
    }

    [ContextMenu("Update Board")]
    public void UpdateBoard()
    {
        ResetBoard();
        TileSize = (_width - (_rows - 1) * _spacing) / _rows;
        BoxGrid = new BoxGrid(TileSize,_spacing);
        foreach (var boardTile in transform.GetComponentsInChildren<BoardTile>())
        {
            if (Application.isPlaying)
                Destroy(boardTile.gameObject);
            else
            {
                DestroyImmediate(boardTile.gameObject);
            }
        }

        _boardTiles.Clear();

        GenerateBoardTiles();

        BoardTile.DebugCoordinate = debugCoordinate;
    }

    private void GenerateBoardTiles()
    {
        for (var i = 0; i < _rows ; i++)
        {
            for (var j = 0; j < _rows ; j++)
            {
                BoardTile tile = null;
                var position = (Vector2) transform.position + new Vector2(-1, -1) * _width/2 +
                               new Vector2(1,1)*TileSize/2+
                               BoxGrid.GetRelativePositionForCoordinate(new Vector2Int(i, j));
                if (Application.isPlaying)
                {
                    tile = Instantiate(boardTilePrefab,position
                        ,
                        Quaternion.identity);
                }
                else
                {
#if UNITY_EDITOR
                    tile = (BoardTile) PrefabUtility.InstantiatePrefab(boardTilePrefab);

                    tile.transform.position = position;
#endif
                }

                tile.Size = TileSize;
                tile.transform.parent = transform;
                tile.LocalCoordinate = new Vector2Int(i, j);

                _boardTiles.Add(tile);
            }
        }
    }

    protected void Refresh()
    {

    }


    public virtual void ResetBoard()
    {
        decoratePieces.ForEach(piece => piece.gameObject.SetActive(false));

        foreach (var boardTile in BoardTiles)
        {
            if(boardTile.Tile is BoardTile tile)
            {
                if (tile.Tile != null)
                {
                    Destroy(tile.Tile.gameObject);
                }
                Destroy(tile.gameObject);
            }

            boardTile.Tile = null;
        }
    }
}