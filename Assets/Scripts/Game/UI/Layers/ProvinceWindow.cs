public class ProvinceWindow : UILayer {
	private Province province;
	private bool isDone;
	public override void OnBegin(bool isFirstTime){
		base.OnBegin(isFirstTime);
		province = Stack.SelectedProvince;
		gameObject.name = province.gameObject.name;
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
