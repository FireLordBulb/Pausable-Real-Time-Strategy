using System;
using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation {
    [RequireComponent(typeof(MeshCollider))]
    public class Province : MonoBehaviour, IPositionNode<ProvinceLink, Province>, ISelectable {
        private const float OneThird = 1/3f;
        [SerializeField] private MeshFilter outlineMeshFilter;
        [SerializeField] private MeshRenderer outlineMeshRenderer;
        [SerializeField] private MeshFilter shapeMeshFilter;
        [SerializeField] private MeshRenderer shapeMeshRenderer;
        
        private MeshCollider meshCollider;
        private Terrain terrain;
        private readonly Dictionary<Color32, ProvinceLink> linkMap = new();
        private readonly List<ProvinceLink> linkList = new();
        private readonly List<Vector2> vertexList = new();
        private Type type;
        private Color baseColor;
        private Color hoverColor;
        private Color selectedColor;
        private Color hoverSelectedColor;
        private bool isHovered;
        private bool isSelected;
        
        public Color32 ColorKey {get; private set;}
        public MapGraph Graph {get; private set;}
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
        public Bounds Bounds => meshCollider.bounds;
        public string TerrainType => $"{terrain.Name} {(IsCoast ? "(Coastal)" : "(Landlocked)")}";
        public Material TerrainMaterial => terrain.Material;
        public float GoldMultiplier => terrain.GoldMultiplier;
        public float ManpowerMultiplier => terrain.ManpowerMultiplier;
        public float SailorsMultiplier => IsCoast ? terrain.SailorsMultiplier : terrain.LandLockedSailorsMultiplier;
        public float MoveSpeedMultiplier => terrain.MoveSpeedMultiplier;
        public float DefenderDamageMultiplier => terrain.DefenderDamageMultiplier;
        public int CombatWidth => terrain.CombatWidth;
        public bool IsSea => type == Type.Sea;
        public bool IsCoast => type == Type.Coast;
        public bool IsLand => type is Type.LandLocked or Type.Coast;
        
        public ProvinceLink this[Color32 color] => linkMap[color];
        public IEnumerable<ProvinceLink> Links => linkMap.Values;
        public IReadOnlyList<Vector2> Vertices => vertexList;
        internal IReadOnlyList<ProvinceLink> LinkList => linkList;
        private Color SolidMaterialColor {
            set => shapeMeshRenderer.sharedMaterials[1].color = value;
        }
        private Color BorderColor {
            set => outlineMeshRenderer.sharedMaterial.color = value;
        }

        private void Awake(){
            meshCollider = GetComponent<MeshCollider>();
            Land = GetComponent<Land>();
            Sea = GetComponent<Sea>();
            Debug.Assert(Land == null ^ Sea == null, $"FATAL: Province {gameObject.name} is both land and sea, or neither!");
            type = Land == null ? Type.Sea : Type.LandLocked;
        }
        internal void Init(string provinceName, Color32 colorKey, MapGraph mapGraph, Terrain terrainData, Color mapColor, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh, IEnumerable<Vector2> vertices){
            ColorKey = colorKey;
            gameObject.name = $"R:{colorKey.r}, G:{colorKey.g}, B:{colorKey.b}";
            
            terrain = terrainData;
            Name = provinceName;
            baseColor = mapColor;
            
            shapeMeshRenderer.materials[1].color = baseColor;
            shapeMeshRenderer.sharedMaterial = terrain.Material;
            outlineMeshRenderer.material.color = baseColor; 
            UpdateColors();

            MapPosition = mapPosition;
            outlineMeshFilter.sharedMesh = outlineMesh;
            meshCollider.sharedMesh = shapeMesh;
            shapeMeshFilter.sharedMesh = shapeMesh;
            vertexList.AddRange(vertices);
            
            Graph = mapGraph;
            mapGraph.Add(this);
        }
        public void AddNeighbor(Province neighbor, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter){
            if (neighbor != null){
                if (type == Type.LandLocked && neighbor.type == Type.Sea){
                    type = Type.Coast;
                } else if (type == Type.Sea && neighbor.type != Type.Sea){
                    Name = Sea.AnyCoastLinkName;
                }
            }
            ProvinceLink newLink = ProvinceLink.Create(this, neighbor, startIndex, endIndex, worldSpaceConverter);
            linkList.Add(newLink);
            if (neighbor != null){
                linkMap.Add(neighbor.ColorKey, newLink);
            }
        }
        
        private void UpdateColors(){
            Color opaque = baseColor;
            opaque.a = 1;
            BorderColor = opaque;
            float increasedBrightness = OneThird*(baseColor.grayscale+2);
            selectedColor = 0.5f*(baseColor+new Color(increasedBrightness, increasedBrightness, increasedBrightness));
            hoverColor = 0.5f*(baseColor+Color.gray);
            hoverSelectedColor = 0.25f*(3*selectedColor+Color.gray);
            UpdateColorMaterialColor();
        }
        private void UpdateColorMaterialColor(){
            SolidMaterialColor = isSelected ?
                isHovered ? hoverSelectedColor : selectedColor :
                isHovered ? hoverColor : baseColor;
        }
        
        public void OnHoverEnter(){
            isHovered = true;
            UpdateColorMaterialColor();
        }
        public void OnHoverLeave(){
            isHovered = false;
            UpdateColorMaterialColor();
        }
        public void OnSelect(){
            isSelected = true;
            UpdateColorMaterialColor();
        }
        public void OnDeselect(){
            isSelected = false;
            UpdateColorMaterialColor();
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
