using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private LayerMask PlacementCheckMask;
    [SerializeField] private LayerMask PlacementCollideMask;
    
    [SerializeField] private PlayerStats PlayerStatistics;
    [SerializeField] private Camera PlayerCamera;

    private GameObject CurrentPlacingTower;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentPlacingTower != null)
        {
            Ray camray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit HitInfo;

            if (Physics.Raycast(camray, out HitInfo, 100f, PlacementCollideMask))
            {
                CurrentPlacingTower.transform.position = HitInfo.point;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Destroy(CurrentPlacingTower);
                CurrentPlacingTower = null;
                return;
            }

            if (Input.GetMouseButtonDown(0) && HitInfo.collider.gameObject != null)
            {
                if (!HitInfo.collider.gameObject.CompareTag("CantPlace")) 
                {
                    BoxCollider TowerCollider = CurrentPlacingTower.gameObject.GetComponent<BoxCollider>();
                    TowerCollider.isTrigger = true;

                    Vector3 BoxCenter = CurrentPlacingTower.gameObject.transform.position + TowerCollider.center;
                    Vector3 HalfExtents = TowerCollider.size / 2;

                    if (!Physics.CheckBox(BoxCenter, HalfExtents, Quaternion.identity, PlacementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        TowerBehaviour CurrentTowerBehaviour = CurrentPlacingTower.GetComponent<TowerBehaviour>();
                        GameLoopManager.TowersInGame.Add(CurrentTowerBehaviour);

                        PlayerStatistics.AddMoney(-CurrentTowerBehaviour.SummonCost); // remove money when placing tower

                        TowerCollider.isTrigger = false;
                        CurrentPlacingTower = null;
                    }
                    
                }
            }
        }
    }


    public void SetTowerToPlace(GameObject tower)
    {
        int TowerSummonCost = tower.GetComponent<TowerBehaviour>().SummonCost;

        if (PlayerStatistics.GetMoney() >= TowerSummonCost)
        {
            CurrentPlacingTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
        }
        else
        {
            // ADD THE MESSAGE TO THE UI SAYING YOU CAN'T AFFORD IT
            Debug.Log("You need more money to purchase a " + tower.name);
        }
    }
}
