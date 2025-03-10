using DuloGames.UI.Tweens;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DuloGames.UI
{
    /// <summary>
    /// Loading Overlay
    /// This script requires to be attached to a canvas.
    /// The canvas must have the highest sorting order.
    /// The canvas must have a GraphicRaycaster.
    /// The visibility of the overlay is controlled by a CanvasGroup alpha.
    /// </summary>
    [RequireComponent(typeof(Canvas)), RequireComponent(typeof(GraphicRaycaster)), AddComponentMenu("UI/Loading Overlay", 58)]
    public class UILoadingOverlay : MonoBehaviour
    {
        [SerializeField] private UIProgressBar m_ProgressBar;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Header("Transition")]
        [SerializeField] private TweenEasing m_TransitionEasing = TweenEasing.InOutQuint;
        [SerializeField] private float m_TransitionDuration = 0.4f;

        private bool m_Showing = false;
        private string m_LoadSceneName = null;

        // Tween controls
        [System.NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

        // Called by Unity prior to deserialization, 
        // should not be called by users
        protected UILoadingOverlay()
        {
            if (this.m_FloatTweenRunner == null)
            {
                this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
            }

            this.m_FloatTweenRunner.Init(this);
        }

        protected void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            // Make sure it's top most ordering number
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            Canvas currentCanvas = this.gameObject.GetComponent<Canvas>();

            foreach (Canvas canvas in canvases)
            {
                // Make sure it's not our canvas1
                if (!canvas.Equals(currentCanvas))
                {
                    if (canvas.sortingOrder > currentCanvas.sortingOrder)
                    {
                        currentCanvas.sortingOrder = canvas.sortingOrder + 1;
                    }
                }
            }

            // Update the progress bar
            if (this.m_ProgressBar != null)
            {
                this.m_ProgressBar.fillAmount = 0f;
            }

            // Update the canvas group
            if (this.m_CanvasGroup != null)
            {
                this.m_CanvasGroup.alpha = 0f;
            }
        }

        protected void OnEnable()
        {
            SceneManager.sceneLoaded += this.OnSceneFinishedLoading;
        }

        protected void OnDisable()
        {
            SceneManager.sceneLoaded -= this.OnSceneFinishedLoading;
        }

        /// <summary>
        /// Shows the loading overlay and loads the scene.
        /// </summary>
        /// <param name="sceneName">The scene name.</param>
        public void LoadScene(int a)
        {

        }

        public void LoadSceneAsync(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
            this.ShowSceneLoadOverlay(sceneName);
        }

        public void ShowSceneLoadOverlay(string sceneName)
        {
            print("Changing scene to " + sceneName);
            this.m_LoadSceneName = sceneName;
            this.m_Showing = true;

            if (this.m_ProgressBar != null)
            {
                this.m_ProgressBar.fillAmount = 0f;
            }

            // Update the canvas group
            if (this.m_CanvasGroup != null)
            {
                this.m_CanvasGroup.alpha = 0f;
            }

            this.StartAlphaTween(1f, this.m_TransitionDuration, true);
        }

        /// <summary>
        /// Starts alpha tween.
        /// </summary>
        /// <param name="targetAlpha">Target alpha.</param>
        /// <param name="duration">Duration.</param>
        /// <param name="ignoreTimeScale">If set to <c>true</c> ignore time scale.</param>
        public void StartAlphaTween(float targetAlpha, float duration, bool ignoreTimeScale)
        {
            if (this.m_CanvasGroup == null)
            {
                return;
            }

            FloatTween floatTween = new FloatTween { duration = duration, startFloat = this.m_CanvasGroup.alpha, targetFloat = targetAlpha };
            floatTween.AddOnChangedCallback(this.SetCanvasAlpha);
            floatTween.AddOnFinishCallback(this.OnTweenFinished);
            floatTween.ignoreTimeScale = ignoreTimeScale;
            floatTween.easing = this.m_TransitionEasing;
            this.m_FloatTweenRunner.StartTween(floatTween);
        }

        /// <summary>
        /// Sets the canvas alpha.
        /// </summary>
        /// <param name="alpha">Alpha.</param>
        protected void SetCanvasAlpha(float alpha)
        {
            if (this.m_CanvasGroup == null)
            {
                return;
            }

            // Set the alpha
            this.m_CanvasGroup.alpha = alpha;
        }

        /// <summary>
        /// Raises the list tween finished event.
        /// </summary>
        protected void OnTweenFinished()
        {
            // When the loading overlay is shown
            if (this.m_Showing)
            {
                this.m_Showing = false;
            }
            else
            {
                // Destroy the loading overlay
                Destroy(this.gameObject);
            }
        }

        private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != this.m_LoadSceneName)
            {
                return;
            }

            // Hide the loading overlay
            this.m_Showing = false;
            this.StartAlphaTween(0f, this.m_TransitionDuration, true);
        }
    }
}