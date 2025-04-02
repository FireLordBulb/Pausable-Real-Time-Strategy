using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(Button), typeof(RectTransform))]
	public class MilitaryUnitButton : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI label;

		public RectTransform RectTransform {get; private set;}
		public Button Button {get; private set;}
		
		public TextMeshProUGUI Label => label;
		private void Awake(){
			RectTransform = (RectTransform)transform;
			Button = GetComponent<Button>();
		}
	}
}