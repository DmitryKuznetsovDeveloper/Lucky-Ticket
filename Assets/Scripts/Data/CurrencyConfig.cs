using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "CurrencyConfig", menuName = "Configs/CurrencyConfig", order = 0)]
    public class CurrencyConfig : ScriptableObject
    {
        public int Currency = 100;
    }
}