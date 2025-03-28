using Simulation;
using UnityEngine;

namespace Player {
	public class HUD : UILayer {
		[SerializeField] private GameObject pauseLabel;
		
		private void Start(){
			Calendar.Instance.OnPauseToggle.AddListener(pauseLabel.SetActive);
		}
	}
}
