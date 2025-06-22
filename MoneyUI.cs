using TMPro;
using UnityEngine;
using System.Collections;

public class MoneyUI : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Анимация прибавки")]
    [SerializeField] private TextMeshProUGUI moneyGainText;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField] private Color gainColor = Color.green;
    [SerializeField] private Color lossColor = Color.red;

    private Vector3 originalGainTextPosition;
    private Coroutine currentAnimation;

    private void Awake()
    {
        if (moneyGainText != null)
        {
            originalGainTextPosition = moneyGainText.transform.localPosition;
            moneyGainText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Основное обновление баланса
        moneyText.text = $"${DatabaseManager.Instance.GetMoney():F0}";
    }
    
    public void ShowMoneyChange(float amount)
    {
        if (moneyGainText == null) return;

        // Останавливаем предыдущую анимацию
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Настраиваем текст
        moneyGainText.text = $"{(amount >= 0 ? "+" : "")}{amount:F0}";
        moneyGainText.color = amount >= 0 ? gainColor : lossColor;
        moneyGainText.transform.localPosition = originalGainTextPosition;
        moneyGainText.alpha = 1f;
        moneyGainText.gameObject.SetActive(true);

        // Запускаем новую анимацию
        currentAnimation = StartCoroutine(AnimateMoneyChange());
    }

    private IEnumerator AnimateMoneyChange()
    {
        float elapsed = 0f;
        Vector3 startPosition = moneyGainText.transform.localPosition;
        Vector3 targetPosition = startPosition + new Vector3(0, moveDistance, 0);
        Color startColor = moneyGainText.color;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            // Движение вверх
            moneyGainText.transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            // Плавное исчезновение
            Color newColor = startColor;
            newColor.a = fadeCurve.Evaluate(progress);
            moneyGainText.color = newColor;

            yield return null;
        }

        moneyGainText.gameObject.SetActive(false);
        currentAnimation = null;
    }
}