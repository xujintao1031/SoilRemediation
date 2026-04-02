using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GasChromatography.UI.MainPanel
{
    public class LoadingText : MonoBehaviour
    {
        [SerializeField] private Text uiText;
        [Header("设置")]
        [SerializeField] private string prefix = "加载中";
        [SerializeField] private int maxDots = 3;
        [SerializeField] private float dotInterval = 0.25f; // 秒
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool includeSpaceBeforeDots = true;

        private int _currentDots = 0;
        private float _timer = 0f;
        private StringBuilder _sb = new StringBuilder(16);

        public bool IsRunning { get; set; } = false;
        

        private void OnEnable()
        {
            if (playOnEnable) StartAnimating();
            RefreshText(); // 保证初始显示
        }

        private void OnDisable()
        {
            StopAnimating();
        }

        private void Update()
        {
            if (!IsRunning) return;
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt;
            if (_timer >= dotInterval)
            {
                // 支持累积多个间隔（防止跳帧）
                int steps = Mathf.FloorToInt(_timer / dotInterval);
                _timer -= steps * dotInterval;
                _currentDots = (_currentDots + steps) % (maxDots + 1);
                RefreshText();
            }
        }

        private void RefreshText()
        {
            _sb.Clear();
            _sb.Append(prefix);
            if (includeSpaceBeforeDots) _sb.Append(' ');
            for (int i = 0; i < _currentDots; i++) _sb.Append('.');
            if (uiText != null) uiText.text = _sb.ToString();
        }

        public void StartAnimating()
        {
            IsRunning = true;
            _timer = 0f;
            _currentDots = 0;
            RefreshText();
        }

        public void StopAnimating(bool keepText = true)
        {
            IsRunning = false;
            _timer = 0f;
            _currentDots = keepText ? _currentDots : 0;
            RefreshText();
        }

        // 可在代码中动态设置前缀
        public void SetPrefix(string newPrefix)
        {
            prefix = newPrefix ?? string.Empty;
            RefreshText();
        }

    }
}
