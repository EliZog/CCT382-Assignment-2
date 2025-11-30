using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MoneyDisplayText;
    [SerializeField] private TextMeshProUGUI HealthDisplayText;
    [SerializeField] private int StartingMoney;
    [SerializeField] private int StartingHealth;
    private int CurrentMoney;
    private float CurrentHealth;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentMoney = StartingMoney;
        CurrentHealth = StartingHealth;
        MoneyDisplayText.SetText($"${StartingMoney}");
    }

    public void AddMoney(int MoneyToAdd)
    {
        CurrentMoney += MoneyToAdd;
        MoneyDisplayText.SetText($"${CurrentMoney}");

    }

    public int GetMoney()
    {
        return CurrentMoney;
    }

    public float GetHealth()
    {
        return CurrentHealth;
    }

    public void RemoveHealth(float HealthToRemove)
    {
        CurrentHealth -= HealthToRemove;
        HealthDisplayText.SetText($"x{CurrentHealth}");

    }

    public void SetHealthToZero()
    {
        CurrentHealth = 0f;
        HealthDisplayText.SetText($"x{CurrentHealth}");

    }
}
