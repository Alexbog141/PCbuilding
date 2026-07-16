using UnityEngine;
using UnityEngine.InputSystem; // Подключаем новую систему ввода

public class AITester : MonoBehaviour
{
    public AIController aiController;

    void Update()
    {
        // Проверяем, есть ли клавиатура, и нажата ли клавиша пробел в этом кадре
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Тестер: Отправляю запрос к ИИ...");

            string testPrompt = @"Проверь совместимость ПК.
Установлено:
Мат. плата: ASUS ROG STRIX B550-F GAMING
CPU: AMD Ryzen 5 5600X
RAM: Нет
Новая деталь: Kingston FURY Beast 16GB.
Есть ли несовместимость? Ответь в одно предложение:";

            aiController.CheckCompatibility(testPrompt, (result) =>
            {
                Debug.Log($"<color=green>Тестер получил ответ от ИИ:</color> {result}");
            });
        }
    }
}