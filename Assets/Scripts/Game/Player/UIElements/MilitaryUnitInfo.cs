using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(RectTransform), typeof(Button))]
	public class MilitaryUnitInfo : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI healthPoints;
		[SerializeField] private Image flag;
		public RectTransform RectTransform => (RectTransform)transform;
		
		public void Init(IUnit unit, UIStack uiStack){
			healthPoints.text = unit.HpText;
			flag.material = new Material(flag.material){
				color = unit.Owner.MapColor
			};
			uiStack.Links.LinkButton(GetComponent<Button>(), unit);
		}
	}
}
