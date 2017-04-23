#if UNITY_4 || UNITY_5_1
#pragma warning disable 0618
#endif

#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using System.Xml;

using UnityEditor;

public static class BitmapFontGenerater 
{
	static string DEFAULT_SHADER = "Unlit/Transparent";

	[MenuItem("Assets/Create/Bitmap Font")]
	public static void GenerateBitmapFont()
	{
		Object[] textAssets = Selection.GetFiltered(typeof(TextAsset), SelectionMode.DeepAssets);
		Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

		if(textAssets.Length < 1)
		{
			Debug.LogError("BitmapFont Create Error -- XML File is not Selected. (XMLファイルを選択してください)");
			return;
		}
		if(textures.Length < 1)
		{
			Debug.LogError("BitmapFont Create Error -- Texture File is not selected. (フォントテクスチャを選択してください)");
			return;
		}

		Generate((TextAsset)textAssets[0] , (Texture2D)textures[0]);
	}

	static void Generate(TextAsset textAsset , Texture2D texture)
	{
		XmlDocument xml = new XmlDocument();
		xml.LoadXml(textAsset.text);

		XmlNode common = xml.GetElementsByTagName("common")[0];
		XmlNodeList chars = xml.GetElementsByTagName("chars")[0].ChildNodes;
		
		CharacterInfo[] charInfos = new CharacterInfo[chars.Count];

		float textureW = float.Parse(GetValue(common , "scaleW"));
		float textureH = float.Parse(GetValue(common , "scaleH"));

		for (int i=0; i < chars.Count; i++) 
		{
			XmlNode charNode = chars[i];
			if(charNode.Attributes != null)
			{
				charInfos[i].index = int.Parse(GetValue(charNode, "id"));

				Rect vertRect = new Rect();
				vertRect.width = float.Parse(GetValue(charNode, "width"));
				vertRect.height = -(float.Parse(GetValue(charNode, "height")));
				vertRect.x = float.Parse(GetValue(charNode, "xoffset"));
				vertRect.y = -(float.Parse(GetValue(charNode, "yoffset")));

#if UNITY_4 || UNITY_5_1
				Rect uvRect = new Rect();
				uvRect.x = float.Parse(GetValue(charNode, "x")) / textureW;
				uvRect.width = float.Parse(GetValue(charNode, "width")) / textureW;
				uvRect.height = float.Parse(GetValue(charNode, "height"));
				uvRect.y = (textureH - float.Parse(GetValue(charNode, "y")) - uvRect.height) / textureH;
				uvRect.height = uvRect.height / textureH;

				charInfos[i].width = int.Parse(GetValue(charNode, "xadvance"));
				charInfos[i].flipped = false;
				charInfos[i].uv = uvRect;
				charInfos[i].vert = vertRect;
#else
				float charX = float.Parse(GetValue(charNode, "x")) / textureW;
				float charWidth = float.Parse(GetValue(charNode, "width")) / textureW;
				float charHeight = float.Parse(GetValue(charNode, "height"));
				float charY = (textureH - float.Parse(GetValue(charNode, "y")) - charHeight) / textureH;
				charHeight = charHeight / textureH;

				// UnFlipped.
				charInfos[i].uvBottomLeft = new Vector2(charX, charY);
				charInfos[i].uvBottomRight = new Vector2(charX + charWidth, charY);
				charInfos[i].uvTopLeft = new Vector2(charX, charY + charHeight);
				charInfos[i].uvTopRight = new Vector2(charX + charWidth, charY + charHeight);

				charInfos[i].minX = (int)vertRect.xMin;
				charInfos[i].maxX = (int)vertRect.xMax;
				charInfos[i].minY = (int)vertRect.yMax;
				charInfos[i].maxY = (int)vertRect.yMin;

				charInfos[i].advance = int.Parse(GetValue(charNode, "xadvance"));
#endif
			}
		}

		string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(textAsset));
		string exportPath = rootPath + "/" + Path.GetFileNameWithoutExtension(textAsset.name);

		Material material = GenerateMaterial(exportPath, texture);
		Font font = GenerateFont(exportPath, textAsset.name, material);

		font.characterInfo = charInfos;

		// Save m_LineSpacing.
		XmlNode info = xml.GetElementsByTagName("info")[0];
		SerializedObject serializedFont = new SerializedObject(font);
		SerializedProperty serializedLineSpacing = serializedFont.FindProperty("m_LineSpacing");
		serializedLineSpacing.floatValue = Mathf.Abs(float.Parse(GetValue(info, "size")));
		serializedFont.ApplyModifiedProperties();
	}

	static Material GenerateMaterial(string materialPath , Texture2D texture)
	{
		Shader shader = Shader.Find(DEFAULT_SHADER);
		Material material = LoadAsset<Material>(materialPath + ".mat", new Material(shader));
		material.shader = shader;
		material.mainTexture = texture;

		SaveAsset(material, materialPath + ".mat");

		return material;
	}

	static Font GenerateFont(string fontPath, string fontName, Material material)
	{
		Font font = LoadAsset<Font>(fontPath + ".fontsettings", new Font(fontName));
		font.material = material;

		SaveAsset(font, fontPath + ".fontsettings");

		return font;
	}

	static string GetValue(XmlNode node, string name)
	{
		return node.Attributes.GetNamedItem(name).InnerText;
	}

	static void SaveAsset(Object obj, string path)
	{
		Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
		if(existingAsset != null)
		{
			EditorUtility.CopySerialized(obj, existingAsset);
			AssetDatabase.SaveAssets();
		}
		else
		{
			AssetDatabase.CreateAsset(obj, path);
		}
	}
	
	static T LoadAsset<T>(string path , T defaultAsset) where T : Object 
	{
		T existingAsset = AssetDatabase.LoadMainAssetAtPath(path) as T;
		if(existingAsset == null)
		{
			existingAsset = defaultAsset;
		}
		return existingAsset;
	}
}
#endif

#if UNITY_4 || UNITY_5_1
#pragma warning restore 0618
#endif