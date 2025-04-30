using UnityEngine;

namespace Simulation {
	[CreateAssetMenu(fileName = "Terrain", menuName = "ScriptableObjects/Terrain")]
	public class Terrain : ScriptableObject {
		[SerializeField] private new string name;
		[SerializeField] private Material material;
		[SerializeField] private float goldModifier;
		[SerializeField] private float manpowerModifier;
		[SerializeField] private float sailorsModifier;
		[SerializeField] private float moveSpeedModifier;
		[SerializeField] private float defenderAdvantage;
		[SerializeField] private int combatWidth;
		public string Name => name;
		public Material Material => material;
		public float GoldMultiplier => 1+goldModifier;
		public float ManpowerMultiplier => 1+manpowerModifier;
		public float SailorsMultiplier => 1+sailorsModifier;
		public float MoveSpeedMultiplier => 1+moveSpeedModifier;
		public float DefenderDamageMultiplier => 1+defenderAdvantage;
		public int CombatWidth => combatWidth;
	}
}
