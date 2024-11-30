using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SpritesGroup : ScriptableObject,IEnumerable<Sprite>
{
    [SerializeField] private List<Sprite> _sprites = new List<Sprite>();
    public IEnumerator<Sprite> GetEnumerator() => _sprites.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}