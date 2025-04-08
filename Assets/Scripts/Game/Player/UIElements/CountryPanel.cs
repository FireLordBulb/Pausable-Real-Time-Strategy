using System;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryPanel : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI countryName;
		[SerializeField] private Image flag;
		
		private Country linkedCountry;
		
		// ReSharper disable Unity.PerformanceAnalysis // The guard clause prevents the performance-intensive part from running every frame.
		public void SetCountry(Country country, Action action = null){
			if (linkedCountry == country){
				return;
			}
			linkedCountry = country;
			countryName.text = linkedCountry.Name;
			UILayer.SetSelectLink(countryName, linkedCountry, action);
			flag.material = new Material(flag.material){
				color = country.MapColor
			};
			UILayer.SetSelectLink(flag, linkedCountry, action);
		}
	}
}