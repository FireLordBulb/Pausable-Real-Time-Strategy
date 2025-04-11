using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Terrain = Simulation.Terrain;

namespace Player {
	public class ProvinceWindow : SelectionWindow<Province> {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image terrainImage;
		[SerializeField] private GameObject terrainValuesTable;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private CountryPanel countryPanel;
		
		private Country linkedCountry;

		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			title.text = $"{Selected}";
			Texture2D texture = (Texture2D)Selected.Terrain.Material.mainTexture;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			if (Selected.IsSea){
				terrainValuesTable.SetActive(false);
				countryPanel.gameObject.SetActive(false);
			} else {
				valueTable.Generate(-1, valueNames);
				Refresh();
			}
		}
		public override void Refresh(){
			if (Selected.IsSea){
				return;
			}
			Terrain terrain = Selected.Terrain;
			valueTable.UpdateColumn(0, Format.SignedPercent, terrain.DevelopmentModifier, terrain.MoveSpeedModifier, terrain.DefenderAdvantage);
			countryPanel.SetCountry(Selected.Land.Owner, UI);
		}
	}
}
