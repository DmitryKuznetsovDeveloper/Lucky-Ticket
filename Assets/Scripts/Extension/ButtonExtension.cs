using System;
using UnityEngine;
using UnityEngine.UI;
public static class ButtonExtensions
{
    public static void AddListener(this Button button, Action<Action> actionWithCallback)
    {
        button.onClick.AddListener(() =>
        {
            // Вызываем метод с коллбеком, который выполнится после основного действия
            actionWithCallback?.Invoke(() =>
            {
                // Здесь можно выполнить дополнительные действия после основного действия
                Debug.Log("Callback после основного действия.");
            });
        });
    }
}