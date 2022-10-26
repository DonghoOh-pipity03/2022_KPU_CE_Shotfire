using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectKind
{
    Flesh, Rock, Wood, ThinMetal, Metal, Concrete, Brick, Glass, Plywood,
    Asphalt, Sand, Mud, Ground, Water, Ceramics
}

public class EffectManager : MonoBehaviour
{
    [SerializeField] float offset = 0.02f;
    [Header("데칼")]
    [SerializeField] GameObject[] fleshDecal;
    [SerializeField] GameObject[] rockDecal;
    [SerializeField] GameObject[] woodDecal;
    [SerializeField] GameObject[] thinMetalDecal;
    [SerializeField] GameObject[] metalDecal;
    [SerializeField] GameObject[] concreteDecal;
    [SerializeField] GameObject[] brickDecal;
    [SerializeField] GameObject[] glassDecal;
    [SerializeField] GameObject[] plywoodDecal;
    [SerializeField] GameObject[] asphaltDecal;
    [SerializeField] GameObject[] sandDecal;
    [SerializeField] GameObject[] mudDecal;
    [SerializeField] GameObject[] groundDecal;
    [SerializeField] GameObject[] waterDecal;
    [SerializeField] GameObject[] ceramicDecal;
    [Header("파티클")]
    [SerializeField] GameObject fleshImpact;
    [SerializeField] GameObject rockImpact;
    [SerializeField] GameObject woodImpact;
    [SerializeField] GameObject thinMetalImpact;
    [SerializeField] GameObject metalImpact;
    [SerializeField] GameObject concreteImpact;
    [SerializeField] GameObject brickImpact;
    [SerializeField] GameObject glassImpact;
    [SerializeField] GameObject plywoodImpact;
    [SerializeField] GameObject asphaltImpact;
    [SerializeField] GameObject sandImpact;
    [SerializeField] GameObject mudImpact;
    [SerializeField] GameObject groundImpact;
    [SerializeField] GameObject waterImpact;
    [SerializeField] GameObject ceramicImpact;

    GameObject decal;   // 사용할 데칼
    GameObject impact;  // 사용할 파티클시스템

    private static EffectManager instance;
    public static EffectManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<EffectManager>();
            return instance;
        }
    }

    public void PlayEffect(Vector3 _point, Vector3 _normal, EffectKind _effectKind, Transform _parent = null)
    {
        switch (_effectKind)
        {
            case EffectKind.Flesh:
                decal = fleshDecal[Random.Range(0, fleshDecal.Length)];
                impact = fleshImpact;
                goto default;
            case EffectKind.Rock:
                decal = rockDecal[Random.Range(0, rockDecal.Length)];
                impact = rockImpact;
                goto default;
            case EffectKind.Wood:
                decal = woodDecal[Random.Range(0, woodDecal.Length)];
                impact = woodImpact;
                goto default;
            case EffectKind.ThinMetal:
                decal = thinMetalDecal[Random.Range(0, thinMetalDecal.Length)];
                impact = thinMetalImpact;
                goto default;
            case EffectKind.Metal:
                decal = metalDecal[Random.Range(0, metalDecal.Length)];
                impact = metalImpact;
                goto default;
            case EffectKind.Concrete:
                decal = concreteDecal[Random.Range(0, concreteDecal.Length)];
                impact = concreteImpact;
                goto default;
            case EffectKind.Brick:
                decal = brickDecal[Random.Range(0, brickDecal.Length)];
                impact = brickImpact;
                goto default;
            case EffectKind.Glass:
                decal = glassDecal[Random.Range(0, glassDecal.Length)];
                impact = glassImpact;
                goto default;
            case EffectKind.Plywood:
                decal = plywoodDecal[Random.Range(0, plywoodDecal.Length)];
                impact = plywoodImpact;
                goto default;
            case EffectKind.Asphalt:
                decal = asphaltDecal[Random.Range(0, asphaltDecal.Length)];
                impact = asphaltImpact;
                goto default;
            case EffectKind.Sand:
                decal = sandDecal[Random.Range(0, sandDecal.Length)];
                impact = sandImpact;
                goto default;
            case EffectKind.Mud:
                decal = mudDecal[Random.Range(0, mudDecal.Length)];
                impact = mudImpact;
                goto default;
            case EffectKind.Ground:
                decal = groundDecal[Random.Range(0, groundDecal.Length)];
                impact = groundImpact;
                goto default;
            case EffectKind.Water:
                decal = null;
                impact = waterImpact;
                goto default;
            case EffectKind.Ceramics:
                decal = ceramicDecal[Random.Range(0, ceramicDecal.Length)];
                impact = ceramicImpact;
                goto default;
            default:
                if (decal != null)
                {
                    var newDecal = Instantiate(decal, _point + _normal * offset, Quaternion.LookRotation(_normal));
                    if (_parent != null) newDecal.transform.SetParent(_parent);
                }
                if (impact != null)
                {
                    var newImpact = Instantiate(impact, _point, Quaternion.LookRotation(_normal));
                    Destroy(newImpact, 10);
                }
                break;
        }
    }

}
