using TMPro;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class LevelIndicatorElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;

        public void UpdateLevel(int levelIndex)
        {
            levelText.text = $"Level {levelIndex+1}";
        }
    }
}
