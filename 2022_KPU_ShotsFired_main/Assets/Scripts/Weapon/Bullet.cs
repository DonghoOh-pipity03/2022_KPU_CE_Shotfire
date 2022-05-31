using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{   
    private Rigidbody m_rigidbody;
    [SerializeField] public BulletData bulletData;
    #region 전역 변수
    #endregion
    #region 전역 동작 변수
    GameObject topLevelParent;  // 총알 주인
    float enabledTime;  // 발사된 시간
    public IObjectPool<Bullet> poolToReturn;    // 자신을 관리하는 오브젝트 풀 변수
    #endregion

   private void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable() {
        enabledTime = Time.time;
    }
 
    private void FixedUpdate() {
        if( Time.time > enabledTime + bulletData.LifeTime) poolToReturn.Release(this);
    }

    // 오브젝트 풀에서 생성되었을 때    
    public void Reset(GameObject _topLevelParent, Vector3 _firePosition, Vector3 _direction) 
    {   
        topLevelParent = _topLevelParent;
        transform.position = _firePosition;
        transform.rotation = Quaternion.Euler(_direction);
        m_rigidbody.velocity = transform.forward * bulletData.Speed + Physics.gravity * bulletData.GravityMultiple;
    }

    private void OnTriggerEnter(Collider other) 
    {
        // 제압
        var target1 = other.GetComponent<SuppressPoint>();
        if( target1 != null) 
        {   
            if(other.transform.root.tag != topLevelParent.tag) target1.ApplySuppress(bulletData.Suppress);
        }
    }
    private void OnCollisionEnter(Collision other) 
    {
        // 공격
        var target = other.gameObject.GetComponent<IDamageable>();
        if( target != null)
        {
            if(other.transform.root.tag != topLevelParent.tag)
            {
                 DamageMessage damageMessage;

                damageMessage.attacker = topLevelParent;
                damageMessage.ID = Random.Range(0, 2147483647); // 사용안함_원래는 관통시스템 사용시 중복 공격 방지용이였던 것
                damageMessage.damageKind = DamageKind.bullet;
                damageMessage.damageAmount = bulletData.Damage;
                damageMessage.suppressAmount = bulletData.Suppress;
                damageMessage.hitPoint = other.contacts[0].point;
                damageMessage.hitNormal =  other.contacts[0].normal;

                target.ApplyDamage(damageMessage);
            }
        }
        // 레벨디자인에 맞았을 때와 공격한 후
        poolToReturn.Release(this);
    }
}