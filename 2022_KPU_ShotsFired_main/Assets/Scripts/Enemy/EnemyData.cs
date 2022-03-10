using UnityEngine;


[CreateAssetMenu(fileName = "Enemy Data", menuName ="Scriptable Object/Enemy Data", order = int.MaxValue)]
public class EnemyData : ScriptableObject
{
    [SerializeField]private string enemyName;  // 적 이름
    public string EnemyName{get{return enemyName;} set{}}

}
