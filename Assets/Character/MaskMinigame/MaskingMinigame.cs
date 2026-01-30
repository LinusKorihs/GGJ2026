using System;
using System.Collections;
using UnityEngine;

public class MaskingMinigame : MonoBehaviour
{
    [SerializeField] GameObject _minigameCamera;
    [SerializeField] GameObject _maskingMinigameObject;

    public Action OnEventFinished;

    //hardcoded offset, so the camera dont just sit on the object
    Vector3 _cameraOffset = new Vector3(0f, 0f, -10f);

    void Awake()
    {
        //emergency find, because minigame camera needs to be outside of char controller, so this connection cannot be saved in char controller prefab. 
        // And this might break for a new scene
        if (_minigameCamera == null)
            GameObject.FindGameObjectWithTag("MiniGameCamera");
    }
    public void StartStealingMinigame(GameObject target)
    {
        SetupMinigame(target);
        StartCoroutine(RunEvent());
    }


    public void SetupMinigame(GameObject target)
    {
        _minigameCamera.SetActive(true);
        _maskingMinigameObject.SetActive(true);
        _minigameCamera.transform.position = target.transform.position + _cameraOffset;
    }

    public IEnumerator RunEvent()
    {
        bool eventActive = true;
        while (eventActive)
        {

            yield return null;
        }

        OnEventFinished?.Invoke();
        EndMinigame();
    }

    public void EndMinigame()
    {
        _minigameCamera.SetActive(false);
        _maskingMinigameObject.SetActive(false);
        OnEventFinished.Invoke();
    }

}
