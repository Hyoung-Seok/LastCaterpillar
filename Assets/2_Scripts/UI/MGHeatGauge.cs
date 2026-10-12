using UnityEngine;
using UnityEngine.UI;

public class MGHeatGauge : MonoBehaviour
{
    [SerializeField] private CoaxialMG coaxialMG;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private Image fillImage;

    [Header("Color Config")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.85f, 0.3f);
    [SerializeField] private Color overheatColor = new Color(0.9f, 0.2f, 0.2f);

    private void Update()
    {
        // 앵커 x 최대값으로 게이지 길이를 조절 (스프라이트 없이 동작)
        var max = fillRect.anchorMax;
        max.x = coaxialMG.Heat;
        fillRect.anchorMax = max;

        fillImage.color = coaxialMG.IsOverheated ? overheatColor : normalColor;
    }
}
