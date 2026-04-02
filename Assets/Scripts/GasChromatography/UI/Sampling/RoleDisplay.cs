using GasChromatography.APPUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GasChromatography.UI.Sampling
{
    public class RoleDisplay : MonoBehaviour
    {
        [SerializeField] private RawImage roleImage;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Transform characterTransform;
        [SerializeField] private float rotateSpeed = 0.5f;
        [SerializeField] private float buttonRotateSpeed = 0.5f;

        private bool _isRotatingLeft;
        private bool _isRotatingRight;

        private void Start()
        {
            if (roleImage != null)
                Utils.AddEventTriggerListener(roleImage.gameObject, EventTriggerType.Drag,
                    data => { OnDrag((PointerEventData)data); });

            if (leftButton != null)
            {
                Utils.AddEventTriggerListener(leftButton.gameObject, EventTriggerType.PointerDown,
                    _ => { _isRotatingLeft = true; });
                Utils.AddEventTriggerListener(leftButton.gameObject, EventTriggerType.PointerUp,
                    _ => { _isRotatingLeft = false; });
            }

            if (rightButton != null)
            {
                Utils.AddEventTriggerListener(rightButton.gameObject, EventTriggerType.PointerDown,
                    _ => { _isRotatingRight = true; });
                Utils.AddEventTriggerListener(rightButton.gameObject, EventTriggerType.PointerUp,
                    _ => { _isRotatingRight = false; });
            }
        }

        private void Update()
        {
            if (!characterTransform) return;

            if (_isRotatingLeft)
                characterTransform.Rotate(Vector3.up, -buttonRotateSpeed);
            else if (_isRotatingRight) characterTransform.Rotate(Vector3.up, buttonRotateSpeed);
        }

        private void OnEnable()
        {
            characterTransform.localEulerAngles = Vector3.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (characterTransform)
                characterTransform.Rotate(Vector3.up, -eventData.delta.x * rotateSpeed);
        }
    }
}