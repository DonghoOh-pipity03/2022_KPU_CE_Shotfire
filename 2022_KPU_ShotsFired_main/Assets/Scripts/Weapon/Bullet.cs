using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{   
    private Rigidbody m_rigidbody;
    [SerializeField] private BulletData bulletData;
    #region 전역 변수
    [Header("이하 디버그용 표시")]
    [SerializeField] private string bulletName; // 총알 이름
    [SerializeField] private float bulletSpeed;   // 총알 속도
    [SerializeField] private float bulletDamage;    // 총알 데미지
    [SerializeField] private float bulletSuppress;  // 제압량
    [SerializeField] private float lifeTime; // 생명주기
    [SerializeField] private float gravityMultiple;// 중력 계수
    #endregion
    #region 전역 동작 변수
    private float enabledTime;  // 발사된 시간
    private GameObject topLevelParent;  // 총알 주인
    public IObjectPool<Bullet> poolToReturn;    // 자신을 관리하는 오브젝트 풀 변수
    #endregion

   private void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();
        SettingData();
    }

    private void OnEnable() {
        enabledTime = Time.time;
    }
 
    private void FixedUpdate() {
        if( Time.time > enabledTime + lifeTime) poolToReturn.Release(this);
    }

    // 오브젝트 풀에서 생성되었을 때    
    public void Reset(GameObject _topLevelParent, Vector3 _firePosition, Vector3 _direction) 
    {   
        topLevelParent = _topLevelParent;
        transform.position = _firePosition;
        transform.rotation = Quaternion.Euler(_direction);
        m_rigidbody.velocity = transform.forward * bulletSpeed + Physics.gravity * gravityMultiple;
    }

    private void OnTriggerEnter(Collider other) 
    {
        // 제압
        var target1 = other.GetComponent<SuppressPoint>();
        if( target1 != null) 
        {   
            if(other.transform.root.tag != topLevelParent.tag) target1.ApplySuppress(bulletSuppress);
        }

        // 공격
        var target2 = other.GetComponent<IDamageable>();
        if( target2 != null)
        {
            if(other.transform.root.tag != topLevelParent.tag)
            {
                 DamageMessage damageMessage;

                damageMessage.attacker = topLevelParent;
                damageMessage.ID = Random.Range(0, 2147483647); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
                damageMessage.damageKind = DamageKind.bullet;
                damageMessage.damageAmount = bulletDamage;
                damageMessage.suppressAmount = bulletSuppress;
                damageMessage.hitPoint = transform.position;
                damageMessage.hitNormal = transform.forward * -1;

                target2.ApplyDamage(damageMessage);
                poolToReturn.Release(this);
            }
        }
    }
    private void OnCollisionEnter(Collision other) 
    {
        // 레벨디자인에 맞았을 때
        poolToReturn.Release(this);
    }

    // 총알 정보를 SO에서 초기화
    private void SettingData()
    {
        bulletSpeed = bulletData.Speed;
        bulletDamage = bulletData.Damage;
        bulletSuppress = bulletData.Suppress;
        lifeTime = bulletData.LifeTime;
        bulletName = bulletData.BulletName;
        gravityMultiple = bulletData.GravityMultiple;
    }
}