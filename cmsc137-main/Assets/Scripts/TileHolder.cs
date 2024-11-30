// /*
// Created by Darsan
// */

using UnityEngine;

public class TileHolder : MonoBehaviour
{
    [SerializeField] private float _shapeScale = 0.5f;
    private Shape _shape;


    public Shape Shape
    {
        get => _shape;
        set
        {
            if(value!=null)
            value.transform.parent = transform;
            _shape = value;
        }
    }


    public void ReturnShapeWithAnim()
    {
        var targetScale = _shapeScale * Vector3.one;
        var targetPieceScale = 1f;
        var startPieceScale = Shape.PieceScale;


        var startScale = Shape.transform.localScale;

        var centerPoint = Shape.CenterPoint;


        StartCoroutine(SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
        {
            Shape.transform.localScale = Vector3.Lerp(startScale, targetScale, n);
            Shape.CenterPoint = Vector2.Lerp(centerPoint, transform.position, n);
            Shape.PieceScale = Mathf.Lerp(startPieceScale, targetPieceScale, n);
        }, onFinished: ReturnShape, speed: 20f));
    }

    public void ReturnShape()
    {
        Shape.CenterPoint = transform.position;
        Shape.transform.localScale = _shapeScale * Vector3.one;
        Shape.PieceScale = 1f;
    }
}