using Simulation;
using UnityEngine;

namespace Player {
	public class HUD : UILayer {
		[SerializeField] private GameObject pauseLabel;
		
		public override void OnBegin(bool isFirstTime){
			Debug.Assert(UI.PlayerCountry != null);
			gameObject.SetActive(true);
		}
		private void Start(){
			Calendar.Instance.OnPauseToggle.AddListener(pauseLabel.SetActive);
			pauseLabel.SetActive(Calendar.Instance.IsPaused);
		}
		public override void OnProvinceSelected(){
			UI.Push(UI.ProvinceWindow);
		}
	}
}
