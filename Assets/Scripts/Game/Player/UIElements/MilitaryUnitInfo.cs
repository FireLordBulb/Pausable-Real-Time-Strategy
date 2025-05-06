using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(RectTransform))]
	public class MilitaryUnitInfo : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI healthPoints;
		[SerializeField] private Image flag;
		public RectTransform RectTransform => (RectTransform)transform;
		
		public void Init(IUnit unit){
			healthPoints.text = unit.HpText;
			flag.material = new Material(flag.material){
				color = unit.Owner.MapColor
			};
		}
	}
}