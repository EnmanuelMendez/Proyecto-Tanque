using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra el nombre y matrícula del estudiante en la esquina superior derecha de la pantalla.
/// Adjuntar a un Canvas en la escena.
/// </summary>
public class PlayerInfoUI : MonoBehaviour
{
    private void Start()
    {
        // Crear el Canvas si no existe uno adecuado
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Siempre al frente
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Crear el objeto de texto
        GameObject textObj = new GameObject("PlayerInfoText");
        textObj.transform.SetParent(transform, false);

        Text infoText = textObj.AddComponent<Text>();
        infoText.text = "Enmanuel Mendez\n2-19-0262";
        infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        infoText.fontSize = 20;
        infoText.color = Color.white;
        infoText.alignment = TextAnchor.UpperRight;
        infoText.horizontalOverflow = HorizontalWrapMode.Overflow;
        infoText.verticalOverflow = VerticalWrapMode.Overflow;

        // Agregar sombra para mejor legibilidad
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);

        // Posicionar en la esquina superior derecha
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-10, -10);
        rectTransform.sizeDelta = new Vector2(300, 60);
    }
}
