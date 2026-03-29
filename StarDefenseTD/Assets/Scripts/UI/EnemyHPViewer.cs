using UnityEngine;
using UnityEngine.UI;

public class EnemyHPViewer : MonoBehaviour
{
    private EnemyHP enemyHP;
    private Slider hpSlider;

    public void Setup(EnemyHP enemyHP)
    {
        this.enemyHP = enemyHP;

        if (hpSlider == null)
        {
            hpSlider = GetComponent<Slider>();
        }
    }

    private void Update()
    {
        if (enemyHP == null || hpSlider == null)
        {
            return;
        }

        hpSlider.value = enemyHP.CurrentHP / enemyHP.MaxHP;
    }

    private void OnDisable()
    {
        enemyHP = null;

        if (hpSlider == null)
        {
            hpSlider = GetComponent<Slider>();
        }

        if (hpSlider != null)
        {
            hpSlider.value = 1.0f;
        }
    }
}
