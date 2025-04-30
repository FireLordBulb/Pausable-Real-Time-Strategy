using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class ProvinceWindow : SelectionWindow<Province> {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private GameObject terrainTab;
		[SerializeField] private TextMeshProUGUI terrainType;
		[SerializeField] private Image terrainImage;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private CountryPanel countryPanel;
		
		private Country linkedCountry;

		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			title.text = $"{Selected.Name}";
			Texture2D texture = (Texture2D)Selected.TerrainMaterial.mainTexture;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			if (Selected.IsSea){
				terrainTab.SetActive(false);
				countryPanel.gameObject.SetActive(false);
			} else {
				terrainType.text = $"<i>Terrain:</i> {Selected.TerrainType}";
				valueTable.Generate(-1, valueNames);
				Refresh();
			}
		}
		public override void Refresh(){
			if (Selected.IsSea){
				return;
			}
			valueTable.UpdateColumn(0, Format.SignedPercent, Selected.GoldMultiplier-1, Selected.ManpowerMultiplier-1, Selected.SailorsMultiplier-1, Selected.MoveSpeedMultiplier-1, Selected.DefenderDamageMultiplier-1);
			countryPanel.SetCountry(Selected.Land.Owner, UI);
		}
	}
}
