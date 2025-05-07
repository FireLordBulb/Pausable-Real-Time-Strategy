using UnityEngine;
using UnityEngine.UI;

namespace Player {
    public class MapModeSelect : MonoBehaviour {
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private int defaultMapMode;
        [SerializeField] private float xOffset;

        private Button[] buttons;
        private Button currentMapModeButton;
        
        public CameraInput CameraInput {private get; set;}

        private void Awake(){
            buttons = new Button[sprites.Length];
            for (int i = 0; i < sprites.Length; i++){
                buttons[i] = Instantiate(buttonPrefab, transform);
                int mapModeIndex = i;
                buttons[i].onClick.AddListener(() => Select(mapModeIndex));
                buttons[i].image.sprite = sprites[i];
                ((RectTransform)buttons[i].transform).anchoredPosition = new Vector2(i*xOffset, 0);
            }
            currentMapModeButton = buttons[defaultMapMode];
            currentMapModeButton.interactable = false;
        }

        public void Select(int index){
            if (buttons == null || !buttons[index].interactable){
                return;
            }
            CameraInput.SetMapMode(index);
            currentMapModeButton.interactable = true;
            currentMapModeButton = buttons[index];
            currentMapModeButton.interactable = false;
        }
    }
}
