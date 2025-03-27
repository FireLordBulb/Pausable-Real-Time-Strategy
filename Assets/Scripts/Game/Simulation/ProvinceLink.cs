using Graphs;
using UnityEngine;

public class ProvinceLink : IDistanceLink<Province, ProvinceLink> {
	public float Distance {get;}
	public Province Source {get;}
	public Province Target {get;}
	public int SegmentIndex {get;}
	public ProvinceLink(Province source, Province target, int segmentIndex){
		Source = source;
		Target = target;
		Distance = Vector2.Distance(source.MapPosition, target.MapPosition);
		SegmentIndex = segmentIndex;
	}
}