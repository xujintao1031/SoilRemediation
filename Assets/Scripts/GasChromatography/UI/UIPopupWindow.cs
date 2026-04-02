using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GasChromatography.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPopupWindow : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private bool destroyOnHide = false;

        [Header("Animation Settings")] [SerializeField]
        private float animationDuration = 0.3f;

        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;
        [SerializeField] protected Button closeButton;
        [SerializeField] private Image blockerImage;

        private CanvasGroup _canvasGroup;
        private Vector3 _initialScale;
        public Action OnShowComplete;
        public Action OnHideComplete;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _initialScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        }

        protected virtual void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        protected virtual void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        protected virtual void Close()
        {
            Hide();
        }

        protected virtual void SetPopupData(object data)
        {
            // 子类重写以设置数据
        }

        public virtual void Show(object content = null)
        {
            SetPopupData(content);
            gameObject.SetActive(true);

            transform.DOKill();
            _canvasGroup.DOKill();

            transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;
            transform.DOScale(_initialScale, animationDuration).SetEase(showEase);
            _canvasGroup.DOFade(1f, animationDuration).OnComplete(() =>
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
                if(blockerImage == null) blockerImage = transform.parent.GetComponent<Image>();
                if(blockerImage != null) blockerImage.enabled = true;
                OnShowComplete?.Invoke();
            });
        }

        public void Hide()
        {
            transform.DOKill();
            _canvasGroup.DOKill();

            transform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase);
            _canvasGroup.DOFade(0f, animationDuration).OnComplete(() =>
            {
                if (destroyOnHide)
                {
                    if(blockerImage != null) blockerImage.enabled = false;
                    Destroy(gameObject);
                }
                else
                {
                    _canvasGroup.blocksRaycasts = false;
                    _canvasGroup.interactable = false;
                    var isNeedBlocker = false;
                    if(blockerImage != null)
                    {
                        for (var i = 0; i < transform.parent.childCount; i++)
                        {
                            if(transform.parent.GetChild(i) == transform) continue;
                            if (transform.parent.GetChild(i).gameObject.activeInHierarchy)
                            {
                                isNeedBlocker = true;
                                break;
                            }
                        }
                        blockerImage.enabled = isNeedBlocker;
                    }

                    gameObject.SetActive(false);
                }

                OnHideComplete?.Invoke();
            });
        }

        private void OnDestroy()
        {
            OnHideComplete = null;
            OnShowComplete = null;
        }

        [ContextMenu("Test Show")]
        public void TestShow()
        {
            Show();
        }

        [ContextMenu("Test Hide")]
        public void TestHide()
        {
            Hide();
        }
    }
}