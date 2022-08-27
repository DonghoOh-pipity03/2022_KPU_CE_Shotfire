using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BossAgent : EnemyAgent
{
    protected override void Awake() {
        base.Awake();
    }

    public override void OnEnable() {
        base.OnEnable();
    }

    public override void OnDisable() {
        base.OnDisable();
    }

    protected override void Update() {
        base.Update();
        GameUIManager.Instance.UpdateBossHealth((int)maxHealth, (int)curHealth);
    }

    protected override void LateUpdate() {
        base.LateUpdate();
    }

    public float GetHealth(){return curHealth;}
}
