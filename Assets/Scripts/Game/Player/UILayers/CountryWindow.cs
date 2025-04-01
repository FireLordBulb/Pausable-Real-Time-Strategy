using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryWindow : UILayer, IRefreshable {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image flag;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private Button select;
		[SerializeField] private Button close;
		
		private Country country;

		private void Awake(){
			country = UI.SelectedCountry;
			title.text = $"{country.Name}";
			flag.material = new Material(flag.material){
				color = country.MapColor
			};
			
			valueTable.Generate(-1, valueNames);
			Refresh();
			if (UI.PlayerCountry == null || UI.CanSwitchCountry){
				select.onClick.AddListener(() => {
					UI.PlayAs(country);
					UI.Deselect();
					UI.ClearSelectHistory();
				});
			} else {
				select.gameObject.SetActive(false);
			}
			close.onClick.AddListener(() => UI.Deselect(country));
			Calendar.Instance.OnMonthTick.AddListener(Refresh);
			
			country.OnSelect();
		}

		public void Refresh(){
			valueTable.UpdateColumn(0, (
				Format.FormatLargeNumber(country.ProvinceCount, Format.SevenDigits)),	
				Format.FormatLargeNumber(country.Gold, Format.FiveDigits),	
				Format.FormatLargeNumber(country.Manpower, Format.SevenDigits),	
				Format.FormatLargeNumber(country.Sailors, Format.SevenDigits)
			);
		}
		
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			return LayerBelow.OnSelectableClicked(clickedSelectable, isRightClick);
		}	
		public override void OnEnd(){
			Calendar.Instance.OnMonthTick.RemoveListener(Refresh);
			country.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedCountry != country;
		}
	}
}
