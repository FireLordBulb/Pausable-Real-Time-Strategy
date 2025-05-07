using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class ButtonColorToggle : MonoBehaviour {
		[SerializeField] private Button button;
		[SerializeField] private Color activeTint;

		private Color defaultColor;
		private Color activeColor;

		public Button Button => button;
		
		private void Awake(){
			defaultColor = button.image.color;
			activeColor = defaultColor*activeTint;
		}

		public void SetActiveColor(bool isActive){
			button.image.color = isActive ? activeColor : defaultColor;
		}
	}
}