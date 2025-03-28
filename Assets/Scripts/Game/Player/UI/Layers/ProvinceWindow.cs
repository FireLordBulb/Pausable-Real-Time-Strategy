using System.Text;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class ProvinceWindow : UILayer {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image terrainImage;
		[SerializeField] private GameObject terrainValuesTable;
		[SerializeField] private TextMeshProUGUI developmentModifier;
		[SerializeField] private TextMeshProUGUI defenderAdvantage;
		[SerializeField] private TextMeshProUGUI owner;
		[SerializeField] private Image ownerFlag;
		[SerializeField] private Button button;
		
		private Province province;
		private bool isDone;
		private void Awake(){
			province = UI.SelectedProvince;
			title.text = $"{province.Terrain.Name}";
			Texture2D texture = (Texture2D)province.Terrain.Material.mainTexture;
			terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			if (province.IsSea){
				terrainValuesTable.SetActive(false);
				owner.gameObject.SetActive(false);
			} else {
				developmentModifier.text = Format.SignedPercent(province.Terrain.DevelopmentModifier);
				defenderAdvantage.text = Format.SignedPercent(province.Terrain.DefenderAdvantage);
				owner.text = $"Part of {province.Owner.Name}";
				ownerFlag.material = new Material(ownerFlag.material){
					color = province.Owner.MapColor
				};
			}
			province.OnSelect();
			
			button.onClick.AddListener(() => {
				isDone = true;
				UI.DeselectProvince(province);
			});
		}
		public override void OnUpdate(){
			// TODO: When a tick passes or an action was taken on the province, update window info.
			if (UI.SelectedProvince == province){
				return;
			}
			isDone = true;
		}
		public override void OnProvinceSelected(){
			// Delay the push until right after the end of OnUpdate so the current window can remove itself first.
			UI.DelayedPush(UI.ProvinceWindow);
		}
		public override void OnEnd(){
			province.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			return isDone;
		}
	}
}
