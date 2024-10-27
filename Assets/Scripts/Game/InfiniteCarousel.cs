using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Game;
using UI.View;

public sealed class InfiniteCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject _ticketPrefab; // Один префаб для создания элементов
    [SerializeField] private List<FastTicketConfigs> _configs; // Конфигурации для элементов
    [SerializeField] private float _spacing = 850f; // Расстояние между элементами
    [SerializeField] private float _animationDuration = 1f; // Длительность анимации
    
    private readonly List<TicketView> _ticketViews = new List<TicketView>(); // Динамически созданные элементы
    private readonly HashSet<int> _hiddenConfigs = new HashSet<int>(); // Индексы скрытых конфигов
    private int _currentIndex = 0; // Индекс текущей центральной конфигурации
    private Vector2 _startDragPosition;
    private bool _isSwipeBlocked;

    // Событие, которое вызывается при изменении центрального элемента
    public event Action<FastTicketConfigs> OnCenterConfigChanged;

    public void SetSwipeBlocked(bool isBlocked) => _isSwipeBlocked = isBlocked;
    public SimpleScratch GetCurrentSimpleScratch() => _ticketViews[_currentIndex].GetComponentInChildren<SimpleScratch>();
    public TicketView GetCurrentTicketView() => _ticketViews[_currentIndex];
    private void Start()
    {
        // Создаем три элемента на основе префаба
        for (int i = 0; i < 3; i++)
        {
            var ticketObject = Instantiate(_ticketPrefab, transform);
            var ticketView = ticketObject.GetComponent<TicketView>();
            _ticketViews.Add(ticketView);
        }

        UpdateCarouselContent();
        ArrangeCarousel();
        UpdateCarouselState();
        InvokeCenterConfigChanged(); // Уведомляем об изначально установленном центральном элементе
    }

    // Метод для обновления данных каждого элемента из конфигураций
    private void UpdateCarouselContent()
    {
        int visibleIndex = _currentIndex;
        
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            // Находим следующий видимый конфиг
            visibleIndex = GetNextVisibleConfigIndex(visibleIndex);

            var config = _configs[visibleIndex];
            _ticketViews[i].SetTitleLabel(config.TitleLabel);
            _ticketViews[i].SetRewardLabel(config.GetReward());
            _ticketViews[i].SetArtImage(config.ArtIcon);
            _ticketViews[i].SetScratchFrame(config.ScratchFrame);
            _ticketViews[i].SetScratchCenter(config.ScratchCenter);
            _ticketViews[i].SetPriceLabel(config.BuyButton);

            // Переходим к следующему индексу для следующего элемента
            visibleIndex = (visibleIndex + 1) % _configs.Count;
        }
    }

    // Метод для получения текущей конфигурации центрального элемента
    public FastTicketConfigs GetCurrentCenterConfig()
    {
        return _configs[_currentIndex];
    }

    // Вспомогательный метод для вызова события при изменении центрального элемента
    private void InvokeCenterConfigChanged()
    {
        OnCenterConfigChanged?.Invoke(GetCurrentCenterConfig());
    }

    // Метод для поиска следующего видимого индекса конфигурации, который не скрыт
    private int GetNextVisibleConfigIndex(int startIndex)
    {
        int index = startIndex;
        while (_hiddenConfigs.Contains(index))
        {
            index = (index + 1) % _configs.Count;
        }
        return index;
    }

    // Метод для скрытия конфигурации, чтобы она больше не показывалась
    public void HideConfig(FastTicketConfigs config)
    {
        int index = _configs.IndexOf(config);
        if (index >= 0)
        {
            _hiddenConfigs.Add(index);
            UpdateCarouselContent();
        }
    }

    private void ArrangeCarousel()
    {
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            float offset = (i - 1) * _spacing;
            _ticketViews[i].transform.localPosition = new Vector3(offset, i == 1 ? -50 : 0, 0);
            _ticketViews[i].transform.localScale = i == 1 ? Vector3.one * 1.2f : Vector3.one;

            // Устанавливаем центральный элемент в последний индекс, чтобы он перекрывал остальные
            _ticketViews[i].transform.SetSiblingIndex(i == 1 ? _ticketViews.Count - 1 : i);
        }
    }

    private void UpdateCarouselState()
    {
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            var state = i == 1 ? TicketView.AnimationStateTicket.Selected : TicketView.AnimationStateTicket.Normal;
            _ticketViews[i].PlayAnimation(state);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentDragPosition = eventData.position;
        float dragDistance = currentDragPosition.x - _startDragPosition.x;

        // Перемещаем все элементы вместе с пальцем/мышкой
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            float targetX = (i - 1) * _spacing + dragDistance;
            _ticketViews[i].transform.localPosition = new Vector3(targetX, _ticketViews[i].transform.localPosition.y, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistance = eventData.position.x - _startDragPosition.x;
        if (Mathf.Abs(dragDistance) > 100f) // Проверяем минимальную длину свайпа для сдвига
        {
            // Определяем направление сдвига
            if (dragDistance > 0)
            {
                _currentIndex = GetNextVisibleConfigIndex((_currentIndex - 1 + _configs.Count) % _configs.Count);
                // Перемещаем последний элемент на первую позицию
                var lastTicket = _ticketViews[2];
                _ticketViews.RemoveAt(2);
                _ticketViews.Insert(0, lastTicket);
            }
            else
            {
                _currentIndex = GetNextVisibleConfigIndex((_currentIndex + 1) % _configs.Count);
                // Перемещаем первый элемент на последнюю позицию
                var firstTicket = _ticketViews[0];
                _ticketViews.RemoveAt(0);
                _ticketViews.Add(firstTicket);
            }

            UpdateCarouselContent();
            InvokeCenterConfigChanged(); // Вызываем событие изменения центрального элемента
        }
        
        SmoothMoveToCenter();
    }

    private void SmoothMoveToCenter()
    {
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            float targetX = (i - 1) * _spacing;
            _ticketViews[i].transform.DOLocalMoveX(targetX, _animationDuration)
                .OnComplete(UpdateCarouselState);
            _ticketViews[i].transform.DOScale(i == 1 ? 1.2f : 1f, _animationDuration);
        }
        
        // Устанавливаем центральный элемент в последний индекс, чтобы он всегда отображался поверх других
        _ticketViews[1].transform.SetSiblingIndex(_ticketViews.Count - 1);
    }
}
