using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTMPViewer : MonoBehaviour
{
   [SerializeField] private  TextMeshProUGUI textMeshProUGUI;
   [SerializeField] private PlayerHP playerHP;
   [SerializeField] private TextMeshProUGUI textPlayerGold;
   [SerializeField] private PlayerGold playerGold;

   private void Update()
    {
        textMeshProUGUI.text = $"{playerHP.CurrentHP} / {playerHP.MaxHP}";
        textPlayerGold.text = $"{playerGold.CurrnetGold}";
    }
}
