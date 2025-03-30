using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryWindow : UILayer {
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
			UpdateValueTable();
			if (UI.PlayerCountry == null){
				select.onClick.AddListener(() => UI.PlayerCountry = country);
			} else {
				select.gameObject.SetActive(false);
			}
			close.onClick.AddListener(() => UI.Deselect(country));
			Calendar.Instance.OnMonthTick.AddListener(UpdateValueTable);
			
			country.OnSelect();
		}

		public void UpdateValueTable(){
			valueTable.UpdateColumn(0, (
				Format.FormatLargeNumber(country.ProvinceCount, Format.SevenDigits)),	
				Format.FormatLargeNumber(country.Gold, Format.FiveDigits),	
				Format.FormatLargeNumber(country.Manpower, Format.SevenDigits),	
				Format.FormatLargeNumber(country.Sailors, Format.SevenDigits)
			);
		}
		
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return RegularProvinceClick(clickedProvince, isRightClick);
		}	
		public override void OnEnd(){
			Calendar.Instance.OnMonthTick.RemoveListener(UpdateValueTable);
			country.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			return UI.SelectedCountry != country;
		}
	}
}
