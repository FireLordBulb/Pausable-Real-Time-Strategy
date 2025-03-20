using System.Text;
using TMPro;
using UnityEngine;

public class ProvinceWindow : UILayer {
	[SerializeField] private TextMeshProUGUI color;
	[SerializeField] private TextMeshProUGUI neighbors;
	
	private Province province;
	private bool isDone;
	public override void OnBegin(bool isFirstTime){
		base.OnBegin(isFirstTime);
		province = Stack.SelectedProvince;
		color.text = $"Color: {province.gameObject.name}";
		StringBuilder neighborsString = new("Neighbors:");
		foreach (ProvinceLink provinceLink in province.Links){
			neighborsString.Append($"\n{provinceLink.Target.gameObject.name}");
		}
		neighbors.text = neighborsString.ToString();
		province.OnSelect();
	}
	public override void OnUpdate(){
		// TODO: When a tick passes or an action was taken on the province, update window info.
		if (Stack.SelectedProvince != province){
			isDone = true;
		}
	}
	public override void OnEnd(){
		province.OnDeselect();
		base.OnEnd();
	}
	public override bool IsDone(){
		return isDone;
	}
}
