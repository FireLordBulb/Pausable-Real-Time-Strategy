using Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(ButtonColorToggle))]
	public class MilitaryUnitButton : MonoBehaviour {
		[SerializeField] private RectTransform border;
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private RectTransform infoBox;
		[SerializeField] private TextMeshProUGUI cost;
		[SerializeField] private TextMeshProUGUI message;

		private ButtonColorToggle buttonColorToggle;
		
		public RectTransform RectTransform {get; private set;}
		
		public Button Button => buttonColorToggle.Button;
		public TextMeshProUGUI Label => label;
		public TextMeshProUGUI Cost => cost;
		public TextMeshProUGUI Message => message;
		
		private void Awake(){
			RectTransform = (RectTransform)transform;
			buttonColorToggle = GetComponent<ButtonColorToggle>();
		}

		public void ShowInfoBox(){
			buttonColorToggle.SetActiveColor(true);
			infoBox.gameObject.SetActive(true);
			VectorGeometry.SetRectHeight(RectTransform, RectTransform.rect.height+infoBox.rect.height);
		}
		public void HideInfoBox(){
			buttonColorToggle.SetActiveColor(true);
			infoBox.gameObject.SetActive(false);
			RectTransform.sizeDelta = border.sizeDelta;
		}
	}
}