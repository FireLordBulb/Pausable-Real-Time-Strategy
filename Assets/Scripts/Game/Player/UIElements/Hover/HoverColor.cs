using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Player {
    public class HoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private Graphic graphic;
        [SerializeField] private Color color;

        private Color regularColor;

        private void Awake(){
            regularColor = graphic.color;
        }
        public void OnPointerEnter(PointerEventData eventData){
            graphic.color = color;
        }
        public void OnPointerExit(PointerEventData eventData){
            graphic.color = regularColor;
        }
    }
}