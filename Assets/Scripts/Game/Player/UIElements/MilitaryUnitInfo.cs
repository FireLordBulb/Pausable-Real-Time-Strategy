using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(RectTransform), typeof(Button))]
	public class MilitaryUnitInfo : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI healthPoints;
		[SerializeField] private Image flag;
		[SerializeField] private GameObject defendingIcon;
		[SerializeField] private GameObject attackingIcon;
		
		private IUnit unit;
		
		public RectTransform RectTransform => (RectTransform)transform;
		
		public void Init(IUnit unitReference, UIStack uiStack){
			unit = unitReference;
			flag.material = new Material(flag.material){
				color = unit.Owner.MapColor
			};
			uiStack.Links.LinkButton(GetComponent<Button>(), unit);
			Refresh();
		}
		public void Refresh(){
			healthPoints.text = unit.HpText;
			if (unit.BattleSide == BattleSide.Defending){
				defendingIcon.SetActive(true);
				attackingIcon.SetActive(false);
			} else if (unit.BattleSide == BattleSide.Attacking){
				defendingIcon.SetActive(false);
				attackingIcon.SetActive(true);
			} else {
				defendingIcon.SetActive(false);
				attackingIcon.SetActive(false);
			}
		}
	}
}
