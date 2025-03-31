using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Calendar = Simulation.Calendar;

namespace Player {
	public class HUD : UILayer {
		[SerializeField] private GameObject pauseLabel;
		[SerializeField] private Image countryFlag;
		[SerializeField] private TextMeshProUGUI countryName;
		[SerializeField] private TextMeshProUGUI gold;
		[SerializeField] private TextMeshProUGUI manpower;
		[SerializeField] private TextMeshProUGUI sailors;
		
		public override void OnBegin(bool isFirstTime){
			gameObject.SetActive(true);
			RefreshCountry();
		}
		public void RefreshCountry(){
			if (Player == null){
				countryFlag.material = new Material(countryFlag.material){
					color = Color.magenta
				};
				countryName.text = "<i>Observing</i>";
				sailors.text = manpower.text = gold.text = "N/A";
				return;
			}
			countryFlag.material = new Material(countryFlag.material){
				color = Player.MapColor
			};
			countryName.text = Player.gameObject.name;
			RefreshResources();
		}
		private void RefreshResources(){
			gold.text = Format.FormatLargeNumber(Player.Gold, Format.FiveDigits);
			manpower.text = Format.FormatLargeNumber(Player.Manpower, Format.SevenDigits);
			sailors.text = Format.FormatLargeNumber(Player.Sailors, Format.SevenDigits);
		}
		private void Start(){
			Calendar.Instance.OnPauseToggle.AddListener(pauseLabel.SetActive);
			pauseLabel.SetActive(Calendar.Instance.IsPaused);
		}
		private void Update(){
			if (Player == null){
				return;
			}
			if (Player.IsDirty){
				RefreshResources();
				Player.MarkClean();
			}
		}
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return RegularProvinceClick(clickedProvince, isRightClick);
		}
		public override bool IsDone(){
			return false;
		}
	}
}
