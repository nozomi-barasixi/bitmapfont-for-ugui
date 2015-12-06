#if UNITY_EDITOR

#if UNITY_5
#pragma warning disable 0618
#endif
// TODO:disable 0618について -- CharacterInfoのPropertiesがread onlyであるため、非推奨のメンバへアクセスしている。この問題はUnity5.2で修正予定のようだ。
// http://forum.unity3d.com/threads/unity5-problem-with-characterinfo-for-runtime-font-creation.306583/
// 抜粋：「Unity 5.2 will make all these new properties writeable.」（all theseとはacvance.minX,maxXを始めとする新しいプロパティのこと）

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
		Rect rect;
		float textureW = float.Parse(GetValue(common , "scaleW"));
		float textureH = float.Parse(GetValue(common , "scaleH"));

		for (int i=0; i < chars.Count; i++) 
		{
			XmlNode charNode = chars[i];
			if(charNode.Attributes != null)
			{
				charInfos[i].index = int.Parse(GetValue(charNode, "id"));
				charInfos[i].width = int.Parse(GetValue(charNode, "xadvance"));
				charInfos[i].flipped = false;
				
				rect = new Rect();
				rect.x = float.Parse(GetValue(charNode, "x")) / textureW;
				rect.width = float.Parse(GetValue(charNode, "width")) / textureW;

				rect.height = float.Parse(GetValue(charNode, "height"));
				rect.y = (textureH - float.Parse(GetValue(charNode, "y")) - rect.height) / textureH;
				rect.height = rect.height / textureH;
				charInfos[i].uv = rect;

				rect = new Rect();
				rect.width = float.Parse(GetValue(charNode, "width"));
				rect.height = -(float.Parse(GetValue(charNode, "height")));
				rect.x = float.Parse(GetValue(charNode, "xoffset"));
				rect.y = -(float.Parse(GetValue(charNode, "yoffset")));
				charInfos[i].vert = rect;
			}
		}

		string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(textAsset));
		string exportPath = rootPath + "/" + Path.GetFileNameWithoutExtension(textAsset.name);

		Material material = GenerateMaterial(exportPath, texture);
		Font font = GenerateFont(exportPath, textAsset.name, material);

		font.characterInfo = charInfos;

		// Save m_LineSpacing.
		SerializedObject serializedFont = new SerializedObject(font);
		SerializedProperty serializedLineSpacing = serializedFont.FindProperty("m_LineSpacing");
		serializedLineSpacing.floatValue = float.Parse(GetValue(common, "lineHeight"));
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

// TODO:Unity5.2で修正可能予定。詳細は上記TODO参照.
#if UNITY_5
#pragma warning restore 0618
#endif

#endif