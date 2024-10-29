using System;
using Game;
using UI.View;

public class PurchaseHandler : IDisposable
{
    public event Action OnPurchaseCompleted;
    private readonly CurrencyManager _currencyManager;
    private readonly TicketView _ticketView;
    private readonly SimpleScratch _simpleScratch;
    private readonly InfiniteCarousel _carousel;
    private readonly IAudioManager _audioManager;
    private readonly int _ticketCost;
    private readonly int _rewardAmount;

    public PurchaseHandler(
        CurrencyManager currencyManager,
        TicketView ticketView,
        SimpleScratch simpleScratch,
        int ticketCost,
        int rewardAmount,
        InfiniteCarousel carousel,
        IAudioManager audioManager)
    {
        _currencyManager = currencyManager;
        _ticketView = ticketView;
        _simpleScratch = simpleScratch;
        _ticketCost = ticketCost;
        _rewardAmount = rewardAmount;
        _carousel = carousel;
        _audioManager = audioManager;
    }

    public void Initialize() => _ticketView.Button.BaseButton.onClick.AddListener(() => _ticketView.Button.PlayOnClick(OnBuyTicket));


    private void OnBuyTicket()
    {
        _audioManager.PlayClickSound();
        _simpleScratch.OnEraseCompleted += OnEraseCompleted;
        _currencyManager.SpendCurrency(_ticketCost);
        _carousel.LockScroll();
        _ticketView.SetLocked(false);
        _ticketView.SetBuyButton(false);
    }

    private void OnEraseCompleted()
    {
        _audioManager.PlayClickSound();
        _currencyManager.AddCurrency(_rewardAmount);
        _carousel.UnlockScroll();
        UnlockTicketView();
        OnPurchaseCompleted?.Invoke();
        _simpleScratch.OnEraseCompleted -= OnEraseCompleted;
        _carousel.SmoothMoveToCenter();
    }

    private void UnlockTicketView()
    {
        _ticketView.SetLocked(true);
        _ticketView.SetBuyButton(true);
        _simpleScratch.ResetTexture();
    }
    public void Dispose()
    {
        _ticketView.Button.BaseButton.onClick.RemoveAllListeners();
        _simpleScratch.OnEraseCompleted -= OnEraseCompleted;
    }
}