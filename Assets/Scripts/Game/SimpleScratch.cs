using System;
using UnityEngine;
using UnityEngine.UI;
namespace Game
{
    public sealed class SimpleScratch : MonoBehaviour
    {
        public event Action OnEraseCompleted;
        [SerializeField] private Texture2D _brushTexture; 
        [SerializeField] private float _brushSize = 1f; 
    
        private RawImage _scratchSurface;  
        private Texture2D _originalTexture; 
        private Texture2D _textureWithAlpha;
        private Color[] _originalPixels;      
        private Color[] _pixels;
        private float _erasePercentage;
        private int _totalPixels;
        private int _transparentPixels;     
        private bool _isDirty;        

        public void Awake()
        {
            _scratchSurface = GetComponent<RawImage>();
            _originalTexture = (Texture2D)_scratchSurface.texture!;
        }
        void Start()
        {
            _textureWithAlpha = new Texture2D(_originalTexture.width, _originalTexture.height, TextureFormat.RGBA32, false);
            _originalPixels = _originalTexture.GetPixels();
            _pixels = _originalPixels.Clone() as Color[];  // Копируем пиксели
            _textureWithAlpha.SetPixels(_pixels);
            _textureWithAlpha.Apply();

            // Устанавливаем текстуру с альфа-каналом на UI элемент
            _scratchSurface.texture = _textureWithAlpha;

            // Рассчитываем общее количество пикселей
            _totalPixels = _textureWithAlpha.width * _textureWithAlpha.height;
            _transparentPixels = 0;  // Изначально нет прозрачных пикселей
        }

        void Update()
        {
            // Проверка на попадание в область, если нажата левая кнопка мыши
            if (Input.GetMouseButton(0))
            {
                Vector2 mousePosition = Input.mousePosition;

                // Проверяем, находится ли курсор в области текущего SimpleScratch
                RectTransform rectTransform = _scratchSurface.rectTransform;
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition, null))
                {
                    DrawOnTexture(mousePosition);
                    _isDirty = true;  // Отмечаем, что текстура была изменена
                }
            }

            // Пересчитываем процент стирания только при изменении текстуры
            if (_isDirty)
            { 
                _erasePercentage = (_transparentPixels / (float)_totalPixels) * 100f;
                if (_erasePercentage >= 80f) OnEraseCompleted?.Invoke();
                _isDirty = false;  // Сбрасываем флаг
            }
        }


        /// <summary>
        /// Метод для стирания (создания прозрачных областей) на текстуре
        /// </summary>
        /// <param name="position">Позиция тача или мышки</param>
        void DrawOnTexture(Vector2 position)
        {
            // Преобразуем экранные координаты в координаты текстуры
            RectTransform rectTransform = _scratchSurface.rectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, null, out var localPos);

            // Преобразуем локальные координаты в текстурные
            var rect = rectTransform.rect;
            float x = (localPos.x + rect.width / 2) * (_textureWithAlpha.width / rect.width);
            float y = (localPos.y + rect.height / 2) * (_textureWithAlpha.height / rect.height);

            // Рассчитываем масштабированные позиции для кисти с учетом размера кисти
            int scaledBrushWidth = Mathf.RoundToInt(_brushTexture.width * _brushSize);
            int scaledBrushHeight = Mathf.RoundToInt(_brushTexture.height * _brushSize);

            // Позиции для кисти на текстуре
            int brushX = Mathf.Clamp((int)x - scaledBrushWidth / 2, 0, _textureWithAlpha.width - scaledBrushWidth);
            int brushY = Mathf.Clamp((int)y - scaledBrushHeight / 2, 0, _textureWithAlpha.height - scaledBrushHeight);

            // Применяем прозрачный цвет для каждого пикселя, где "рисует" кисть
            for (int i = 0; i < scaledBrushWidth; i++)
            {
                for (int j = 0; j < scaledBrushHeight; j++)
                {
                    // Координаты внутри кисти
                    float brushU = (float)i / scaledBrushWidth;
                    float brushV = (float)j / scaledBrushHeight;

                    // Получаем пиксели кисти с масштабированными координатами
                    Color brushPixel = _brushTexture.GetPixelBilinear(brushU, brushV);

                    // Проверяем, что пиксели кисти не полностью прозрачные
                    if (brushPixel.a > 0)
                    {
                        int pixelIndex = (brushY + j) * _textureWithAlpha.width + (brushX + i);
                        if (_pixels[pixelIndex].a != 0)  // Если пиксель еще не прозрачный
                        {
                            // Устанавливаем полностью прозрачный цвет (RGBA: 1,1,1,0) для текущего пикселя
                            _pixels[pixelIndex] = new Color(_pixels[pixelIndex].r, _pixels[pixelIndex].g, _pixels[pixelIndex].b, 0);
                            _transparentPixels++;  // Увеличиваем счетчик прозрачных пикселей
                        }
                    }
                }
            }

            // Применяем изменения реже
            _textureWithAlpha.SetPixels(_pixels);
            _textureWithAlpha.Apply();
        }

        /// <summary>
        /// Метод для сброса альфа-канала (восстановления оригинальной текстуры)
        /// </summary>
        public void ResetTexture()
        {
            if (_originalPixels == null || _textureWithAlpha == null) return;
            _pixels = _originalPixels.Clone() as Color[];  // Восстанавливаем оригинальные пиксели
            _textureWithAlpha.SetPixels(_pixels);
            _textureWithAlpha.Apply();
            _transparentPixels = 0;  // Сбрасываем счетчик прозрачных пикселей
        }
    }
}
