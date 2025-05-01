using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public abstract class HoverPanel<TComponent> : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler where TComponent : Component {
        [SerializeField] private TComponent panelPrefab;
        private TComponent panel;
        
        public void OnPointerEnter(PointerEventData eventData){
            panel = Instantiate(panelPrefab, transform);
            InitPanel(panel);
        }
        protected abstract void InitPanel(TComponent panel);
        public void OnPointerExit(PointerEventData eventData){
            Destroy(panel.gameObject);
        }
    }
}
