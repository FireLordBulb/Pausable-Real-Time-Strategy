using Mathematics;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

		private void Awake(){
			province = UI.SelectedProvince;
			title.text = $"{province.Terrain.Name}";
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
			valueTable.UpdateColumn(0, Format.SignedPercent, province.Terrain.DevelopmentModifier, province.Terrain.DefenderAdvantage);
			ownerName.text = province.Owner.Name;
			ownerName.ForceMeshUpdate();
			VectorGeometry.SetRectWidth((RectTransform)ownerName.transform, ownerName.textBounds.size.x);
			AddCountryLink(ownerName.gameObject, province.Owner);
			ownerFlag.material = new Material(ownerFlag.material){
				color = province.Owner.MapColor
			};
			DestroyImmediate(ownerFlag.gameObject.GetComponent<UILink>());
			AddCountryLink(ownerFlag.gameObject, province.Owner);
		}
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return RegularProvinceClick(clickedProvince, isRightClick);
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
