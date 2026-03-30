using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTMPViewer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private TextMeshProUGUI textPlayerGold;
    [SerializeField] private PlayerGold playerGold;

    [SerializeField] private TextMeshProUGUI textPlayerMineral;
    [SerializeField] private PlayerMineral playerMineral;

    [SerializeField] private TextMeshProUGUI textWave;
    [SerializeField] private WaveSystem waveSystem;

    private void Update()
    {
        textMeshProUGUI.text = $"{playerHP.CurrentHP}";
        textPlayerGold.text = $"{playerGold.CurrnetGold}";
        textPlayerMineral.text = $"{playerMineral.CurrentMineral}";
        textWave.text = $"{waveSystem.CurrentWaveNumber} / {waveSystem.MaxWave}";
    }
}
