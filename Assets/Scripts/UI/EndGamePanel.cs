using TMPro;
using UnityEngine;

namespace Shashki
{
    public class EndGamePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerText;



        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }
        
        public void Activate(Winner winner)
        {
            gameObject.SetActive(true);
            _playerText.text = winner.ToString();
        }
    }
}