using System;
using DG.Tweening;
using UnityEngine;

namespace GasChromatography.Manager
{
    public class CityCameraHandle : Singleton<CityCameraHandle>
    {
        public Vector3 Pos;
        public Quaternion rot;

        public bool isPause;

        public float MoveSpeeed = 3;
        public float QESpeeed = 3;

        public bool allowVerticalRotation = true;
        public float minVerticalAngle = -80f;
        public float maxVerticalAngle = 80f;
        private Transform _nowtransform;

        public Camera MainCamera => Camera.main;

        public Transform NowCamera
        {
            get => _nowtransform;
            set
            {
                if (_nowtransform != value)
                {
                    if (_nowtransform != Camera.main.transform)
                        if (Pos != null && _nowtransform != null)
                        {
                            _nowtransform.position = Pos;
                            _nowtransform.rotation = rot;
                        }

                    _nowtransform = value;
                    Pos = _nowtransform.position;
                    rot = _nowtransform.rotation;
                }
                else
                {
                    _nowtransform = value;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            NowCamera = GetComponent<Transform>();
        }


        // Update is called once per frame
        private void Update()
        {
            if (_nowtransform != null)
            {
                if (!isPause)
                {
                    if (Input.GetKey(KeyCode.Q))
                        _nowtransform.position -= new Vector3(0, 1, 0) * Time.deltaTime * QESpeeed;
                    if (Input.GetKey(KeyCode.E))
                        _nowtransform.position += new Vector3(0, 1, 0) * Time.deltaTime * QESpeeed;
                    Move();
                }

                ControlViewTransLate();
            }
        }

        private void Move()
        {
            var a = Input.GetAxis("Vertical");
            var b = Input.GetAxis("Horizontal");

            _nowtransform.position +=
                _nowtransform.TransformDirection(new Vector3(b, 0, a)) * Time.deltaTime * MoveSpeeed;
        }


        private void ViewTransLate()
        {
            if (Input.GetMouseButton(1))
            {
                var View = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);

                _nowtransform.localEulerAngles += View;
            }
        }

        private void ControlViewTransLate()
        {
            if (Input.GetMouseButton(1))
            {
                var delta = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);

                if (!allowVerticalRotation) delta.x = 0f;

                var e = _nowtransform.localEulerAngles;
                var curX = e.x;
                if (curX > 180f) curX -= 360f;
                var newX = Mathf.Clamp(curX + delta.x, minVerticalAngle, maxVerticalAngle);
                var newY = e.y + delta.y;

                _nowtransform.localEulerAngles = new Vector3(newX, newY, e.z);
            }
        }

        public void MoveCameraToTargetPoint(Transform point, Action onCompleted = null, float duration = 2f,
            bool isRotate = true)
        {
            NowCamera.DOMove(point.position, duration).OnComplete(() => {
                Debug.Log(point.name);onCompleted?.Invoke(); });

            if (isRotate)
                NowCamera.DORotate(point.eulerAngles, duration);
        }
    }
}