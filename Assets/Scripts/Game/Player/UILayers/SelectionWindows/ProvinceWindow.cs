using System;
using System.Collections.Generic;
using System.Text;
using Mathematics;
using Simulation;
using Simulation.Military;
using Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Player {
	public class ProvinceWindow : SelectionWindow<Province> {
		private static readonly int TabTypeCount = Enum.GetNames(typeof(TabType)).Length;
		
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Tab[] tabs;
		[SerializeField] private GameObject tabButtons;
		[Header("Terrain")]
		[SerializeField] private TextMeshProUGUI terrainType;
		[SerializeField] private Image terrainImage;
		[SerializeField] private ValueTable terrainValueTable;
		[SerializeField] private string[] terrainValueNames;
		[Header("Production")]
		[SerializeField] private ValueTable productionValueTable;
		[SerializeField] private string[] productionValueNames;
		[Header("War")]
		[SerializeField] private TextMeshProUGUI occupationInfo;
		[SerializeField] private float occupationInfoLineHeight;
		[SerializeField] private RectTransform scrollRect;
		[SerializeField] private UnitListScrollView unitListScrollView;
		[SerializeField] private float warPanelNoOwnerExtraHeight;
		[Space]
		[SerializeField] private CountryPanel countryPanel;
		
		private Country linkedCountry;
		private Tab activeTab;
		private Vector2 defaultScrollRectSizeDelta;
		private IReadOnlyList<IUnit> unitsYesterday;
		private MilitaryUnitInfo[] unitInfoItems;
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			Calendar.OnDayTick.AddListener(RefreshDaily);
			defaultScrollRectSizeDelta = scrollRect.sizeDelta;
			title.text = $"{Selected.Name}";
			for (int i = 0; i < tabs.Length; i++){
				tabs[i].type = (TabType)i;
			}
			if (Selected.IsSea){
				activeTab = tabs[(int)TabType.War];
				activeTab.SetActive(true);
				occupationInfo.text = "";
				tabButtons.SetActive(false);
				countryPanel.gameObject.SetActive(false);
				VectorGeometry.SetRectHeight(activeTab.transform, activeTab.transform.rect.height+warPanelNoOwnerExtraHeight);
				Refresh();
				return;
			}
			Texture2D texture = (Texture2D)Selected.TerrainMaterial.mainTexture;
			terrainImage.type = Image.Type.Tiled;
			terrainImage.pixelsPerUnitMultiplier = texture.width/terrainImage.rectTransform.rect.width;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			terrainType.text = $"<i>Terrain:</i> {Selected.TerrainType}";
			terrainValueTable.Generate(-1, terrainValueNames);
			
			productionValueTable.Generate(0, productionValueNames);
			
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
		
		public void NextTab(){
			tabs[((int)activeTab.type+1)%TabTypeCount].button.onClick.Invoke();
		}
		
		public override void Refresh(){
			if (Selected.IsLand){
				countryPanel.SetCountry(Selected.Land.Owner, UI);
			}
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
		private void RefreshDaily(){
			if (activeTab.type == TabType.War){
				RefreshWar();
			}
		}
		private void RefreshTerrain(){
			terrainValueTable.UpdateColumn<float>(0, Format.SignedPercent, (
				Selected.MoveSpeedMultiplier-1),
				Selected.DefenderDamageMultiplier-1,
				0,
				Selected.GoldMultiplier-1,
				Selected.ManpowerMultiplier-1,
				Selected.SailorsMultiplier-1
			);
			// Leading space before the number is intentional, to take up the same space that the +/- signs do for the SignedPercent values.
			terrainValueTable.UpdateCell(0, 2, $" {Selected.CombatWidth}");
		}
		private void RefreshProduction(){
			Land land = Selected.Land;
			productionValueTable.UpdateColumn(-1, (
				land.GoldProduction.ToString("0.0")),
				land.ManpowerProduction.ToString(),
				land.SailorsProduction.ToString(),
				"",
				(land.Development-1).ToString()
			);
		}
		private void RefreshWar(){
			IReadOnlyList<IUnit> units;
			Vector2 scrollRectSizeDelta = defaultScrollRectSizeDelta;
			if (Selected.IsSea){
				units = Selected.Sea.NavyLocation.Units;
				occupationInfo.text = "";
				scrollRectSizeDelta.y += occupationInfoLineHeight*2;
			} else {
				LandLocation armyLocation = Selected.Land.ArmyLocation;
				units = armyLocation.Units;
				StringBuilder occupationText = new();
				if (Selected.Land.IsOccupied){
					occupationText.Append($"Occupied by {Selected.Land.Occupier}\n");
				} else {
					scrollRectSizeDelta.y += occupationInfoLineHeight;
				}
				if (armyLocation.SiegeIsOngoing){
					occupationText.Append($"Under siege by {armyLocation.Sieger.Name} ({armyLocation.SiegeDaysLeft} days left)");
				} else {
					scrollRectSizeDelta.y += occupationInfoLineHeight;
				}
				occupationInfo.text = occupationText.ToString();
			}
			scrollRect.sizeDelta = scrollRectSizeDelta;
			unitListScrollView.Refresh(units, UI);
		}
		public override void OnEnd(){
			Calendar.OnDayTick.RemoveListener(RefreshDaily);
			base.OnEnd();
		}

		[Serializable]
		private class Tab {
			public RectTransform transform;
			public Button button;
			[NonSerialized] public TabType type;

			public void SetActive(bool isActive){
				transform.gameObject.SetActive(isActive);
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
