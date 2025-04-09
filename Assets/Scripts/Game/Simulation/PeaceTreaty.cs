using System.Collections.Generic;
using UnityEngine;

namespace Simulation {
	public class PeaceTreaty {
		public readonly HashSet<Land> AnnexedLands = new();
		public float GoldTransfer;
		public bool DidTreatyInitiatorWin;
		public bool IsWhitePeace;
		
		private readonly Country initiator;
		private readonly Country recipient;
		private readonly TruceData truceData;
		
		public Country Loser => DidTreatyInitiatorWin ? recipient : initiator;
		public Country Winner => DidTreatyInitiatorWin ? initiator : recipient;
		
		public int TruceLength {
			get {
				float sum = 0;
				sum += truceData.DaysPerProvince*AnnexedLands.Count;
				foreach (Land annexedLand in AnnexedLands){
					sum += truceData.DaysPerDevelopment*(1+annexedLand.Terrain.DevelopmentModifier);
				}
				sum += truceData.DaysPerGold*GoldTransfer;
				return Mathf.Max((int)sum, truceData.MinTruceLength);
			}
		}
		
		internal PeaceTreaty(Country initiatingCountry, Country receivingCountry, TruceData truceLengthData){
			initiator = initiatingCountry;
			recipient = receivingCountry;
			truceData = truceLengthData;
			IsWhitePeace = true;
		}
		
		internal void Apply(){
			if (IsWhitePeace){
				return;
			}
			(Country winner, Country loser) = DidTreatyInitiatorWin ? (initiator, recipient) : (recipient, initiator);
			float actualGoldTransfer = Mathf.Min(GoldTransfer, loser.Gold);
			loser.GainResources(-actualGoldTransfer, 0, 0);
			winner.GainResources(+actualGoldTransfer, 0, 0);
			foreach (Land annexedLand in AnnexedLands){
				// Note that a country can give up land occupied by a third party in a peace deal, but it stays occupied by the third party.
				if (annexedLand.Owner == loser){
					annexedLand.Owner = winner;
				}
			}
		}
	}
}