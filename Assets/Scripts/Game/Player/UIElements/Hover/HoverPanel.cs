using UnityEngine;
using UnityEngine.EventSystems;

namespace Player {
    [RequireComponent(typeof(RectTransform))]
    public abstract class HoverPanel<TComponent> : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler where TComponent : Component {
        [SerializeField] private TComponent panelPrefab;
        
        private bool doAlwaysShow;
        private TComponent panel;
        
        public void AlwaysShow(){
            doAlwaysShow = true;
            SpawnPanel();
        }
        public void OnPointerEnter(PointerEventData eventData){
            if (!doAlwaysShow){
                SpawnPanel();
            }
        }
        private void SpawnPanel(){
            panel = Instantiate(panelPrefab, transform);
            InitPanel(panel);
        }
        protected abstract void InitPanel(TComponent panel);
        public void OnPointerExit(PointerEventData eventData){
            if (!doAlwaysShow){
                RemovePanel();
            }
        }
        public void RemovePanel(){
            Destroy(panel.gameObject);
        }
    }
}
