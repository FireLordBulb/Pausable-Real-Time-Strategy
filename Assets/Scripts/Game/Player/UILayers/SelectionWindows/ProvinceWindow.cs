using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Terrain = Simulation.Terrain;

namespace Player {
	public class ProvinceWindow : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image terrainImage;
		[SerializeField] private GameObject terrainValuesTable;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private CountryPanel countryPanel;
		
		private Province province;
		private Country linkedCountry;

		public override void OnBegin(bool isFirstTime){
			if (!isFirstTime){
				return;
			}
			province = UI.SelectedProvince;
			title.text = $"{province}";
			Texture2D texture = (Texture2D)province.Terrain.Material.mainTexture;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			if (province.IsSea){
				terrainValuesTable.SetActive(false);
				countryPanel.gameObject.SetActive(false);
			} else {
				valueTable.Generate(-1, valueNames);
				Refresh();
			}
			province.OnSelect();
		}
		public void Refresh(){
			if (province.IsSea){
				return;
			}
			Terrain terrain = province.Terrain;
			valueTable.UpdateColumn(0, Format.SignedPercent, terrain.DevelopmentModifier, terrain.MoveSpeedModifier, terrain.DefenderAdvantage);
			countryPanel.SetCountry(province.Land.Owner, UI);
		}
		public override void OnEnd(){
			province.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedProvince != province;
		}
		public void Close(){
			UI.Deselect(province);
		}
	}
}
