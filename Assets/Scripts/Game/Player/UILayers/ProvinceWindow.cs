using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Terrain = Simulation.Terrain;

namespace Player {
	public class ProvinceWindow : UILayer, IRefreshable {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image terrainImage;
		[SerializeField] private GameObject terrainValuesTable;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private GameObject ownerRow;
		[SerializeField] private TextMeshProUGUI ownerName;
		[SerializeField] private Image ownerFlag;
		[SerializeField] private Button close;
		
		private Province province;
		private Country linkedCountry;

		private void Awake(){
			province = UI.SelectedProvince;
			title.text = $"{province}";
			Texture2D texture = (Texture2D)province.Terrain.Material.mainTexture;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			if (province.IsSea){
				terrainValuesTable.SetActive(false);
				ownerRow.gameObject.SetActive(false);
			} else {
				valueTable.Generate(-1, valueNames);
				Refresh();
			}
			province.OnSelect();
			
			close.onClick.AddListener(() => UI.Deselect(province));
		}
		public void Refresh(){
			if (province.IsSea){
				return;
			}
			Terrain terrain = province.Terrain;
			valueTable.UpdateColumn(0, Format.SignedPercent, terrain.DevelopmentModifier, terrain.MoveSpeedModifier, terrain.DefenderAdvantage);
			
			if (linkedCountry == province.Owner){
				return;
			}
			linkedCountry = province.Owner;
			ownerName.text = linkedCountry.Name;
			SetCountryLink(ownerName, linkedCountry);
			ownerFlag.material = new Material(ownerFlag.material){
				color = province.Owner.MapColor
			};
			SetCountryLink(ownerFlag, linkedCountry);
		}
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			return RegularProvinceClick(clickedSelectable, isRightClick);
		}
		public override void OnEnd(){
			province.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedProvince != province;
		}
	}
}
