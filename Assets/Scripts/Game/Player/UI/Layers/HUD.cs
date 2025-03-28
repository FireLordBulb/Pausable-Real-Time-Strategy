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

		private bool doRefreshNextFrame;
		private bool doRefreshThisFrame;
		
		public override void OnBegin(bool isFirstTime){
			// TODO: Alter when observer mode is added.
			Debug.Assert(Player != null);
			gameObject.SetActive(true);

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
			Calendar.Instance.OnMonthTick.AddListener(() => {
				if (Player.IsDirty){
					doRefreshNextFrame = true;
					Player.MarkClean();
				}
			});
			Calendar.Instance.OnPauseToggle.AddListener(pauseLabel.SetActive);
			pauseLabel.SetActive(Calendar.Instance.IsPaused);
		}
		public override void OnUpdate(){
			if (doRefreshNextFrame){
				doRefreshNextFrame = false;
				doRefreshThisFrame = true;
			} else if (doRefreshThisFrame){
				doRefreshThisFrame = false;
				RefreshResources();
			}
		}
		public override void OnProvinceSelected(){
			UI.Push(UI.ProvinceWindow);
		}
	}
}
