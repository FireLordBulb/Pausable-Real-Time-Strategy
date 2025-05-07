using System;
using System.Collections.Generic;
using System.Linq;
using Mathematics;
using Simulation.Military;
using TMPro;
using UnityEngine;

namespace Player {
	public class UnitListScrollView : MonoBehaviour {
		[SerializeField] private MilitaryUnitInfo militaryUnitInfo;
		[SerializeField] private RectTransform scrollViewContent;
		[SerializeField] private TextMeshProUGUI fallbackText;

		private IReadOnlyList<IUnit> unitsYesterday;
		private MilitaryUnitInfo[] unitInfoItems;
		
		private void Awake(){
			unitsYesterday = new List<IUnit>();
			unitInfoItems = Array.Empty<MilitaryUnitInfo>();
		}
		public void Refresh(IReadOnlyList<IUnit> units, UIStack ui){
			bool haveUnitsChanged = unitsYesterday.Count != units.Count;
			if (!haveUnitsChanged){
				for (int i = 0; i < units.Count; i++){
					if (unitsYesterday[i] == units[i]){
						continue;
					}
					haveUnitsChanged = true;
					break;
				}
			}
			if (haveUnitsChanged){
				foreach (MilitaryUnitInfo unitInfo in unitInfoItems){
					Destroy(unitInfo.gameObject);
				}
				unitInfoItems = new MilitaryUnitInfo[units.Count];
				unitsYesterday = units.ToList();
			}
			if (units.Count == 0){
				fallbackText.gameObject.SetActive(true);
				VectorGeometry.SetRectHeight(scrollViewContent, fallbackText.rectTransform.rect.height);
			} else if (haveUnitsChanged){
				Vector2 anchoredPosition = Vector2.zero;
				Vector2 infoItemHeight = new(0, -militaryUnitInfo.RectTransform.rect.height);
				for (int i = 0; i < units.Count; i++){
					unitInfoItems[i] = Instantiate(militaryUnitInfo, scrollViewContent);
					unitInfoItems[i].Init(units[i], ui);
					unitInfoItems[i].RectTransform.anchoredPosition = anchoredPosition;
					anchoredPosition += infoItemHeight;
				}
				fallbackText.gameObject.SetActive(false);
				VectorGeometry.SetRectHeight(scrollViewContent, -anchoredPosition.y);
			} else {
				foreach (MilitaryUnitInfo unitInfo in unitInfoItems){
					unitInfo.Refresh();
				}
			}
		}
	}
}
