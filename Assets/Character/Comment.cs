using UnityEngine;

public class Comment : MonoBehaviour
{
    [TextArea(5, 20)] // Multi-line text box in the inspector
    public string notes = "Write your notes here...";
}
