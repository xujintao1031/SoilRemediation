
using System;
using System.Collections;
using System.Globalization;
using DG.Tweening;
using GasChromatography.UI;
using GasChromatography.UI.MainPanel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GasChromatography.Manager
{
    public class SceneTransitionManager : Singleton<SceneTransitionManager>
    {
        [Header("Elements")]
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject logo;
        [SerializeField] private GameObject progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Image blackScreen; // 全屏黑色遮罩
        [SerializeField] private LoadingText loadingText;
        [Header("Timings")]
        [SerializeField] private float bgFadeDuration = 0.25f;

        [SerializeField] private float logoDuration = 0.6f;
        [SerializeField] private float logoPunchDuration = 0.35f;
        [SerializeField] private float progressFadeDuration = 0.4f;
        [SerializeField] private float logoExitDuration = 0.25f; // 场景切换时 logo 出场时长
        [SerializeField] private float minLoadDisplay = 1.2f; // 最少显示加载 UI 的秒数
        [SerializeField] private float progressSmoothSpeed = 4f; // 进度平滑速度（值越大越快贴近真实进度）

        [Header("Logo options")]
        [SerializeField]
        private float logoStartScale = 0.9f;

        [SerializeField] private float logoPeakScale = 1.05f;
        [SerializeField] private Vector3 logoStartRotation = new(0, 0, -8f);
        [SerializeField] private float logoExitScaleMultiplier = 0.9f;

        [Header("Behavior")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool fadeBackground; // 新：是否对背景做淡入/淡出

        private CanvasGroup _bgCg;
        private float _bgOriginalAlpha = 1f;
        private CanvasGroup _logoCg;
        private Quaternion _logoOriginalRotation;

        // cached originals
        private Vector3 _logoOriginalScale;

        private RectTransform _logoRt;
        private CanvasGroup _progressCg;
        private float _progressOriginalAlpha = 1f;

        private Slider _progressSlider; // 若 progressBar 包含 Slider 则更新它

        private Sequence _seq;

        // 新：加载中标识，避免重复/冲突的动画
        private bool _isLoading = false;

        protected override void Awake()
        {
            base.Awake();
            if (background != null) _bgCg = EnsureCanvasGroup(background);
            if (logo != null)
            {
                _logoCg = EnsureCanvasGroup(logo);
                _logoRt = logo.GetComponent<RectTransform>() ?? logo.AddComponent<RectTransform>();
            }

            if (progressBar != null)
            {
                _progressCg = EnsureCanvasGroup(progressBar);
                _progressSlider = progressBar.GetComponentInChildren<Slider>();
            }

            if (_logoRt != null)
            {
                _logoOriginalScale = _logoRt.localScale;
                _logoOriginalRotation = _logoRt.localRotation;
            }

            if (_bgCg != null) _bgOriginalAlpha = _bgCg.alpha;
            if (_progressCg != null) _progressOriginalAlpha = _progressCg.alpha;

            DOTween.SetTweensCapacity(50, 10);
        }

        private void Update()
        {
            // for testing
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     StartCoroutine(DoCloseAndLoad("Demo"));
            // }
        }

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        private void OnDisable()
        {
            _seq?.Kill();
            DOTween.Kill(_logoRt);
            DOTween.Kill(_logoCg);
            DOTween.Kill(_bgCg);
            DOTween.Kill(_progressCg);
            DOTween.Kill(_progressSlider); // 确保清理 slider 上的 tween
        }

        // 开场动画：涉及 logo，背景按 fadeBackground 控制是否淡入
        public void Play()
        {
            
            _seq?.Kill();
            DOTween.Kill(_logoRt);
            DOTween.Kill(_logoCg);
            DOTween.Kill(_progressCg);
            DOTween.Kill(_progressSlider);

            PrepareInitialState();

            _seq = DOTween.Sequence();

            _seq.AppendCallback(() =>
            {
                if (_logoCg != null) _logoCg.alpha = 0f;
                if (_logoRt != null)
                {
                    _logoRt.localScale = Vector3.one * logoStartScale;
                    _logoRt.localRotation = Quaternion.Euler(logoStartRotation);
                }

                if (fadeBackground && _bgCg != null)
                    // 确保背景从 0 开始淡入
                    _bgCg.alpha = 0f;
            });

            if (fadeBackground && _bgCg != null)
            {
                _seq.Append(_bgCg.GetComponent<RectTransform>().DOScale(1.3f, bgFadeDuration * 1.5f)
                    .SetEase(Ease.OutCubic));
                _seq.Join(_bgCg.DOFade(_bgOriginalAlpha, bgFadeDuration).SetEase(Ease.Linear));
            }

            if (_logoRt != null)
            {
                _seq.Append(_logoRt.DOScale(Vector3.one * logoPeakScale, logoDuration).SetEase(Ease.OutBack));
                _seq.Join(_logoRt.DORotateQuaternion(_logoOriginalRotation, logoDuration).SetEase(Ease.OutBack));
            }

            if (_logoCg != null) _seq.Join(_logoCg.DOFade(1f, logoDuration * 0.5f));

            if (_logoRt != null)
                _seq.Append(_logoRt.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), logoPunchDuration, 6, 0.4f));

            _seq.OnComplete(() =>
            {
                if (_logoRt != null)
                {
                    _logoRt.localScale = _logoOriginalScale;
                    _logoRt.localRotation = _logoOriginalRotation;
                }

                if (_logoCg != null) _logoCg.alpha = 1f;
                blackScreen.enabled = false;
                ShowProgressBar();
            });

            _seq.Play();
        }

        public void LoadScene(string sceneName, Action onComplete = null)
        {
            StartCoroutine(DoCloseAndLoad(sceneName, onComplete));
        }

        
        private IEnumerator DoCloseAndLoad(string sceneName, Action onComplete = null)
        {
            // 如果目标场景和当前场景相同，直接返回
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                Debug.Log("SceneTransition: Target scene is the same as current scene, skipping load.");
                yield break;
            }

            _isLoading = true;
            blackScreen.gameObject.SetActive(true);
            // 1) 保证 background / logo / progressBar 都处于可见并重置状态
            if (background != null) background.SetActive(true);
            if (logo != null) logo.SetActive(true);
            if (progressBar != null) progressBar.SetActive(true);
            if (_bgCg != null) _bgCg.alpha = _bgOriginalAlpha;
            if (_logoCg != null) _logoCg.alpha = 1f;
            if (_logoRt != null) _logoRt.localScale = _logoOriginalScale;
            if (_progressCg != null) _progressCg.alpha = 0f; // 先从透明开始淡入
            if (_progressSlider != null) _progressSlider.value = 0f;
            if (progressText != null) progressText.text = "0";

            // 停止任何 slider 上的 tween，避免与下面的平滑逻辑冲突
            DOTween.Kill(_progressSlider);

            // 2) 轻微过渡动画（保持元素可见）
            if (_logoRt != null || _logoCg != null || (fadeBackground && _bgCg != null))
            {
                var seqOut = DOTween.Sequence();
                if (_logoRt != null)
                    seqOut.Join(_logoRt.DOScale(_logoOriginalScale * logoExitScaleMultiplier, logoExitDuration)
                        .SetEase(Ease.InCubic));
                if (_logoCg != null)
                    seqOut.Join(_logoCg.DOFade(1f, logoExitDuration).SetEase(Ease.Linear));
                if (fadeBackground && _bgCg != null)
                {
                    var bgRt = _bgCg.GetComponent<RectTransform>();
                    if (bgRt != null) seqOut.Join(bgRt.DOScale(1.02f, logoExitDuration).SetEase(Ease.InCubic));
                }

                yield return seqOut.WaitForCompletion();
                if (_logoRt != null) _logoRt.localScale = _logoOriginalScale;
            }

            // 3) 淡入进度条（等待淡入完成再开始加载）
            if (_progressCg != null)
                yield return _progressCg.DOFade(_progressOriginalAlpha, progressFadeDuration).SetEase(Ease.Linear)
                    .WaitForCompletion();
            else
                yield return null;
            loadingText.IsRunning = true;
            // 4) 异步加载并用独立 displayedProgress 平滑更新 UI，保证最小显示时长
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            var startUnscaled = Time.unscaledTime;

            float displayedProgress = 0f;
            const float completionThreshold = 0.995f; // 认为 UI 达到 100% 的阈值
            float lastDelta = 0f;

            while (true)
            {
                // Unity async progress 在 0.0 ~ 0.9，激活前不会到 1.0
                float realTarget = Mathf.Clamp01(op.progress / 0.9f);

                // 平滑系数（用 unscaledDeltaTime 保证在暂停时也工作）
                float dt = Time.unscaledDeltaTime;
                lastDelta = dt;
                float smooth = 1f - Mathf.Exp(-progressSmoothSpeed * dt);

                // 当 op 到达 0.9 时，我们把真实目标设为 1（等待 displayedProgress 平滑到 1）
                if (op.progress >= 0.9f)
                    realTarget = 1f;

                // 更新 displayedProgress 并应用到 UI（不再使用 DOTween 的 DOValue）
                displayedProgress = Mathf.Lerp(displayedProgress, realTarget, smooth);

                if (_progressSlider != null)
                    _progressSlider.value = displayedProgress;

                if (progressText != null)
                    progressText.text = Mathf.RoundToInt(displayedProgress * 100f)
                        .ToString(CultureInfo.InvariantCulture);

                // 如果真实加载已经完成到可激活，并且 UI 已经平滑到接近 100%，并满足最小显示时间，则激活场景
                if (op.progress >= 0.9f && displayedProgress >= completionThreshold)
                {
                    var elapsed = Time.unscaledTime - startUnscaled;
                    if (elapsed < minLoadDisplay)
                    {
                        yield return new WaitForSecondsRealtime(minLoadDisplay - elapsed);
                    }

                    // 小延迟让 UI 显示 100%
                    yield return new WaitForSecondsRealtime(0.15f);

                    op.allowSceneActivation = true;
                    onComplete?.Invoke();

                    // 关闭 UI
                    CloseCanvasGroup(_bgCg);
                    CloseCanvasGroup(_logoCg);
                    CloseCanvasGroup(_progressCg);
                    loadingText.IsRunning = false;
                    _isLoading = false;
                    yield break;
                }

                // 如果异步加载已经 complete（防御性处理）
                if (op.isDone)
                {
                    _isLoading = false;
                    yield break;
                }

                yield return null;
            }
        }



        private void ShowProgressBar()
        {
            if (progressBar == null) return;

            // 加载中不由此处触发进度动画
            if (_isLoading) return;

            DOTween.Kill(_progressCg);
            DOTween.Kill(_progressSlider);

            progressBar.SetActive(true);
           
            if (_progressCg != null)
            {
                _progressCg.alpha = 0f;
                _progressCg.DOFade(_progressOriginalAlpha, progressFadeDuration).SetEase(Ease.Linear).OnComplete(() =>
                {
                    loadingText.IsRunning = true;
                    _progressSlider.DOValue(1f, 3f).OnUpdate(() =>
                    {
                       
                        var percent = Mathf.RoundToInt(_progressSlider.value * 100f);
                        progressText.text = percent.ToString(CultureInfo.InvariantCulture);
                    }).OnComplete(() =>
                    {
                        CloseCanvasGroup(_bgCg);
                        CloseCanvasGroup(_logoCg);
                        CloseCanvasGroup(_progressCg);
                        loadingText.IsRunning = false;
                    });
                });
            }
        }

        private void PrepareInitialState()
        {
            DOTween.Kill(_logoRt);
            DOTween.Kill(_logoCg);
            DOTween.Kill(_progressCg);
            DOTween.Kill(_bgCg);
            DOTween.Kill(_progressSlider);

            // 背景是否淡入由 fadeBackground 控制
            if (fadeBackground && _bgCg != null) _bgCg.alpha = 0f;

            // logo 初始隐藏与缩放（使用缓存的原始 scale）
            if (_logoCg != null) _logoCg.alpha = 0f;
            if (_logoRt != null)
            {
                _logoRt.localScale = _logoOriginalScale * logoStartScale;
                _logoRt.localRotation = Quaternion.Euler(logoStartRotation);
            }

            if (progressBar != null)
            {
                progressBar.SetActive(false);
                if (_progressCg != null) _progressCg.alpha = 0f;
                if (_progressSlider != null) _progressSlider.value = 0f;
            }
        }

        private void CloseCanvasGroup(CanvasGroup canvasGroup)
        {
            canvasGroup.DOFade(0f, progressFadeDuration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
        }

        private void Complete()
        {

        }

        private CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
