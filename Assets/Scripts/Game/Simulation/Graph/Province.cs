using System;
using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation {
    [RequireComponent(typeof(MeshCollider))]
    public class Province : MonoBehaviour, IPositionNode<ProvinceLink, Province>, ISelectable {
        private const float OneThird = 1/3f;
        [SerializeField] private MeshFilter outlineMeshFilter;
        [SerializeField] private MeshFilter shapeMeshFilter;
        [SerializeField] private MeshRenderer shapeMeshRenderer;
        
        private MeshCollider meshCollider;
        private readonly Dictionary<Color32, ProvinceLink> links = new();
        private Type type;
        private Color baseColor;
        private Color hoverColor;
        private Color selectedColor;
        private bool isHovered;
        private bool isSelected;
        
        [NonSerialized] public List<int> TriPointIndices;
        [NonSerialized] public List<Vector2> Vertices;
        [NonSerialized] public readonly List<(int startIndex, int endIndex, ProvinceLink link)> OutlineSegments = new();
        
        
        public Color32 ColorKey {get; private set;}
        public MapGraph Graph {get; private set;}
        public Terrain Terrain {get; private set;}
        public string Name {get; private set;}
        public Vector2 MapPosition {get; private set;}
        public Land Land {get; private set;}
        public Sea Sea {get; private set;}
        
        internal Color BaseColor { set {
            float alpha = baseColor.a;
            baseColor = value;
            baseColor.a = alpha;
            UpdateColors();
        }}
        public float Alpha { set {
            baseColor.a = value;
            UpdateColors();
        }}
        
        public Calendar Calendar => Graph.Calendar;
        public MeshRenderer MeshRenderer => shapeMeshRenderer;
        public Vector3 WorldPosition => transform.position;
        public bool IsSea => type == Type.Sea;
        public bool IsCoast => type == Type.Coast;
        public bool IsLand => type is Type.LandLocked or Type.Coast;
        
        public IEnumerable<ProvinceLink> Links => links.Values;
        public ProvinceLink this[Color32 color] => links[color];
        private Color SolidMaterialColor {
            set => shapeMeshRenderer.sharedMaterials[1].color = value;
        }

        private void Awake(){
            meshCollider = GetComponent<MeshCollider>();
            Land = GetComponent<Land>();
            Sea = GetComponent<Sea>();
            Debug.Assert(Land == null ^ Sea == null, $"FATAL: Province {gameObject.name} is both land and sea, or neither!");
            type = Land == null ? Type.Sea : Type.LandLocked;
        }
        public void Init(Color32 colorKey, MapGraph mapGraph, Terrain terrain, Color mapColor, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
            ColorKey = colorKey;
            gameObject.name = $"R:{colorKey.r}, G:{colorKey.g}, B:{colorKey.b}";
            Graph = mapGraph;
            mapGraph.Add(this);
            
            Terrain = terrain;
            Name = $"Rural {Terrain.Name}";
            baseColor = mapColor;
            
            shapeMeshRenderer.materials[1].color = baseColor;
            shapeMeshRenderer.sharedMaterial = terrain.Material;
            UpdateColors();

            MapPosition = mapPosition;
            outlineMeshFilter.sharedMesh = outlineMesh;
            meshCollider.sharedMesh = shapeMesh;
            shapeMeshFilter.sharedMesh = shapeMesh;
        }
        public void AddNeighbor(Province neighbor, int triPointIndex){
            ProvinceLink newLink;
            if (neighbor != null){
                if (type == Type.LandLocked && neighbor.type == Type.Sea){
                    type = Type.Coast;
                }
                if (type == Type.Sea){
                    if (neighbor.type == Type.Sea){
                        newLink = new SeaLink(this, neighbor, OutlineSegments.Count);
                    } else {
                        newLink = new CoastLink(this, neighbor, OutlineSegments.Count);
                    }
                } else {
                    if (neighbor.type == Type.Sea){
                        newLink = new ShallowsLink(this, neighbor, OutlineSegments.Count);
                    } else {
                        newLink = new LandLink(this, neighbor, OutlineSegments.Count);
                    }
                }
                links.Add(neighbor.ColorKey, newLink);
            } else {
                newLink = null;
            }
            int previousTriPointIndex = OutlineSegments.Count == 0 ? -1 : OutlineSegments[^1].endIndex;
            OutlineSegments.Add((previousTriPointIndex, triPointIndex, newLink));
        }
        public void CompleteSegmentLoop(){
            (int _, int endIndex, ProvinceLink link) = OutlineSegments[0];
            OutlineSegments[0] = (OutlineSegments[^1].endIndex, endIndex, link);
        }
        
        private void UpdateColors(){
            float increasedBrightness = OneThird*(baseColor.grayscale+2);
            selectedColor = 0.5f*(baseColor+new Color(increasedBrightness, increasedBrightness, increasedBrightness));
            hoverColor = 0.5f*(baseColor+Color.gray);
            SolidMaterialColor = isSelected ? selectedColor : isHovered ? hoverColor : baseColor;
        }
        
        public void OnHoverEnter(){
            isHovered = true;
            if (!isSelected){
                SolidMaterialColor = hoverColor;
            }
        }
        public void OnHoverLeave(){
            isHovered = false;
            if (!isSelected){
                SolidMaterialColor = baseColor;
            }
        }
        public void OnSelect(){
            isSelected = true;
            SolidMaterialColor = selectedColor;
        }
        public void OnDeselect(){
            isSelected = false;
            SolidMaterialColor = isHovered ? hoverColor : baseColor;
        }
        
        public override string ToString(){
            return $"{Name} ({gameObject.name})";
        }

        private enum Type {
            Sea,
            LandLocked,
            Coast
        }
    }
}
