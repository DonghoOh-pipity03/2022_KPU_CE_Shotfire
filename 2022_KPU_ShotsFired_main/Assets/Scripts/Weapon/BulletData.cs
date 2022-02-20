using UnityEngine;

[CreateAssetMenu(fileName = "Bullet Data", menuName ="Scriptable Object/Bullet Data", order = int.MaxValue)]
public class BulletData : ScriptableObject
{
    [SerializeField]private string bulletName;  // 총알이름
    public string BulletName{get{return bulletName;} set{}}
    [SerializeField]private float damage; // 총알 데미지
    public float Damage{ get{return damage;} set{}}
        // 총격 충격력
    [SerializeField]private float speed;  // 총알 속도
    public float Speed{get{return speed;} set{}}
    [SerializeField]private float lifeTime; // 총알 생명시간
    public float LifeTime{ get{return lifeTime;} set{}}
}
