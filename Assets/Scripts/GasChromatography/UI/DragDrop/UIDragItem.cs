using UnityEngine;
using UnityEngine.EventSystems;

namespace GasChromatography.UI.DragDrop
{
    public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CanvasGroup _canvasGroup;

        private bool _isComplete;

        private Vector3 _originalPosition;
        private RectTransform _rectTransform;

        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                _isComplete = value;
                if (!_isComplete) ReturnToOriginalParent();
            }
        }

        public Transform OriginalParent { get; private set; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsComplete) return;
            OriginalParent = transform.parent;
            _originalPosition = transform.position;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsComplete) return;
            var canvas = GetComponentInParent<Canvas>();
            _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            if (!IsComplete) ReturnToOriginalParent();
        }

        public void SetNewParent(Transform newParent)
        {
            OriginalParent = newParent;
            transform.SetParent(newParent);
            _rectTransform.anchoredPosition = Vector2.zero;
        }

        private void ReturnToOriginalParent()
        {
            transform.SetParent(OriginalParent);
            _rectTransform.position = _originalPosition;
        }
    }
}