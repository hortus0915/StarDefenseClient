using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsBuuldTower { get; set; }
    public TowerWeapon CurrentTower { get; private set; }

    private void Awake()
    {
        IsBuuldTower = CurrentTower != null;
    }

    public void SetTower(TowerWeapon tower)
    {
        CurrentTower = tower;
        IsBuuldTower = tower != null;
    }

    public void ClearTower(TowerWeapon tower)
    {
        if (CurrentTower != tower)
        {
            return;
        }

        CurrentTower = null;
        IsBuuldTower = false;
    }
}