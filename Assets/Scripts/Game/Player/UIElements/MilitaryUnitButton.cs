using Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class MilitaryUnitButton : MonoBehaviour {
		[SerializeField] private Button button;
		[SerializeField] private RectTransform border;
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private RectTransform infoBox;
		[SerializeField] private TextMeshProUGUI cost;
		[SerializeField] private TextMeshProUGUI message;
		[SerializeField] private Color activeTint;

		private Color defaultColor; 
		private Color activeColor; 
		
		public RectTransform RectTransform {get; private set;}
		
		public Button Button => button;
		public TextMeshProUGUI Label => label;
		public TextMeshProUGUI Cost => cost;
		public TextMeshProUGUI Message => message;
		
		private void Awake(){
			RectTransform = (RectTransform)transform;
			defaultColor = Button.image.color;
			activeColor = defaultColor*activeTint;
		}

		public void ShowInfoBox(){
			Button.image.color = activeColor;
			infoBox.gameObject.SetActive(true);
			VectorGeometry.SetRectHeight(RectTransform, RectTransform.rect.height+infoBox.rect.height);
		}
		public void HideInfoBox(){
			Button.image.color = defaultColor;
			infoBox.gameObject.SetActive(false);
			RectTransform.sizeDelta = border.sizeDelta;
		}
	}
}