using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public class CharSprites
{
    public Sprite NSprite;
    public Sprite SSprite;
    public Sprite ESprite;
    public Sprite WSprite;
}

[CreateAssetMenu(menuName = "Game/Mask")]
public class MaskData : ScriptableObject
{


    public string MaskId;

    public CharSprites MaskSprites;
    public CharSprites HighlightSprites;

    public AnimatorController WalkingController;

    public Color ScreenTint = Color.clear;

    public bool ShouldTintScreen = false;
}
