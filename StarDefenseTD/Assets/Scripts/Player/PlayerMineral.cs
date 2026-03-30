using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMineral : MonoBehaviour
{
    [SerializeField] private int currentMineral = 0;
    public int CurrentMineral
    {
        set => currentMineral = Mathf.Max(0, value);
        get => currentMineral;
    }
}
