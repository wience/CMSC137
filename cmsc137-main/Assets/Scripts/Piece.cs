using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : Tile
{
    [SerializeField] protected SpriteRenderer spRenderer;
    

    private int _color;
    private bool _isDecorator;

    public Shape Shape { get; set; }

    public bool IsFront
    {
        get => spRenderer.sortingLayerName == "Front";
        set => spRenderer.sortingLayerName = value ? "Front" : "Default";
    }

    public bool IsDecorator
    {
        get => _isDecorator;
        set
        {
            _isDecorator = value;
            UpdateSpriteColorForDecorator();
            GetComponents<Collider>().ForEach(c => c.enabled = !value);
        }
    }

    public Color ColorValue
    {
        get => spRenderer.color;
        set => spRenderer.color = value;
    }

    public int Color
    {
        get => _color;
        set
        {
            _color = value;


            var i = value % PieceSprites.DEFAULT.Count();
            spRenderer.sprite = PieceSprites.DEFAULT.ElementAt(i);

            UpdateSpriteColorForDecorator();
        }
    }

    private void UpdateSpriteColorForDecorator()
    {
        spRenderer.color = spRenderer.color.WithAlpha(IsDecorator ? 0.6f : 1f);
    }

    public override int Order => 2;
}