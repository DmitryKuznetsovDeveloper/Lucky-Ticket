using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Game;
using UI.View;
using VContainer;

public sealed class InfiniteCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject _ticketPrefab;
    [SerializeField] private List<FastTicketConfigs> _configs;
    [SerializeField] private float _spacing = 850f;
    [SerializeField] private float _animationDuration = 1f;

    private List<TicketView> _ticketViews = new List<TicketView>();
    private List<PurchaseHandler> _purchaseHandlers = new List<PurchaseHandler>();
    private readonly HashSet<int> _hiddenConfigs = new HashSet<int>();
    private int _currentConfigsIndex;
    private bool _isScrollLocked;
    private Vector2 _startDragPosition;
    private IObjectResolver _container;

    [Inject]
    public void Construct(IObjectResolver container) => _container = container;

    public event Action<FastTicketConfigs> OnCenterConfigChanged;
    
    public void LockScroll() => _isScrollLocked = true;
    public void UnlockScroll() => _isScrollLocked = false;
    
    private void Start()
    {
        InitializeTickets();
        UpdateCarouselContent();
        ArrangeCarousel();
        InvokeCenterConfigChanged();
    }

    // Метод InitializeTickets для создания нужного количества TicketView
    private void InitializeTickets()
    {
        // Установите нужное количество видимых элементов, например 5
        for (int i = 0; i < 5; i++)
        {
            var ticketObject = Instantiate(_ticketPrefab, transform);
            var ticketView = ticketObject.GetComponent<TicketView>();
            _ticketViews.Add(ticketView);
        }
    }

    // Метод для обновления контента элементов карусели
    private void UpdateCarouselContent()
    {
        int visibleIndex = _currentConfigsIndex;

        // Освобождаем старые PurchaseHandler'ы
        foreach (var handler in _purchaseHandlers)
            handler.Dispose();

        _purchaseHandlers.Clear();

        // Заполняем каждый элемент новыми данными из конфигураций
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            visibleIndex = GetNextVisibleConfigIndex(visibleIndex);

            var config = _configs[visibleIndex];
            var ticketView = _ticketViews[i];

            // Устанавливаем данные для каждого TicketView
            ticketView.SetTitleLabel(config.TitleLabel);
            ticketView.SetRewardLabel(config.GetReward());
            ticketView.SetArtImage(config.ArtIcon);
            ticketView.SetPriceLabel(config.BuyButton);

            var simpleScratch = ticketView.SimpleScratch;
            if (simpleScratch == null)
            {
                Debug.LogError("SimpleScratch is missing in TicketView. Ensure each TicketView has a unique SimpleScratch component.");
                continue;
            }

            simpleScratch.ResetTexture();

            // Создаем PurchaseHandler для каждого TicketView
            var purchaseHandler = new PurchaseHandler(
                _container.Resolve<CurrencyManager>(),
                ticketView,
                simpleScratch,
                config.BuyButton,
                config.GetReward(),
                this
            );

            // Подписываемся на событие покупки для обновления конфига
            purchaseHandler.OnPurchaseCompleted += () => RefreshConfig(config);

            purchaseHandler.Initialize();
            _purchaseHandlers.Add(purchaseHandler);

            visibleIndex = (visibleIndex + 1) % _configs.Count;
        }
    }

    // Метод для обновления конкретного элемента новыми данными из следующего конфига
    public void RefreshConfig(FastTicketConfigs config)
    {
        int index = _configs.IndexOf(config);
        if (index >= 0)
        {
            _currentConfigsIndex = GetNextVisibleConfigIndex(index);
            UpdateCarouselContent(); // Перезаполняем контент
        }
    }

    private int GetNextVisibleConfigIndex(int startIndex)
    {
        int index = startIndex;
        while (_hiddenConfigs.Contains(index))
        {
            index = (index + 1) % _configs.Count;
        }
        return index;
    }

    private void InvokeCenterConfigChanged() => OnCenterConfigChanged?.Invoke(_configs[_currentConfigsIndex]);

    private void ArrangeCarousel()
    {
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            float offset = (i - 1) * _spacing;
            _ticketViews[i].transform.localPosition = new Vector3(offset, i == 1 ? -50 : 0, 0);
            _ticketViews[i].transform.localScale = i == 1 ? Vector3.one * 1.2f : Vector3.one;
            _ticketViews[i].transform.SetSiblingIndex(i == 1 ? _ticketViews.Count - 1 : i);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isScrollLocked) return;
        _startDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isScrollLocked) return;
        Vector2 currentDragPosition = eventData.position;
        float dragDistance = currentDragPosition.x - _startDragPosition.x;

        for (int i = 0; i < _ticketViews.Count; i++)
        {
            float targetX = (i - 1) * _spacing + dragDistance;
            _ticketViews[i].transform.localPosition = new Vector3(targetX, _ticketViews[i].transform.localPosition.y, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isScrollLocked) return;
        float dragDistance = eventData.position.x - _startDragPosition.x;
        if (Mathf.Abs(dragDistance) > 100f)
        {
            if (dragDistance > 0)
            {
                // Прокрутка вправо
                _currentConfigsIndex = GetNextVisibleConfigIndex((_currentConfigsIndex - 1 + _configs.Count) % _configs.Count);
                ShiftTicketsRight();
            }
            else
            {
                // Прокрутка влево
                _currentConfigsIndex = GetNextVisibleConfigIndex((_currentConfigsIndex + 1) % _configs.Count);
                ShiftTicketsLeft();
            }

            UpdateCarouselContent();
            InvokeCenterConfigChanged();
        }
        
        SmoothMoveToCenter();
    }

    private void ShiftTicketsRight()
    {
        var lastTicket = _ticketViews[_ticketViews.Count - 1];
        _ticketViews.RemoveAt(_ticketViews.Count - 1);
        _ticketViews.Insert(0, lastTicket);
    }

    private void ShiftTicketsLeft()
    {
        var firstTicket = _ticketViews[0];
        _ticketViews.RemoveAt(0);
        _ticketViews.Add(firstTicket);
    }

    private void SmoothMoveToCenter()
    {
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            float targetX = (i - 1) * _spacing;
            _ticketViews[i].transform.DOLocalMoveX(targetX, _animationDuration).OnComplete(UpdateCarouselState);
            _ticketViews[i].transform.DOScale(i == 1 ? 1.2f : 1f, _animationDuration);
        }
        _ticketViews[1].transform.SetSiblingIndex(_ticketViews.Count - 1);
    }

    private void UpdateCarouselState()
    {
        for (int i = 0; i < _ticketViews.Count; i++)
        {
            var state = i == 1 ? TicketView.AnimationStateTicket.Selected : TicketView.AnimationStateTicket.Normal;
            _ticketViews[i].PlayAnimation(state);
        }
    }
}
