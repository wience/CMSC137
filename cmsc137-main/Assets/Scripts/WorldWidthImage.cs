using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WorldWidthImage : MonoBehaviour
{
    [SerializeField] private float _worldWidth;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        var image = GetComponent<Image>();
        var width = WorldToCanvasWidth(_worldWidth);
        Debug.Log(width);
        image.rectTransform.sizeDelta = new Vector2(width,
            width * (image.sprite.texture.height+0f) / image.sprite.texture.width);

    }


    private float WorldToCanvasWidth(float width)
    {
        var worldWidth = Mathf.Abs((Camera.main.ScreenToWorldPoint(Vector2.zero) -
                          Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height))).x);

        var canvas = transform.root.GetComponent<Canvas>();

        if (canvas == null)
            throw new Exception();

        var rectTransform = (RectTransform)canvas.transform;
        Debug.Log("Canvas Width:" + rectTransform.rect.width);

        return rectTransform.rect.width * width / (worldWidth);
    }
}