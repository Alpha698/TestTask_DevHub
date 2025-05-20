using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private CannonController cannonController;

    [Header("UI")]
    [SerializeField] private Button fireButton;
    [SerializeField] private TextMeshProUGUI fireButtonText;
    [SerializeField] private Slider reloadSlider;
    [Header("Lose UI")]
    [SerializeField] private GameObject losePopup;
    [SerializeField] private GameObject losePopupCointainer;
    [SerializeField] private Button restartButton;
    [Header("Start UI")]
    [SerializeField] private GameObject startPopup;
    [SerializeField] private Button startButton;

    [Header("Settings")]
    [SerializeField] private float reloadTime = 3f;

    private bool isGameOver = false;
    private bool isReloading = false;
    private bool isGameStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        startButton.onClick.AddListener(StartGame);
        fireButton.onClick.AddListener(TryFire);
        restartButton.onClick.AddListener(RestartGame);
    }

    private void Start()
    {
        ShowPopup(startPopup);
    }

    private void OnEnable()
    {
        Enemy.EnemyAttack += OnPlayerLose;
    }

    private void OnDisable()
    {
        Enemy.EnemyAttack -= OnPlayerLose;
    }

    public void TryFire()
    {
        if (isGameOver || isReloading) return;

        cannonController.Shoot();
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        fireButton.interactable = false;

        // Make the button inactive while reloading
        Color fadedColor = fireButtonText.color;
        fadedColor.a = 0.3f;
        fireButtonText.color = fadedColor;

        float timer = 0f;
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            reloadSlider.value = timer / reloadTime;
            yield return null;
        }

        fireButton.interactable = true;
        fireButtonText.color = new Color(fadedColor.r, fadedColor.g, fadedColor.b, 1f);
        isReloading = false;
    }

    private void OnPlayerLose()
    {
        if (isGameOver) return;

        isGameOver = true;
        cannonController.HandleLose();

        fireButton.interactable = false;
        Color fadedColor = fireButtonText.color;
        fadedColor.a = 0.3f;
        fireButtonText.color = fadedColor;
        reloadSlider.value = 0f;

        // Show lose popup
        losePopup.SetActive(true);
        ShowPopup(losePopupCointainer);
    }

    private void StartGame()
    {
        isGameStarted = true;
        HidePopup(startPopup);

        fireButton.gameObject.SetActive(true);
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void ShowPopup(GameObject popup)
    {
        popup.SetActive(true);

        popup.transform.localScale = Vector3.zero;
        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        popup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        cg.DOFade(1f, 0.5f).OnComplete(() => {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        });
    }

    private void HidePopup(GameObject popup)
    {
        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        popup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        cg.DOFade(0f, 0.3f).OnComplete(() => popup.SetActive(false));
    }

    public bool IsGameStarted() => isGameStarted;
    public bool IsGameOver() => isGameOver;
    public bool IsReloading() => isReloading;
}
