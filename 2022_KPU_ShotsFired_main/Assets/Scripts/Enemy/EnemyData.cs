using UnityEngine;


[CreateAssetMenu(fileName = "Enemy Data", menuName ="Scriptable Object/Enemy Data", order = int.MaxValue)]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] string enemyName;  // 적 이름
    public string EnemyName => enemyName;

    [Header("체력 파라미터")]
    [SerializeField] float maxHealth; // 시작 및 최대 체력
    public float MaxHealth => maxHealth;
    [SerializeField] float maxSuppress; // 최대 제압수치
    public float MaxSuppress => maxSuppress;
    [SerializeField] float unsuppressAmount; // 초당 제압해제수치
    public float UnsuppressAmount => unsuppressAmount;
    [SerializeField] float[] hitMultiple;  // 부위별 데미지 계수 / 머리,몸통,팔,다리
    public float[] HitMultiple => hitMultiple;

    [Header("시야 파라미터")]
    [SerializeField] float eyeDistance; // 시야 거리
    public float EyeDistance => eyeDistance;
    [SerializeField] float fieldOfView; // 시야각
    public float FieldOfView => fieldOfView;

    [Header("공격 파라미터")]
    [SerializeField] float engageDistance;  // 교전을 위한 이동 정지거리
    public float EngageDistance => engageDistance;
    [SerializeField] float melleeDistance; // 근접공격을 시작하는 거리
    public float MelleeDistance => melleeDistance;
    [SerializeField] float distanceTargetWeight;    // 공격 타겟 선정을 위한 거리별 가중치
    public float DistanceTargetWeight => distanceTargetWeight;
    [SerializeField] float attackerTargetWeight;    // 공격 타겟 선정을 위한 마지막 공격자 가중치
    public float AttackerTargetWeight => attackerTargetWeight;

    [Header("이동 파라미터")]
    [SerializeField] float moveSpeed;   // 이동 속도
    public float MoveSpeed => moveSpeed;
}
