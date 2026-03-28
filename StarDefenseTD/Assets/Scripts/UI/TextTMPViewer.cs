using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTMPViewer : MonoBehaviour
{
   [SerializeField] private  TextMeshProUGUI textMeshProUGUI;
   [SerializeField] private PlayerHP playerHP;

   private void Update()
    {
        textMeshProUGUI.text = $"{playerHP.CurrentHP} / {playerHP.MaxHP}";
    }
}
