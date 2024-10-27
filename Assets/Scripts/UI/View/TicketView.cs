using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI.View
{
    public class TicketView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _rewardLabel;
        [SerializeField] private GameObject _scratchPrefab;
        [SerializeField] private CanvasGroup _lockedCanvasGroup;
        [SerializeField] private Image _artIcon;
        [SerializeField] private Image _frame;
        [SerializeField] private BaseButtonView _buyButton;
        
    }
}