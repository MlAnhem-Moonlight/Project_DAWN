using TMPro;
using UnityEngine;

public class TextController : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Color textColor = Color.white;

    public void ChangeColor()
    {
        text.color = textColor;
    }
}
