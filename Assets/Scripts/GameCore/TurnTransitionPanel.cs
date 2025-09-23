using UnityEngine;
using UnityEngine.UI;

namespace Shashki
{
    /// <summary>
    /// Компонент для отображения темной полупрозрачной панели между ходами.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TurnTransitionPanel : MonoBehaviour
    {
        [SerializeField] private Color _panelColor = new Color(0f, 0f, 0f, 0.5f); // Полупрозрачный черный
        [SerializeField] private float _fadeDuration = 0.3f; // Длительность анимации появления/исчезновения

        private Image _panelImage;
        private bool _isFading;

        private void Awake()
        {
            _panelImage = GetComponent<Image>();
            if (_panelImage == null)
            {
                Debug.LogError("[TurnTransitionPanel] Image component not found!");
                return;
            }
            _panelImage.color = new Color(_panelColor.r, _panelColor.g, _panelColor.b, 0f); // Изначально невидимая
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Показать панель с анимацией появления.
        /// </summary>
        public void Show()
        {
            if (_isFading) return;
            gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Скрыть панель с анимацией исчезновения.
        /// </summary>
        public void Hide()
        {
            if (_isFading) return;
            StartCoroutine(FadeOut());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            _isFading = true;
            float elapsed = 0f;
            Color startColor = new Color(_panelColor.r, _panelColor.g, _panelColor.b, 0f);
            Color targetColor = _panelColor;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;
                _panelImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            _panelImage.color = targetColor;
            _isFading = false;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            _isFading = true;
            float elapsed = 0f;
            Color startColor = _panelImage.color;
            Color targetColor = new Color(_panelColor.r, _panelColor.g, _panelColor.b, 0f);

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;
                _panelImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            _panelImage.color = targetColor;
            gameObject.SetActive(false);
            _isFading = false;
        }
    }
}