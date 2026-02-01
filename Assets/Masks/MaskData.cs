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

[System.Serializable]
public class MaskData : ScriptableObject
{


    public string MaskId;

    public CharSprites MaskSprites;
    public CharSprites HighlightSprites;

    public RuntimeAnimatorController WalkingController;

    public Color ScreenTint = Color.clear;

    public bool ShouldTintScreen = false;
}
