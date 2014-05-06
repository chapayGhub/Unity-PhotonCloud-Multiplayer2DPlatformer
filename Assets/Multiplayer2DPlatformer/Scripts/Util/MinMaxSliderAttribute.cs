using UnityEngine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MinMaxSliderAttribute : PropertyAttribute {
	
	public readonly float max;
	public readonly float min;
	
	public MinMaxSliderAttribute (float min, float max) {
		this.min = min;
		this.max = max;
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer (typeof (MinMaxSliderAttribute))]
class MinMaxSliderDrawer : PropertyDrawer {
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		if (property.propertyType == SerializedPropertyType.Vector2)
		{
			Vector2 range = property.vector2Value;
			float min = range.x;
			float max = range.y;
			MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
			EditorGUI.BeginChangeCheck ();
			EditorGUI.MinMaxSlider (label, position, ref min, ref max, attr.min, attr.max);
			position.y += 15;
			position.x += 150;
			EditorGUI.LabelField ( position, "Min:" + min.ToString() + " Max:" + max.ToString() );
			if (EditorGUI.EndChangeCheck ()) {
				range.x = min;
				range.y = max;
				property.vector2Value = range;
			}
		}
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return base.GetPropertyHeight (property, label) + 15;
	}
}
#endif