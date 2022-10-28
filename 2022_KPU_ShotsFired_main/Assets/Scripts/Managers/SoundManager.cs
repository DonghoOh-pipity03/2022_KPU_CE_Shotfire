using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnvSFX { none, lobby, basement, office, rooptop } // EnvSFX 종류

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;

    [Tooltip("BGM")]
    [SerializeField] AudioClip[] envSFX;
    [Tooltip("동시 재생이 제한되는 SFX의 연속재생 대기시간")]
    [SerializeField] float SFXWaitTime = 0.2f;

    List<string> playingSFX = new List<string>(); // 재생중인 SFX의 목록을 가지는 리스트 

    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<SoundManager>();
            return instance;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SetEnv(EnvSFX.lobby);
    }

    #region 함수
    public void SetEnv(EnvSFX _envSFX)
    {
        if (audioSource.clip == envSFX[(int)_envSFX]) return;
        audioSource.clip = envSFX[(int)_envSFX];
        audioSource.Play();
    }

    // SFX 재생 함수
    public void PlaySFX(AudioClip _clip, Vector3 _pos, string _SFXName)
    {
        GameObject gameObject = new GameObject("SFX " + _SFXName);
        gameObject.transform.position = _pos;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(_clip);

        Destroy(gameObject, _clip.length);
    }
    public void PlaySFX(AudioClip _clip, float _volume, Vector3 _pos, string _SFXName)
    {
        GameObject gameObject = new GameObject("SFX " + _SFXName);
        gameObject.transform.position = _pos;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = _volume;
        audioSource.PlayOneShot(_clip);

        Destroy(gameObject, _clip.length);
    }

    // 짧고 반복되는 SFX 재생 함수
    public void PlayLimitSFX(AudioClip _clip, Vector3 _pos, string _SFXName)
    {

        // 이미 재생중이면 재생불가
        if (playingSFX.Contains(_SFXName)) return;

        GameObject gameObject = new GameObject("SFX " + _SFXName);
        gameObject.transform.position = _pos;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(_clip);

        playingSFX.Add(_SFXName);

        Destroy(gameObject, _clip.length);
        StartCoroutine(WaitOneShot(_SFXName, SFXWaitTime));
    }
    public void PlayLimitSFX(AudioClip _clip, float _lifeTime, float _waitTime, Vector3 _pos, string _SFXName)
    {

        // 이미 재생중이면 재생불가
        if (playingSFX.Contains(_SFXName)) return;

        GameObject gameObject = new GameObject("SFX " + _SFXName);
        gameObject.transform.position = _pos;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(_clip);

        playingSFX.Add(_SFXName);

        Destroy(gameObject, _lifeTime);
        StartCoroutine(WaitOneShot(_SFXName, _waitTime));
    }
    public void PlayLimitSFX(AudioClip _clip, float _volume, float _lifeTime, float _waitTime, Vector3 _pos, string _SFXName)
    {

        // 이미 재생중이면 재생불가
        if (playingSFX.Contains(_SFXName)) return;

        GameObject gameObject = new GameObject("SFX " + _SFXName);
        gameObject.transform.position = _pos;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = _volume;
        audioSource.PlayOneShot(_clip);

        playingSFX.Add(_SFXName);

        Destroy(gameObject, _lifeTime);
        StartCoroutine(WaitOneShot(_SFXName, _waitTime));
    }

    IEnumerator WaitOneShot(string _SFXName, float _SFXWaitTime)
    {
        yield return new WaitForSeconds(_SFXWaitTime);
        if (playingSFX.Count != 0)
        {
            playingSFX.Remove(_SFXName);
        }
    }
    #endregion
}
