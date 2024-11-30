
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class PieceSprites : SpritesGroup
{
    public const string DEFAULT_NAME = nameof(PieceSprites);
    public static PieceSprites DEFAULT => Resources.Load<PieceSprites>(DEFAULT_NAME);
    
#if UNITY_EDITOR
    [MenuItem("MyGames/Pieces")]
    public static void Open()
    {
        GamePlayEditorManager.OpenScriptableAtDefault<PieceSprites>();
    }
#endif
}