using TMPro;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public class TableRow : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI[] cells;
        
        public RectTransform RectTransform {get; private set;}
        public int ColumnCount => cells.Length;

        private void Awake(){
            RectTransform = transform as RectTransform;
        }

        public void SetCell(int index, string text){
            cells[index].text = text;
        }
    }
}
