using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AutoCameraSizer : MonoBehaviour, IInilizable
{

    [SerializeField] private List<AspectAndWidth> _aspectAndWidths = new List<AspectAndWidth>();

    private Camera _camera;


    public bool Initialized { get; private set; }

    public void Init()
    {
        if (Initialized)
        {
            return;
        }

        _camera = GetComponent<Camera>();


        var aspectRatio = Mathf.Max((Screen.height + 0f) / Screen.width);
        _camera.orthographicSize = aspectRatio * GetWidthForCurrentAspectRatio() * 0.5f;


        Initialized = true;
    }

    public float GetWidthForCurrentAspectRatio()
    {
        var aspect = (float)Screen.height / Screen.width;
        var baseIndex = 0;
        
        for (var i = 0; i < _aspectAndWidths.Count; i++)
        {
            var aspectAndWidth = _aspectAndWidths[i];
            if (aspectAndWidth.aspect>=aspect)
            {
                baseIndex = i-1;
                break;
            }
        }

        if (baseIndex < 0)
            return _aspectAndWidths.First().width;

        if (baseIndex >= _aspectAndWidths.Count - 1)
            return _aspectAndWidths.Last().width;

        var n = Mathf.InverseLerp(_aspectAndWidths[baseIndex].aspect,_aspectAndWidths[baseIndex+1].aspect,aspect);
        Debug.Log("N:"+n+" Base Index:"+baseIndex);
        return Mathf.Lerp(_aspectAndWidths[baseIndex].width, _aspectAndWidths[baseIndex + 1].width, n);
    }
    
    private void Awake()
    {
        Init();
    }

    [Serializable]
    private struct AspectAndWidth
    {
        public float aspect;
        public float width;
    }

}


public interface IInilizable
{
    bool Initialized { get; }
    void Init();
}