using System;
using Graphs;
using UnityEngine;

namespace Simulation {
	public class ProvinceLink : IDistanceLink<Province, ProvinceLink> {
		private readonly int segmentIndex;
		
		public float Distance {get;}
		public Province Source {get;}
		public Province Target {get;}
		public int StartIndex {get;}
		public int EndIndex {get;}
		public Vector2 MapPosition {get; private set;}
		public Vector3 WorldPosition {get; private set;}
		
		public ProvinceLink Reverse => Target[Source.ColorKey];
		public ProvinceLink Next => Source.LinkList[(segmentIndex+1)%Source.LinkList.Count];
		public ProvinceLink Previous => Source.LinkList[(segmentIndex-1+Source.LinkList.Count)%Source.LinkList.Count];
		
		internal static ProvinceLink Create(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter){
			if (target == null){
				return new ProvinceLink(source, startIndex, endIndex, worldSpaceConverter);
			}
			if (source.IsSea){
				if (target.IsSea){
					return new SeaLink(source, target, startIndex, endIndex, worldSpaceConverter);
				}
				return new CoastLink(source, target, startIndex, endIndex, worldSpaceConverter);
			}
			if (target.IsSea){
				return new ShallowsLink(source, target, startIndex, endIndex, worldSpaceConverter);
			}
			return new LandLink(source, target, startIndex, endIndex, worldSpaceConverter);
		}
		protected ProvinceLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : this(source, startIndex, endIndex, worldSpaceConverter){
			Target = target;
			Distance = Vector2.Distance(source.MapPosition, MapPosition)+Vector2.Distance(MapPosition, target.MapPosition);
		}
		private ProvinceLink(Province source, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter){
			segmentIndex = source.LinkList.Count;
			Source = source;
			StartIndex = startIndex;
			EndIndex = endIndex;
			CalculateWorldPosition(worldSpaceConverter);
		}

		// Call right after Source's OutlineSegment for this link has its start- & endIndexes set. 
		private void CalculateWorldPosition(Func<Vector2, Vector3> worldSpaceConverter){
			float segmentLength = 0;
			int nextIndex;
			for (int index = StartIndex; index != EndIndex; index = nextIndex){
				nextIndex = (index+1)%Source.Vertices.Count;
				segmentLength += Vector2.Distance(Source.Vertices[index], Source.Vertices[nextIndex]);
			}
			float lengthUntilCenter = segmentLength*0.5f;
			for (int index = StartIndex; index != EndIndex; index = nextIndex){
				nextIndex = (index+1)%Source.Vertices.Count;
				Vector2 startVertex = Source.Vertices[index];
				Vector2 endVertex = Source.Vertices[nextIndex];
				float lineLength = Vector2.Distance(startVertex, endVertex);
				if (lengthUntilCenter > lineLength){
					lengthUntilCenter -= lineLength;
				} else {
					MapPosition = Source.MapPosition+Vector2.MoveTowards(startVertex, endVertex, lengthUntilCenter);
					WorldPosition = worldSpaceConverter(MapPosition);
					break;
				}
			}
		}
	}
}
