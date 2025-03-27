using System.Text;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class ProvinceWindow : UILayer {
		[SerializeField] private TextMeshProUGUI color;
		[SerializeField] private TextMeshProUGUI terrain;
		[SerializeField] private TextMeshProUGUI developmentModifier;
		[SerializeField] private TextMeshProUGUI defenderAdvantage;
		[SerializeField] private TextMeshProUGUI neighbors;
		[SerializeField] private Image terrainImage;
		[SerializeField] private Button button;
		
		private Province province;
		private bool isDone;
		private void Awake(){
			province = UIStack.Instance.SelectedProvince;
			color.text = $"Color: {province.gameObject.name}";
			if (province.Terrain != null){
				terrain.text = $"Terrain: {province.Terrain.Name}";
				developmentModifier.text = Format.SignedPercent(province.Terrain.DevelopmentModifier);
				defenderAdvantage.text = Format.SignedPercent(province.Terrain.DefenderAdvantage);
				Texture2D texture = (Texture2D)province.Terrain.Material.mainTexture;
				terrainImage.overrideSprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.zero);
			}
			StringBuilder neighborsString = new("Neighbors:");
			foreach (ProvinceLink provinceLink in province.Links){
				neighborsString.Append($"\n{provinceLink.Target.gameObject.name}");
			}
			neighbors.text = neighborsString.ToString();
			province.OnSelect();
			
			button.onClick.AddListener(() => {
				isDone = true;
				Stack.DeselectProvince(province);
			});
		}
		public override void OnUpdate(){
			// TODO: When a tick passes or an action was taken on the province, update window info.
			if (Stack.SelectedProvince == province){
				return;
			}
			isDone = true;
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
