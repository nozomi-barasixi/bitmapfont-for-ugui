using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;

[CustomEditor(typeof(BitmapFontScaling))]
public class BitmapFontScalingEditor :  Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		BitmapFontScaling script = target as BitmapFontScaling;

		Text text = script.GetComponent<Text>();

		if( text == null )
			return;

		if( serializedObject.FindProperty("textSize").intValue != text.fontSize )
			script.OnValidate();

		serializedObject.ApplyModifiedProperties();
	}
}
