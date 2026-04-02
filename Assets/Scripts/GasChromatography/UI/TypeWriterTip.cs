using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GasChromatography.UI
{
    public class TypeWriterTip : MonoBehaviour
    {
        [Tooltip("UI Text 目标")]
        public Text targetText;

        [Tooltip("每个字符间隔（秒）")]
        public float charInterval = 0.1f;

        [Tooltip("全部显示完成时触发（可在 Inspector 绑定）")]
        public UnityEvent onComplete;

        // 代码订阅用
        public event Action OnComplete;

        private Coroutine _typing;
        private string _fullText;
        private bool _completedInvoked;

        void Awake()
        {
            if (targetText == null)
            {
                Debug.LogWarning("[TypewriterTip] targetText 未设置。");
            }
        }
        

        // 重载：带回调
        public void ShowTip(string text, Action finishedCallback = null, float? interval = null, Color? color = null, bool isFirstLineIndent = false)
        {
            if (targetText == null) return;
            if(targetText.text == text) return;
            if (interval.HasValue) charInterval = interval.Value;
            if (color.HasValue) targetText.color = color.Value;

            StopTyping();
            var indent = isFirstLineIndent ? "\u3000\u3000" : "";
            _fullText = indent + text ?? "";
            _completedInvoked = false;

            if (finishedCallback != null)
            {
                OnComplete += finishedCallback;
            }
            

            _typing = StartCoroutine(TypeTextCoroutine());
        }

        // 立即显示全部（也会触发完成回调）
        public void SkipInstant()
        {
            if (targetText == null) return;
            if (_typing != null)
            {
                StopCoroutine(_typing);
                _typing = null;
            }
            targetText.text = _fullText ?? "";
            InvokeComplete();
        }

        // 停止并清空（不触发完成回调）
        public void Stop()
        {
            StopTyping();
            if (targetText != null) targetText.text = "";
        }

        private void StopTyping()
        {
            if (_typing != null)
            {
                StopCoroutine(_typing);
                _typing = null;
            }
        }

        private IEnumerator TypeTextCoroutine()
        {
            targetText.text = "";
            if (string.IsNullOrEmpty(_fullText))
            {
                _typing = null;
                InvokeComplete();
                yield break;
            }

            for (int i = 0; i < _fullText.Length; i++)
            {
                targetText.text += _fullText[i];
                yield return new WaitForSeconds(charInterval);
            }

            _typing = null;
            InvokeComplete();
        }

        private void InvokeComplete()
        {
            if (_completedInvoked) return;
            _completedInvoked = true;

            try
            {
                onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("[TypeWriterTip] onComplete exception: " + ex);
            }

            try
            {
                OnComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("[TypeWriterTip] OnComplete exception: " + ex);
            }

            // 清理临时附加的回调（如果通过 ShowTip 传入的回调被附加到了 OnComplete）
            // 注意：这会移除所有订阅者，如果你需要保留长期订阅者，请改用不同的订阅方式。
            OnComplete = null;
        }
    }
}