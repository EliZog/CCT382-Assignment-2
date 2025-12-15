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
    public bool Dialogue;

    public Camera PlayerCamera;
    public LayerMask Towers;
    public TowerBehaviour PrevMenu;
    public Canvas PauseMenu;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentMoney = StartingMoney;
        CurrentHealth = StartingHealth;
        MoneyDisplayText.SetText($"${StartingMoney}");
        PrevMenu = null;
        Dialogue = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.enabled = !PauseMenu.enabled;
            PauseMenu.gameObject.SetActive(PauseMenu.enabled);
        }

        if (!Dialogue && Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Mouse click registered ");
            Ray camray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit HitInfo;

            if (Physics.Raycast(camray, out HitInfo, 100f, Towers))
            {
                //Debug.Log("Tower Click registered");
                TowerBehaviour temp = HitInfo.collider.gameObject.GetComponent<TowerBehaviour>();

                if (PrevMenu != null && PrevMenu.Menu && PrevMenu != temp)
                    PrevMenu.ToggleMenu();

                temp.ToggleMenu();
                PrevMenu = temp;
            }
        }
    }

    public void IsTalking(bool value)
    {
        Dialogue = value;
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
