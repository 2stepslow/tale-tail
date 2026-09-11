using UnityEngine;

/// <summary>
/// WebGL 자바스크립트 → 유니티 통신 스파이크용 브릿지.
/// SpriteRenderer가 붙은 GameObject에 부착한다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpikeBridge : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Debug.Log("SpikeBridge ready");
    }

    /// <summary>
    /// 스프라이트 색상을 변경한다.
    /// JS의 SendMessage로 호출되므로 매개변수는 string 하나만 받는다.
    /// </summary>
    /// <param name="hex">"#RRGGBB" 형식 문자열</param>
    public void SetColor(string hex)
    {
        // 파싱 실패 시 색을 건드리지 않고 경고만 남긴다.
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            spriteRenderer.color = color;
        }
        else
        {
            Debug.LogWarning($"SpikeBridge.SetColor: 색상 파싱 실패 - \"{hex}\"");
        }
    }
}
