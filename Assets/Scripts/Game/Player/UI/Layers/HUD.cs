using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class HUD : UILayer {
		[SerializeField] private GameObject pauseLabel;
		[SerializeField] private Image countryFlag;
		[SerializeField] private TextMeshProUGUI countryName;
		[SerializeField] private TextMeshProUGUI gold;
		[SerializeField] private TextMeshProUGUI manpower;
		[SerializeField] private TextMeshProUGUI sailors;
		
		public override void OnBegin(bool isFirstTime){
			// TODO: Alter when observer mode is added.
			Debug.Assert(Player != null);
			gameObject.SetActive(true);

			countryFlag.material = new Material(countryFlag.material){
				color = Player.MapColor
			};
			countryName.text = Player.gameObject.name;
			gold.text = "0";
			manpower.text = "0";
			sailors.text = "0";
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
