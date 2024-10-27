using TMPro;
using UnityEngine;
namespace UI.View
{
    public class CurrencyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currentCurrency;
        
        public void SeCurrency(int value) => _currentCurrency.text = $"{value}";
    }
}