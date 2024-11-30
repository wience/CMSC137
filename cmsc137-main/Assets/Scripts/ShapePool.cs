// /*
// Created by Darsan
// */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapePool : MonoBehaviour
{
    [SerializeField] private TileHolder _tileHolderPrefab;
    [SerializeField] private float _width;
    [SerializeField] private float _padding;
    [SerializeField] private float _space;
    [SerializeField] private int _holderCount;



    private readonly List<TileHolder> _holders  = new List<TileHolder>();

    public IEnumerable<Shape> Shapes => _holders.Where(holder=>holder.Shape!=null).Select(holder => holder.Shape);

    public IEnumerable<TileHolder> Holders => _holders;


    private void Awake()
    {
        GenerateTileHolders();
    }

    private void GenerateTileHolders()
    {
        var holderWidth = (_width - 2 * _padding - _space * (_holderCount - 1)) / _holderCount;
        for (var i = 0; i < _holderCount; i++)
        {
            var pos = transform.position -
                      Vector3.right * (_width / 2 - _padding - holderWidth * (0.5f + i) - _space * i);
            var tileHolder = Instantiate(_tileHolderPrefab, transform);
            tileHolder.transform.position = pos;
            _holders.Add(tileHolder);
        }
    }


    public void ReturnShapeWithAnim(Shape shape)
    {
        var tileHolder = _holders.FirstOrDefault(holder => holder.Shape == shape);
        tileHolder.ReturnShapeWithAnim();
    }



    

    public void AddShape(Shape shape)
    {
        var tileHolder = _holders.FirstOrDefault(holder => holder.Shape == null);
        
        if (tileHolder==null)
        {
            return;
        }
        tileHolder.Shape = shape;
        shape.transform.localScale = Vector3.one * 0.1f;
        shape.transform.position = tileHolder.transform.position;
        tileHolder.ReturnShapeWithAnim();

       
    }


    public void RemoveShape(Shape shape)
    {
        var tileHolder = _holders.FirstOrDefault(holder => holder.Shape == shape);

        if (tileHolder == null) return;
        
        if (shape.transform.parent == tileHolder.transform)
        {
            shape.transform.parent = null;
        }

        tileHolder.Shape = null;
    }

    public void Clear()
    {
        _holders.ForEach(holder => Destroy(holder.Shape.gameObject));
    }
}