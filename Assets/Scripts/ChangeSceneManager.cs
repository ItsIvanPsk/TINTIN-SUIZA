using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ChangeSceneManager : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] public int ButtonScene;

    [Header("Fade")]
    [Tooltip("Si no asignas nada, se crea automáticamente en runtime.")]
    [SerializeField] private Image _fadeImage;

    [Tooltip("Duración del fade de entrada/salida en segundos.")]
    [SerializeField] private float _fadeDuration = 0.4f;

    private bool _isLoading = false;

    private void Awake()
    {
        if (_fadeImage == null)
            _fadeImage = CreateFadeCanvas();
    }

    // ── Llamada desde botones / eventos ────────────────────────────────────

    public void OnButtonPressed()
    {
        if (_isLoading) return;
        _isLoading = true;
        StartCoroutine(LoadSceneAsync(ButtonScene));
    }

    /// <summary>Permite cambiar a una escena por índice desde otros scripts.</summary>
    public void LoadScene(int sceneIndex)
    {
        if (_isLoading) return;
        _isLoading = true;
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    /// <summary>Permite cambiar a una escena por nombre desde otros scripts.</summary>
    public void LoadSceneByName(string sceneName)
    {
        if (_isLoading) return;
        _isLoading = true;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    // ── Carga asíncrona ─────────────────────────────────────────────────────

    private IEnumerator LoadSceneAsync(object sceneIdentifier)
    {
        // 1. Fade a negro
        if (_fadeImage != null)
            yield return StartCoroutine(Fade(0f, 1f));

        // 2. Iniciar carga en background (allowSceneActivation = false para
        //    que Unity no active la escena hasta que el fade haya terminado)
        AsyncOperation op = sceneIdentifier is int
            ? SceneManager.LoadSceneAsync((int)sceneIdentifier)
            : SceneManager.LoadSceneAsync((string)sceneIdentifier);

        op.allowSceneActivation = false;

        // 3. Esperar hasta que la escena esté lista (progress llega a 0.9
        //    cuando todo está cargado pero aún no activado)
        while (op.progress < 0.9f)
            yield return null;

        // 4. Activar la escena (el switch real ocurre aquí, casi instantáneo)
        op.allowSceneActivation = true;

        // 5. Esperar al primer frame de la nueva escena
        yield return null;

        // 6. Fade de vuelta a transparente en la nueva escena
        if (_fadeImage != null)
            yield return StartCoroutine(Fade(1f, 0f));

        _isLoading = false;
    }

    // ── Fade ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea en runtime un Canvas de pantalla completa con una Image negra
    /// que se usará para los fades de entrada/salida.
    /// Se marca como DontDestroyOnLoad para sobrevivir al cambio de escena
    /// y poder hacer el fade-in en la nueva escena.
    /// </summary>
    private Image CreateFadeCanvas()
    {
        // Canvas raíz
        var canvasGO = new GameObject("_FadeCanvas");
        DontDestroyOnLoad(canvasGO);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Por encima de cualquier otra UI

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Image negra a pantalla completa
        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);

        var rect = imgGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var img = imgGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f); // Empieza transparente
        img.raycastTarget = false;

        return img;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = _fadeImage.color;

        _fadeImage.gameObject.SetActive(true);
        _fadeImage.raycastTarget = true;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            _fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        _fadeImage.color = c;

        if (to <= 0f)
            _fadeImage.raycastTarget = false;
    }
}
