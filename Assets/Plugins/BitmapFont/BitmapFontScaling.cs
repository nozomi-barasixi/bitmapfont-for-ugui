using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BitmapFontScaling : MonoBehaviour
{
	[SerializeField] int textSize;
	float bitmapFontOriginScale;
	Text targetText;

	public void OnValidate()
	{
		if( targetText == null )
			targetText = GetComponent<Text>();
		 
		if( targetText == null || targetText.font == null || targetText.font.dynamic == true )
		{
			Debug.LogWarning("Bitmap font is not found.");
			return;
		}

		GetFontSize();
		UpdateScale();
	}

	void GetFontSize()
	{
		textSize = targetText.fontSize;

		bitmapFontOriginScale = targetText.font.lineHeight;
	}

	void UpdateScale()
	{
		float scale;
		
		if( textSize <= 0 || bitmapFontOriginScale <= 0 )
			scale = 1f;
		else
			scale = (float)textSize / bitmapFontOriginScale;
		
		transform.localScale = new Vector3(scale, scale, scale);
	}
}