using UnityEngine;
using UnityEngine.Events;

public class Calendar : MonoBehaviour {
	public static Calendar Instance;
	
	private const float NoProgress = 0;
	private const float FullProgress = 1;
	
	[SerializeField] public float[] speedTimeSteps;
	[SerializeField] public int startingSpeed;
	[SerializeField] private Date startDate;
	
	private Date currentDate;
	private float speed;
	private float tickProgress;

	private int speedIndex;
	public int SpeedIndex {
		get => speedIndex;
		set {
			speedIndex = value;
			speed = 1/speedTimeSteps[startingSpeed];
		}
	}
	public bool IsPaused {get; set;}
	
	public UnityEvent OnDayTick => currentDate.OnDayTick;
	public UnityEvent OnMonthTick => currentDate.OnMonthTick;
	public UnityEvent OnYearTick => currentDate.OnYearTick;
	public string Date => currentDate.ToString();
	
	private void Awake(){
		if (Instance != null){
			Destroy(gameObject);
		}
		Instance = this;
		IsPaused = false;
		currentDate = new Date(startDate);
		tickProgress = NoProgress;
		SpeedIndex = startingSpeed;
	}
	private void Update(){
		if (IsPaused){
			return;
		}
		tickProgress += Time.deltaTime*speed;
		if (tickProgress < FullProgress){
			return;
		}
		tickProgress = NoProgress;
		currentDate.ToNextDay();
	}

	public void ChangeSpeed(int change){
		SpeedIndex = Mathf.Clamp(SpeedIndex+change, 0, speedTimeSteps.Length-1);
	}
}