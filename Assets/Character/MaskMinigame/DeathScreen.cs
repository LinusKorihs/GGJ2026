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
            SetupDeathAnimation(null, CharacterControllerScript.CharacterDirection.East);
        }

    }

    public void SetupDeathAnimation(MaskData maskData, CharacterControllerScript.CharacterDirection lookDirection)
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

    void SwapOutSprites(MaskData maskData, CharacterControllerScript.CharacterDirection lookDirection)
    {
        //early out if we dont have maskData
        if (maskData == null)
            return;

        Sprite selectedSprite;
        Vector3 lookVector = Vector3.one;

        switch (lookDirection)
        {
            case CharacterControllerScript.CharacterDirection.North:
                selectedSprite = maskData.MaskSprites.NSprite;
                break;

            case CharacterControllerScript.CharacterDirection.South:
                selectedSprite = maskData.MaskSprites.SSprite;
                break;

            case CharacterControllerScript.CharacterDirection.West:
                selectedSprite = maskData.MaskSprites.WSprite;
                //only overwrite with flipped for West Sprite
                lookVector = new Vector3(-1, 1, 1);
                break;

            case CharacterControllerScript.CharacterDirection.East:
                selectedSprite = maskData.MaskSprites.ESprite;
                break;

            default:
                selectedSprite = maskData.MaskSprites.ESprite;
                break;
        }


        _topSprite.sprite = selectedSprite;
        _bottomSprite.sprite = selectedSprite;

        _topSprite.transform.localScale = lookVector;
        _bottomSprite.transform.localScale = lookVector;
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
        GameManager.Instance.IsGameOver = true;
    }


}
