using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 펫의 상태 수치를 보관한다.
/// 수치를 상태 머신이 아니라 이 클래스에 두는 이유:
/// 상태 머신은 "지금 어떤 표현을 할지"만 결정하고, 그 판단의 입력이 되는 데이터는 따로 있어야 한다.
/// 수치를 상태 머신 안에 두면 전환 규칙을 고칠 때마다 데이터까지 흔들리고,
/// 나중에 웹·서버가 값만 읽어가거나 저장하려 할 때 상태 머신을 거쳐야 해서 얽힌다.
/// </summary>
public class PetStats : MonoBehaviour
{
    public enum Mood { Neutral, Positive, Negative }
    public enum GrowthStage { Egg, Baby, Adult }

    const int MaxEnergy = 100;
    const int DayMissedCost = 20;
    const int PositiveExp = 10;
    const int NeutralExp = 5;
    const int NegativeExp = 5;

    [Header("오늘 상태")]
    [SerializeField, Tooltip("오늘 판정된 감정")]
    Mood mood = Mood.Neutral;

    [SerializeField, Range(0, MaxEnergy), Tooltip("하루를 거르면 줄어든다")]
    int energy = 80;

    [Header("누적")]
    [SerializeField, Min(0), Tooltip("일기를 쓴 누적 일수")]
    int intimacy = 0;

    [SerializeField, Tooltip("성장 단계. 전환 규칙은 다음 카드의 상태 머신이 맡는다")]
    GrowthStage growthStage = GrowthStage.Egg;

    [SerializeField, Min(0)]
    int experience = 0;

    // 외부에는 읽기 전용으로만 연다. 변경은 아래 메서드를 거쳐야 보정과 로그가 보장된다.
    public Mood CurrentMood => mood;
    public int Energy => energy;
    public int Intimacy => intimacy;
    public GrowthStage Stage => growthStage;
    public int Experience => experience;

    /// <summary>
    /// 수치가 바뀔 때마다 알린다.
    /// PetStats는 누가 듣는지 모르고, 구독은 상태 머신 쪽에서 한다.
    /// PetStats가 PetStateMachine을 직접 참조하면 서로 물고 있는 양방향 의존이 되어,
    /// 둘 중 하나만 떼어내 쓰거나 테스트하기 어려워진다.
    /// </summary>
    public event UnityAction Changed;

    /// <summary>
    /// 성장 단계를 바꾼다. 언제 바꿀지는 PetStateMachine이 판단한다.
    /// 필드를 public으로 열지 않은 이유: 아무 데서나 대입하면 "되돌아가지 않는다"는 규칙이 깨진다.
    /// </summary>
    public void SetGrowthStage(GrowthStage stage)
    {
        growthStage = stage;
        // 여기서는 Changed를 쏘지 않는다.
        // 성장 단계는 Idle/Happy/Sad/Tired 판정에 쓰이지 않고,
        // 쏘면 Evaluate() → SetGrowthStage() → Evaluate() 로 곧장 되돌아온다.
    }

    /// <summary>
    /// 감정 판정 결과를 반영한다.
    /// 웹에서 SendMessage로 직접 부를 수 있도록 매개변수는 string 하나만 받는다.
    /// </summary>
    /// <param name="label">"positive" | "negative" | "neutral"</param>
    public void ApplyEmotion(string label)
    {
        string key = (label ?? string.Empty).Trim().ToLowerInvariant();
        int gained;

        switch (key)
        {
            case "positive":
                mood = Mood.Positive;
                gained = PositiveExp;
                break;
            case "negative":
                mood = Mood.Negative;
                gained = NegativeExp;
                break;
            case "neutral":
                mood = Mood.Neutral;
                gained = NeutralExp;
                break;
            default:
                // 판정을 못 받았다고 기록을 버리지는 않는다. 중립으로 두고 진행한다.
                Debug.LogWarning($"PetStats.ApplyEmotion: 알 수 없는 감정 \"{label}\" — 중립으로 처리합니다.");
                mood = Mood.Neutral;
                gained = NeutralExp;
                break;
        }

        experience += gained;
        intimacy += 1;       // 기록한 날이 하루 늘었다
        energy = MaxEnergy;  // 기록하면 기운을 회복한다
        ClampValues();

        Debug.Log($"[PetStats] 감정 {mood} · 경험치 +{gained} (누적 {experience}) · 친밀도 {intimacy} · 기운 {energy}");
        Changed?.Invoke();
    }

    /// <summary>하루를 건너뛴 경우 기운이 줄어든다.</summary>
    public void OnDayMissed()
    {
        energy -= DayMissedCost;
        ClampValues();

        Debug.Log($"[PetStats] 하루 거름 · 기운 -{DayMissedCost} → {energy}");
        Changed?.Invoke();
    }

    // 범위 보정은 이 메서드 한 곳에만 둔다.
    // 호출부마다 Clamp를 흩뿌리면 한 군데를 고칠 때 다른 곳과 규칙이 어긋난다.
    void ClampValues()
    {
        energy = Mathf.Clamp(energy, 0, MaxEnergy);
        intimacy = Mathf.Max(0, intimacy);
        experience = Mathf.Max(0, experience);
    }

    // 인스펙터에서 손으로 고친 값도 같은 규칙을 타게 한다.
    void OnValidate() => ClampValues();
}
