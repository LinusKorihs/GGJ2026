using System.Collections;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] GameObject _deathPrefab;
    [SerializeField] GameObject _deathCamera;
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] SpriteRenderer _topSprite;

    [SerializeField] SpriteRenderer _bottomSprite;

    [SerializeField] Animation _ownAnimation;

    [SerializeField] GameObject _deathTextGameObject;

    [SerializeField] float _deathTextTargetScale = 6;
    [SerializeField] float _scalingPerTick = 0.05f;



    public bool TESTINGTRIGGER = false;

    void Update()
    {
        if (TESTINGTRIGGER)
        {
            TESTINGTRIGGER = false;
            SetupDeathAnimation(null, Vector3.one);
        }

    }

    public void SetupDeathAnimation(MaskData maskData, Vector3 lookDirection)
    {
        _deathPrefab.SetActive(true);
        _deathCamera.SetActive(true);

        SwapOutSprites(maskData, lookDirection);
        StartCoroutine(StartDeathAnimation());
    }


    IEnumerator StartDeathAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        StartParticles();
        yield return new WaitForSeconds(0.3f);
        StartAnimation();
        yield return new WaitForSeconds(0.3f);
        ShowDeathText();
    }

    void SwapOutSprites(MaskData maskData, Vector3 lookDirection)
    {
        if (maskData != null)
            _topSprite.sprite = maskData.maskSprite;
        _topSprite.transform.localScale = lookDirection;

        if (maskData != null)
            _bottomSprite.sprite = maskData.maskSprite;
        _bottomSprite.transform.localScale = lookDirection;
    }

    void StartParticles()
    {
        _particleSystem.gameObject.SetActive(true);
    }

    void StartAnimation()
    {
        _ownAnimation.Play("DeathAnimation");
    }

    void ShowDeathText()
    {
        _deathTextGameObject.SetActive(true);
        StartCoroutine(ScaleUpDeathText());
    }

    IEnumerator ScaleUpDeathText()
    {
        while (_deathTextGameObject.transform.localScale.x < _deathTextTargetScale)
        {
            yield return new WaitForSeconds(0.02f);
            _deathTextGameObject.transform.localScale = new Vector3(_deathTextGameObject.transform.localScale.x + _scalingPerTick,
            _deathTextGameObject.transform.localScale.y + _scalingPerTick,
            _deathTextGameObject.transform.localScale.z + _scalingPerTick);
        }
    }


}
