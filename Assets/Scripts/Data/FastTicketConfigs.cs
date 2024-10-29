using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
namespace Data
{
    [CreateAssetMenu(fileName = "FastTicketConfigs", menuName = "Configs/FastTicketConfigs", order = 0)]
    public sealed class FastTicketConfigs : ScriptableObject
    {
        public string TitleLabel;
        public int BuyButton;
        public Sprite ArtIcon;

        [FormerlySerializedAs("rewards")]
        [Header("Награды")]
        public CardReward[] Rewards;

        // Метод для получения награды в зависимости от шансов
        public int GetReward()
        {
            float randomValue = Random.Range(0, 100f);

            float cumulativeChance = 0f;
            foreach (var reward in Rewards)
            {
                cumulativeChance += reward.dropChance;
                if (randomValue <= cumulativeChance)
                {
                    bool isNegative = Random.value < 0.34f;
                    return isNegative ? -reward.RewardAmount : reward.RewardAmount;
                }
            }
            return 0;
        }
    }
    
    [Serializable]
    public struct CardReward
    {
        public int RewardAmount;
        [Range(0, 100)] public float dropChance;
    }
}