using System.Collections;
using GasChromatography.UI;
using UnityEngine;

namespace GasChromatography.Manager
{
    public class TipManager : Singleton<TipManager>
    {
        [SerializeField] private CanvasGroup tipCanvasGroup;
        [SerializeField] private Transform tipFrame;
        [SerializeField] private TypeWriterTip tipText;

        [Header("动画设置")] [SerializeField] private float showDuration = 0.25f;

        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private float stayDuration = 2f;
        [SerializeField] private float moveDistance = 60f; // 从屏幕外往上移动的距离

        private Coroutine _currentRoutine;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public void ShowTip(string tip, bool stay = false)
        {
            // 确保 RectTransform 在屏幕底部左侧
            if (tipFrame is RectTransform rt)
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
            }

            // 如果当前已经可见，则跳过面板的出现动画，直接显示文本
            var alreadyVisible = tipCanvasGroup != null && tipCanvasGroup.alpha > 0f;

            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(ShowSequence(tip, stay, alreadyVisible));
        }

        public void HideTip()
        {
            if (_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(HideSequence());
        }

        private IEnumerator ShowSequence(string tip, bool isStay = false, bool skipAnim = false)
        {
            tipText.targetText.text = string.Empty;
            if (tipCanvasGroup == null || tipFrame == null) yield break;

            var rt = (RectTransform)tipFrame;
            // 从屏幕左侧往右移动
            var startPos = new Vector2(-moveDistance, 0f);
            var endPos = new Vector2(10f, 0f); // 轻微右移的最终位置

            if (skipAnim)
            {
                // 直接确保可见并把位置设为最终位置
                tipCanvasGroup.alpha = 1f;
                tipCanvasGroup.blocksRaycasts = true;
                tipCanvasGroup.interactable = true;
                rt.anchoredPosition = endPos;
            }
            else
            {
                tipCanvasGroup.alpha = 0f;
                tipCanvasGroup.blocksRaycasts = true;
                tipCanvasGroup.interactable = true;
                rt.anchoredPosition = startPos;

                var t = 0f;
                while (t < showDuration)
                {
                    t += Time.unscaledDeltaTime;
                    var p = Mathf.Clamp01(t / showDuration);
                    tipCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, p);
                    rt.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                    yield return null;
                }

                tipCanvasGroup.alpha = 1f;
                rt.anchoredPosition = endPos;
            }

            var isShowComplete = false;
            tipText.ShowTip(tip, () => { isShowComplete = true; });

            while (!isShowComplete) yield return null;

            if (isStay) yield break;

            var elapsed = 0f;
            while (elapsed < stayDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _currentRoutine = StartCoroutine(HideSequence());
        }


        private IEnumerator HideSequence()
        {
            if (tipCanvasGroup == null || tipFrame == null) yield break;

            var rt = (RectTransform)tipFrame;
            var startPos = rt.anchoredPosition;
            // 隐藏时向左移动到屏幕外
            var endPos = new Vector2(-moveDistance, 0f);

            var t = 0f;
            while (t < hideDuration)
            {
                t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(t / hideDuration);
                tipCanvasGroup.alpha = Mathf.SmoothStep(1f, 0f, p);
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                yield return null;
            }

            tipCanvasGroup.alpha = 0f;
            rt.anchoredPosition = endPos;
            tipCanvasGroup.blocksRaycasts = false;
            tipCanvasGroup.interactable = false;

            _currentRoutine = null;
        }
    }
}