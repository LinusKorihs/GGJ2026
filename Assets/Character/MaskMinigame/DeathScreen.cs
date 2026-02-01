using System;
using System.Collections;
using TMPro;
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

    [SerializeField] string _playerDeathText = "You died!!";
    [SerializeField] string _correctKillText = "Correct Target\nYou Win!!";
    [SerializeField] string _wrongKillText = "Wrong Target\nYou Loose!!";
    [SerializeField] AudioSource __ownAudioSource;
    [SerializeField] AudioClip _deathSound;


    public enum DeathVersion { Player, WrongKill, CorrectKill }

    DeathVersion _deathVersion;

    bool _isInDeathScene = false;



    public bool TESTINGTRIGGER = false;

    void Update()
    {
        if (TESTINGTRIGGER)
        {
            TESTINGTRIGGER = false;
            SetupDeathAnimation(null, CharacterControllerScript.CharacterDirection.East, DeathVersion.Player);
        }

    }

    public void SetupDeathAnimation(MaskData maskData, CharacterControllerScript.CharacterDirection lookDirection, DeathVersion deathVersion)
    {
        if (_isInDeathScene)
            return;


        _isInDeathScene = true;
        _deathVersion = deathVersion;
        _deathPrefab.SetActive(true);
        _deathCamera.SetActive(true);

        SwapOutSprites(maskData, lookDirection);
        StartCoroutine(StartDeathAnimation());
    }


    IEnumerator StartDeathAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        StartParticles();
        PlayDeathSound();
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
        string deathText;
        switch (_deathVersion)
        {
            case DeathVersion.Player:
                deathText = _playerDeathText;
                GameManager.Instance.IsGameWon = false;
                break;

            case DeathVersion.WrongKill:
                deathText = _wrongKillText;
                GameManager.Instance.IsGameWon = false;
                break;

            case DeathVersion.CorrectKill:
                deathText = _correctKillText;
                GameManager.Instance.IsGameWon = true;
                break;

            default:
                deathText = _playerDeathText;
                GameManager.Instance.IsGameWon = false;
                break;
        }


        _deathTextGameObject.SetActive(true);
        _deathTextGameObject.GetComponent<TextMeshProUGUI>().text = deathText;
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

    void PlayDeathSound()
    {
        __ownAudioSource.PlayOneShot(_deathSound);
    }


}
