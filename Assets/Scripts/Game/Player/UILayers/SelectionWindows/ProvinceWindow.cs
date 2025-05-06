using System;
using Simulation;
using Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class ProvinceWindow : SelectionWindow<Province> {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Tab[] tabs;
		[SerializeField] private GameObject tabButtons;
		[SerializeField] private TextMeshProUGUI terrainType;
		[SerializeField] private Image terrainImage;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private CountryPanel countryPanel;
		
		private Country linkedCountry;
		private Tab activeTab;
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			title.text = $"{Selected.Name}";
			for (int i = 0; i < tabs.Length; i++){
				tabs[i].type = (TabType)i;
			}
			if (Selected.IsSea){
				activeTab = tabs[(int)TabType.War];
				activeTab.SetActive(true);
				tabButtons.SetActive(false);
				countryPanel.gameObject.SetActive(false);
				return;
			}
			Texture2D texture = (Texture2D)Selected.TerrainMaterial.mainTexture;
			terrainImage.type = Image.Type.Tiled;
			terrainImage.pixelsPerUnitMultiplier = texture.width/terrainImage.rectTransform.rect.width;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			terrainType.text = $"<i>Terrain:</i> {Selected.TerrainType}";
			valueTable.Generate(-1, valueNames);
			
			foreach (Tab tab in tabs){
				tab.SetActive(false);
				tab.button.onClick.AddListener(() => {
					UI.ProvinceTab = tab.type;
					activeTab.SetActive(false);
					activeTab = tab;
					activeTab.SetActive(true);
					Refresh();
				});
			}
			// Prevent activeTab from being null before the first onClick is Invoked.
			activeTab = tabs[0];
			Button activeButton = tabs[(int)UI.ProvinceTab].button;
			ColorBlock colorBlock = activeButton.colors;
			ColorBlock instantFade = colorBlock;
			instantFade.fadeDuration = 0;
			activeButton.colors = instantFade;
			activeButton.onClick.Invoke();
			activeButton.colors = colorBlock;
			
			Refresh();
		}
		
		public override void Refresh(){
			switch(activeTab.type){
				case TabType.Terrain:
					RefreshTerrain();
					break;
				case TabType.Production:
					RefreshProduction();
					break;
				case TabType.War:
					RefreshWar();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(UI.ProvinceTab), UI.ProvinceTab, "Invalid int was cast to TabType enum!");
			}
		}
		private void RefreshTerrain(){
			valueTable.UpdateColumn<float>(0, Format.SignedPercent, (
				Selected.MoveSpeedMultiplier-1),
				Selected.DefenderDamageMultiplier-1,
				0,
				Selected.GoldMultiplier-1,
				Selected.ManpowerMultiplier-1,
				Selected.SailorsMultiplier-1
			);
			// Leading space before the number is intentional, to take up the same space that the +/- signs do for the SignedPercent values.
			valueTable.UpdateCell(0, 2, $" {Selected.CombatWidth}");
			countryPanel.SetCountry(Selected.Land.Owner, UI);
		}
		private void RefreshProduction(){
			
		}
		private void RefreshWar(){
			
		}
		
		[Serializable]
		private class Tab {
			public GameObject panel;
			public Button button;
			[NonSerialized] public TabType type;

			public void SetActive(bool isActive){
				panel.SetActive(isActive);
				button.interactable = !isActive;
			}
		}
		
		public enum TabType {
			Terrain,
			Production,
			War
		}
	}
}
