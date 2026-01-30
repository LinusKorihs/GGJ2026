using UnityEngine;

[CreateAssetMenu(menuName = "Game/Mask")]
public class MaskData : ScriptableObject
{
    public string maskId;

    public Sprite maskSprite;

    public Color screenTint = Color.clear;
    
    public bool shouldTintScreen = false;
}
