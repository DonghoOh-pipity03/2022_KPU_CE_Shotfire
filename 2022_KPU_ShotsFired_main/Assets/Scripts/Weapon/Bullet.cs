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
        m_rigidbody.velocity = transform.forward * bulletSpeed;
    }

    private void OnTriggerEnter(Collider other) 
    {
        var target = other.GetComponent<SuppressPoint>();

        if( target != null)
        {
            DamageMessage damageMessage;

            damageMessage.attacker = topLevelParent;
            damageMessage.ID = Random.Range(0, 2147483647); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
            damageMessage.damageKind = DamageMessage.DamageKind.bullet;
            damageMessage.damageAmount = bulletDamage;
            damageMessage.suppressAmount = bulletSuppress;
            damageMessage.hitPoint = Vector3.zero;
            damageMessage.hitNormal = Vector3.zero;

            target.ApplySuppress(damageMessage);
        }
    }

    private void OnCollisionEnter(Collision other) 
    {
        var target = other.collider.GetComponent<IDamageable>();

        // 체력이 있는 물체를 맞췄을 때
        if( target != null)
        {
            DamageMessage damageMessage;

            damageMessage.attacker = topLevelParent;
            damageMessage.ID = Random.Range(0, 2147483647); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
            damageMessage.damageKind = DamageMessage.DamageKind.bullet;
            damageMessage.damageAmount = bulletDamage;
            damageMessage.suppressAmount = bulletSuppress;
            damageMessage.hitPoint = other.contacts[0].point;
            damageMessage.hitNormal = other.contacts[0].normal;

            target.ApplyDamage(damageMessage);
        }

        // 레벨 디자인에 맞췄을 때 -> 밑에서 같이 처리
        /*
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            poolToReturn.Release(this);
        }
        */
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
    }
}