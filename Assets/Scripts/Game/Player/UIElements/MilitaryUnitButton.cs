using Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(Button), typeof(RectTransform))]
	public class MilitaryUnitButton : MonoBehaviour {
		[SerializeField] private RectTransform border;
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private RectTransform infoBox;
		[SerializeField] private TextMeshProUGUI cost;
		[SerializeField] private TextMeshProUGUI message;

		public RectTransform RectTransform {get; private set;}
		public Button Button {get; private set;}
		
		public TextMeshProUGUI Label => label;
		public TextMeshProUGUI Cost => cost;
		public TextMeshProUGUI Message => message;
		private void Awake(){
			RectTransform = (RectTransform)transform;
			Button = GetComponent<Button>();
		}

		public void ShowInfoBox(){
			infoBox.gameObject.SetActive(true);
			VectorGeometry.SetRectHeight(RectTransform, RectTransform.rect.height+infoBox.rect.height);
		}
		public void HideInfoBox(){
			infoBox.gameObject.SetActive(false);
			RectTransform.sizeDelta = border.sizeDelta;
		}
	}
}