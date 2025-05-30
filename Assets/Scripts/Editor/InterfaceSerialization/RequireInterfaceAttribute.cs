using UnityEngine;
using UnityEditor;

namespace InterfaceSerialization
{
	// Credit: https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/ and #comment-197 on that very page.
	/// <summary>
	/// Attribute that require implementation of the provided interface.
	/// </summary>
	public class RequireInterfaceAttribute : PropertyAttribute
	{
		// Interface type.
		public System.Type requiredType { get; private set; }
		/// <summary>
		/// Requiring implementation of the <see cref="T:RequireInterfaceAttribute"/> interface.
		/// </summary>
		/// <param name="type">Interface type.</param>
		public RequireInterfaceAttribute(System.Type type)
		{
			this.requiredType = type;
		}
	}
#if UNITY_EDITOR
	/// <summary>
	/// Drawer for the RequireInterface attribute.
	/// </summary>
	[CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
	public class RequireInterfaceDrawer : PropertyDrawer
	{
		/// <summary>
		/// Overrides GUI drawing for the attribute.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="property">Property.</param>
		/// <param name="label">Label.</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Check if this is reference type property.
			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				// Get attribute parameters.
				var requiredAttribute = this.attribute as RequireInterfaceAttribute;
				// Begin drawing property field.
				EditorGUI.BeginProperty(position, label, property);
				// Draw property field.
				var reference = EditorGUI.ObjectField(position, label, property.objectReferenceValue, requiredAttribute.requiredType, true);
				if (reference is null)
				{
					var obj = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Object), true);
					if (obj is GameObject g)
					{
						reference = g.GetComponent(requiredAttribute.requiredType);
					}
				}
				property.objectReferenceValue = reference;
				// Finish drawing property field.
				EditorGUI.EndProperty();
			}
			else
			{
				// If field is not reference, show error message.
				// Save previous color and change GUI to red.
				var previousColor = GUI.color;
				GUI.color = Color.red;
				// Display label with error message.
				EditorGUI.LabelField(position, label, new GUIContent("Property is not a reference type"));
				// Revert color change.
				GUI.color = previousColor;
			}
		}
	}
#endif
}
