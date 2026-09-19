using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 펫이 지금 어떤 표현 상태인지 결정한다.
/// 자기 데이터는 하나도 갖지 않고 PetStats를 읽기만 한다 — 판단(여기)과 데이터(PetStats)를 갈라둬야
/// 판정 기준을 바꿔도 수치가 흔들리지 않는다.
/// </summary>
[RequireComponent(typeof(PetStats))]
public class PetStateMachine : MonoBehaviour
{
    public enum PetState { Idle, Happy, Sad, Tired }

    [Header("상태 판정")]
    [SerializeField, Range(0, 100), Tooltip("이 값 미만이면 기분과 상관없이 지친 상태")]
    int tiredEnergyThreshold = 40;

    [Header("성장 단계 기준 (누적 경험치)")]
    [SerializeField, Min(0)] int babyExperience = 30;
    [SerializeField, Min(0)] int adultExperience = 80;

    PetStats stats;
    // Awake 전에 Evaluate가 불려도 동작하도록 지연 조회한다.
    PetStats Stats => stats != null ? stats : (stats = GetComponent<PetStats>());

    PetState currentState = PetState.Idle;
    public PetState CurrentState => currentState;

    /// <summary>
    /// 상태가 실제로 바뀔 때만 발생한다. (이전 상태, 새 상태)
    /// 나중 카드에서 애니메이션을 여기에 붙인다.
    /// </summary>
    public event UnityAction<PetState, PetState> StateChanged;

    void Awake()
    {
        stats = GetComponent<PetStats>();
    }

    // 구독은 상태 머신 쪽에서만 건다. PetStats는 이 클래스의 존재를 모른다.
    void OnEnable() => Stats.Changed += Evaluate;
    void OnDisable() => Stats.Changed -= Evaluate;

    void Start() => Evaluate();

    /// <summary>PetStats를 다시 읽어 성장 단계와 표현 상태를 갱신한다.</summary>
    public void Evaluate()
    {
        UpdateGrowthStage();

        PetState next;
        string reason;

        // 판정 순서: 지침을 먼저 본다. 기운이 바닥이면 기분보다 그쪽이 앞선다.
        if (Stats.Energy < tiredEnergyThreshold)
        {
            next = PetState.Tired;
            reason = $"기운 {Stats.Energy} < {tiredEnergyThreshold}";
        }
        else
        {
            switch (Stats.CurrentMood)
            {
                case PetStats.Mood.Positive: next = PetState.Happy; break;
                case PetStats.Mood.Negative: next = PetState.Sad; break;
                default: next = PetState.Idle; break;
            }
            reason = $"감정 {Stats.CurrentMood}";
        }

        if (next == currentState) return;  // 그대로면 로그도 이벤트도 없다

        var previous = currentState;
        currentState = next;

        Debug.Log($"[PetStateMachine] 상태 {previous} → {currentState} (이유: {reason})");
        StateChanged?.Invoke(previous, currentState);
    }

    // 성장은 한 방향으로만 간다. 경험치가 줄더라도 단계는 내려가지 않는다.
    void UpdateGrowthStage()
    {
        var current = Stats.Stage;
        var target = current;

        if (Stats.Experience >= adultExperience) target = PetStats.GrowthStage.Adult;
        else if (Stats.Experience >= babyExperience) target = PetStats.GrowthStage.Baby;

        // enum 선언 순서(Egg < Baby < Adult)를 그대로 비교해 역행을 막는다
        if (target <= current) return;

        Stats.SetGrowthStage(target);
        Debug.Log($"[PetStateMachine] 성장 {current} → {target} (경험치 {Stats.Experience})");
    }
}
