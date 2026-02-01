using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class IntroSlideshow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image introImage;

    [Header("Resources paths for sprites")]
    [SerializeField] private string firstSpritePath = "StartPics/Mission";
    [SerializeField] private string secondSpritePath = "StartPics/Controlls";

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Input System")]
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private string actionMapName = "UI";
    [SerializeField] private string submitActionName = "Submit";

    private InputAction _submitAction;

    private Sprite _currentSprite;
    private int _step = 0;

    private void Awake()
    {
        _submitAction = actions.FindActionMap(actionMapName).FindAction(submitActionName);
        _submitAction.performed += OnSubmit;
    }

    private void OnEnable() => _submitAction.Enable();
    private void OnDisable() => _submitAction.Disable();

    private void Start()
    {
        LoadAndShow(firstSpritePath);
        _step = 1;
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (_step == 1)
        {
            UnloadCurrent();
            LoadAndShow(secondSpritePath);
            _step = 2;
        }
        else
        {
            _submitAction.Disable();
            Time.timeScale = 1f;
            Destroy(transform.root.gameObject);   // oder Destroy(gameObject), je nachdem wo das Script hängt
            SceneManager.LoadScene(gameSceneName);
        }
    }

    private void LoadAndShow(string resourcesPath)
    {
        _currentSprite = Resources.Load<Sprite>(resourcesPath);
        if (_currentSprite == null)
        {
            Debug.LogError($"[IntroSlideshow] Sprite not found at Resources/{resourcesPath}. " +
                           $"Make sure it's under Assets/Resources/{resourcesPath}.png and set to Sprite.");
            return;
        }

        introImage.sprite = _currentSprite;
        introImage.enabled = true;
        introImage.preserveAspect = true;
    }

    private void UnloadCurrent()
    {
        if (_currentSprite == null) return;

        introImage.sprite = null;
        Resources.UnloadAsset(_currentSprite);
        _currentSprite = null;
        Resources.UnloadUnusedAssets();
    }
}
