using UnityEngine;
using UnityEngine.UI;

public class SimpleScratch : MonoBehaviour
{
    public float ErasePercentage => erasePercentage;
    
    [SerializeField] private Texture2D _brushTexture; 
    [SerializeField] private float _brushSize = 1f; 
    
    private RawImage scratchSurface;  
    private Texture2D originalTexture; 
    private Texture2D textureWithAlpha;
    private Color[] originalPixels;      
    private Color[] pixels;
    private float erasePercentage;
    private int totalPixels;
    private int transparentPixels;     
    private bool isDirty;        

    private void Awake() => scratchSurface = GetComponent<RawImage>();
    void Start()
    {
        originalTexture = (Texture2D)scratchSurface.texture!;
        textureWithAlpha = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        originalPixels = originalTexture.GetPixels();
        pixels = originalPixels.Clone() as Color[];  // Копируем пиксели
        textureWithAlpha.SetPixels(pixels);
        textureWithAlpha.Apply();

        // Устанавливаем текстуру с альфа-каналом на UI элемент
        scratchSurface.texture = textureWithAlpha;

        // Рассчитываем общее количество пикселей
        totalPixels = textureWithAlpha.width * textureWithAlpha.height;
        transparentPixels = 0;  // Изначально нет прозрачных пикселей
    }

    void Update()
    {
        // Если нажата левая кнопка мыши, то начинается стирание
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            DrawOnTexture(mousePosition);
            isDirty = true;  // Отмечаем, что текстура была изменена
        }

        // Пересчитываем процент стирания только при изменении текстуры
        if (isDirty)
        { 
            erasePercentage = (transparentPixels / (float)totalPixels) * 100f;
            Debug.Log("Percentage Erased: " + erasePercentage + "%");
            isDirty = false;  // Сбрасываем флаг
        }
    }

    /// <summary>
    /// Метод для стирания (создания прозрачных областей) на текстуре
    /// </summary>
    /// <param name="position">Позиция тача или мышки</param>
    void DrawOnTexture(Vector2 position)
    {
        // Преобразуем экранные координаты в координаты текстуры
        RectTransform rectTransform = scratchSurface.rectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, null, out var localPos);

        // Преобразуем локальные координаты в текстурные
        var rect = rectTransform.rect;
        float x = (localPos.x + rect.width / 2) * (textureWithAlpha.width / rect.width);
        float y = (localPos.y + rect.height / 2) * (textureWithAlpha.height / rect.height);

        // Рассчитываем масштабированные позиции для кисти с учетом размера кисти
        int scaledBrushWidth = Mathf.RoundToInt(_brushTexture.width * _brushSize);
        int scaledBrushHeight = Mathf.RoundToInt(_brushTexture.height * _brushSize);

        // Позиции для кисти на текстуре
        int brushX = Mathf.Clamp((int)x - scaledBrushWidth / 2, 0, textureWithAlpha.width - scaledBrushWidth);
        int brushY = Mathf.Clamp((int)y - scaledBrushHeight / 2, 0, textureWithAlpha.height - scaledBrushHeight);

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
                    int pixelIndex = (brushY + j) * textureWithAlpha.width + (brushX + i);
                    if (pixels[pixelIndex].a != 0)  // Если пиксель еще не прозрачный
                    {
                        // Устанавливаем полностью прозрачный цвет (RGBA: 1,1,1,0) для текущего пикселя
                        pixels[pixelIndex] = new Color(pixels[pixelIndex].r, pixels[pixelIndex].g, pixels[pixelIndex].b, 0);
                        transparentPixels++;  // Увеличиваем счетчик прозрачных пикселей
                    }
                }
            }
        }

        // Применяем изменения реже
        textureWithAlpha.SetPixels(pixels);
        textureWithAlpha.Apply();
    }

    /// <summary>
    /// Метод для сброса альфа-канала (восстановления оригинальной текстуры)
    /// </summary>
    public void ResetTexture()
    {
        pixels = originalPixels.Clone() as Color[];  // Восстанавливаем оригинальные пиксели
        textureWithAlpha.SetPixels(pixels);
        textureWithAlpha.Apply();
        transparentPixels = 0;  // Сбрасываем счетчик прозрачных пикселей
    }
}
