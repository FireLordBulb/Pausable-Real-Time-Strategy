using System;
using Graphs;
using UnityEngine;

namespace Simulation {
	public abstract class ProvinceLink : IDistanceLink<Province, ProvinceLink> {
		public float Distance {get;}
		public Province Source {get;}
		public Province Target {get;}
		public int SegmentIndex {get;}
		public Vector3 WorldPosition {get; private set;}
		
		public ProvinceLink Reverse => Target[Source.ColorKey];
		
		internal static ProvinceLink Create(Province source, Province target, int segmentIndex){
			if (source.IsSea){
				if (target.IsSea){
					return new SeaLink(source, target, segmentIndex);
				}
				return new CoastLink(source, target, segmentIndex);
			}
			if (target.IsSea){
				return new ShallowsLink(source, target, segmentIndex);
			}
			return new LandLink(source, target, segmentIndex);
		}
		protected ProvinceLink(Province source, Province target, int segmentIndex){
			Source = source;
			Target = target;
			Distance = Vector2.Distance(source.MapPosition, target.MapPosition);
			SegmentIndex = segmentIndex;
		}

		// Call right after Source's OutlineSegment for this link has its start- & endIndexes set. 
		internal void Init(Func<Vector2, Vector3> worldSpaceConverter){
			(int startIndex, int endIndex, _) = Source.OutlineSegments[SegmentIndex];
			float segmentLength = 0;
			int nextIndex;
			for (int index = startIndex; index != endIndex; index = nextIndex){
				nextIndex = (index+1)%Source.Vertices.Count;
				segmentLength += Vector2.Distance(Source.Vertices[index], Source.Vertices[nextIndex]);
			}
			float lengthUntilCenter = segmentLength*0.5f;
			for (int index = startIndex; index != endIndex; index = nextIndex){
				nextIndex = (index+1)%Source.Vertices.Count;
				Vector2 startVertex = Source.Vertices[index];
				Vector2 endVertex = Source.Vertices[nextIndex];
				float lineLength = Vector2.Distance(startVertex, endVertex);
				if (lengthUntilCenter > lineLength){
					lengthUntilCenter -= lineLength;
				} else {
					Vector2 mapPosition = Source.MapPosition+Vector2.MoveTowards(startVertex, endVertex, lengthUntilCenter);
					WorldPosition = worldSpaceConverter(mapPosition);
					break;
				}
			}
		}
	}
}
