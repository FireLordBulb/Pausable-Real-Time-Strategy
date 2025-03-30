using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class ProvinceWindow : UILayer {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image terrainImage;
		[SerializeField] private GameObject terrainValuesTable;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private TextMeshProUGUI owner;
		[SerializeField] private Image ownerFlag;
		[SerializeField] private Button button;
		
		private Province province;

		private void Awake(){
			province = UI.SelectedProvince;
			title.text = $"{province.Terrain.Name}";
			Texture2D texture = (Texture2D)province.Terrain.Material.mainTexture;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			if (province.IsSea){
				terrainValuesTable.SetActive(false);
				owner.gameObject.SetActive(false);
			} else {
				valueTable.Generate(-1, valueNames);
				valueTable.UpdateColumn(0, Format.SignedPercent, province.Terrain.DevelopmentModifier, province.Terrain.DefenderAdvantage);
				owner.text = $"Part of {province.Owner.Name}";
				ownerFlag.material = new Material(ownerFlag.material){
					color = province.Owner.MapColor
				};
			}
			province.OnSelect();
			
			button.onClick.AddListener(() => UI.Deselect(province));
		}
		public override void OnUpdate(){
			// TODO: When a tick passes or an action was taken on the province, update window info.
		}
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return RegularProvinceClick(clickedProvince, isRightClick);
		}
		public override void OnEnd(){
			province.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			return UI.SelectedProvince != province;
		}
	}
}
