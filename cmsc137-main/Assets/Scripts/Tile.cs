using System;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    [HideInInspector] [SerializeField] private Vector2Int _localCoordinate;
    private bool _inverted;
    private float _size=1;
    private float _scale = 1;

    public event Action<Tile> Clicked;

    public float Size
    {
        get => _size;
        set
        {
            _size = value;
            UpdateTransformScale();
        }
    }

    private void UpdateTransformScale()
    {
        transform.localScale = Vector3.one * (Size * Scale);
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            UpdateTransformScale();
        }
    }
    
    public virtual Vector2Int LocalCoordinate
    {
        get => _localCoordinate;
        set => _localCoordinate = value;
    }

    public Tile Holder { get; set; }



    public abstract int Order { get; }

    public virtual bool Interaction { get; set; } = true;

    public void OnClick()
    {
        Clicked?.Invoke(this);
    }
}