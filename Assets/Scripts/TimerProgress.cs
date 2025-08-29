using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shashki
{
    public class TimerProgress : MonoBehaviour
    {
        [SerializeField] private Image _timerImg;
        [SerializeField] private RectTransform _fillImageRect;
        [SerializeField] private Color _playerColor;
        [SerializeField] private Color _opponentColor;
            
        [Range(0f, 1f)] [SerializeField] public float _value;
        private object _maxWidth;

        private void Awake()
        {
            _fillImageRect ??= _timerImg.GetComponent<RectTransform>();
            _maxWidth = _fillImageRect.rect.width;
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            // Изменяем размер по горизонтали
            _fillImageRect.localScale = new Vector3(progress, 1f, 1f);
            // Или используем sizeDelta, если нужно точное управление шириной
            // _fillImageRect.sizeDelta = new Vector2(_maxWidth * progress, fillImageRect.sizeDelta.y);
        }

        public void SetOwner(bool isPlayer)
        {
            if (isPlayer)
                _fillImageRect.pivot = new Vector2(0f, 0.5f);
            else
                _fillImageRect.pivot = new Vector2(1f, 0.5f);
            
            _timerImg.color = isPlayer ? _playerColor : _opponentColor;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_fillImageRect == null)
                _fillImageRect = _timerImg.GetComponent<RectTransform>();
            
            SetProgress(_value);
            _fillImageRect.pivot = new Vector2(0f, 0.5f);
            _timerImg.color = _playerColor;
        }
#endif
        public void ResetProgress()
        {
            SetProgress(1);
            //TODO add animation dotween
        }
        
    }
}