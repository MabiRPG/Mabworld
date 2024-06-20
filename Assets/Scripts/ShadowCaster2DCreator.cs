using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class TilesetMeta
{
	public int FileFormatVersion;
	public int GUID;
	public Dictionary<string, object> TextureImporter;
}

#if UNITY_EDITOR

public class ShadowCaster2DCreator : MonoBehaviour
{
	[SerializeField]
	private bool selfShadows = true;

	private ShadowCaster2D shadowCaster;
	private static BindingFlags accessFlagsPrivate =
		BindingFlags.NonPublic | BindingFlags.Instance;
	private static FieldInfo meshField =
		typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
	private static FieldInfo shapePathField =
		typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
	private static MethodInfo onEnableMethod =
		typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);

	private void ReadPrefabYamlFile(string filePath, int guid)
	{
        using var reader = new StreamReader(filePath);
        var yaml = new YamlStream();
        yaml.Load(reader);

		// // Examine the stream
		// foreach (var node in yaml.Documents)
		// {
		// 	var mapping = (YamlMappingNode)node.RootNode;

		// 	foreach (var entry in mapping.Children)
		// 	{
		// 		Debug.Log(((YamlScalarNode)entry.Key).Value);
		// 	}
		// }

		// ShadowCaster2D shadowCaster2D = GetComponent<ShadowCaster2D>();
		// SerializedProperty m_ShapePath = new UnityEditor.SerializedObject(shadowCaster2D).FindProperty("m_ShapePath");
		// SerializedProperty m_Mesh = new UnityEditor.SerializedObject(shadowCaster2D).FindProperty("m_mesh");
		// m_ShapePath.ClearArray();
		// m_Mesh = null;

		shadowCaster = GetComponent<ShadowCaster2D>();
		meshField.SetValue(shadowCaster, null);

		var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
		var spriteSheet = (YamlMappingNode)mapping.Children["TextureImporter"]["spriteSheet"];
		var sprites = (YamlSequenceNode)spriteSheet.Children["sprites"];

		foreach (YamlMappingNode entry in sprites.Children)
		{
			if (int.Parse(entry.Children["internalID"].ToString()) == guid)
			{
				var outlinePoints = (YamlSequenceNode)entry.Children["outline"][0];
				Vector3[] points = new Vector3[outlinePoints.Children.Count];
				int i = 0;

				var rect = (YamlMappingNode)entry.Children["rect"];
				float width = float.Parse(rect["width"].ToString());
				float height = float.Parse(rect["height"].ToString());

				var pivot = (YamlMappingNode)entry.Children["pivot"];
				float xPivot = float.Parse(pivot["x"].ToString());
				float yPivot = float.Parse(pivot["y"].ToString());

				RectTransform rectTransform = GetComponent<RectTransform>();
				float xScale = rectTransform.sizeDelta.x / width;
				float yScale = rectTransform.sizeDelta.y / height;

				foreach (YamlMappingNode point in outlinePoints.Children)
				{
					float x = float.Parse(point["x"].ToString()) * xScale + rectTransform.sizeDelta.x * (rectTransform.pivot.x - xPivot);
					float y = float.Parse(point["y"].ToString()) * yScale + rectTransform.sizeDelta.y * (rectTransform.pivot.y - yPivot);

					Vector3 pos = new Vector3(x, y, 0);
					points[i] = pos;
					i++;
				}

				shapePathField.SetValue(shadowCaster, points);
				break;
			}
		}

		onEnableMethod.Invoke(shadowCaster, new object[0]);


		// Debug.Log(m_ShapePath.arraySize);

		// var spriteSheet = (YamlMappingNode)textureImporter.Children[new YamlScalarNode("spriteSheet")];
		// var sprite = (YamlSequenceNode)spriteSheet.Children[

        // var deserializer = new DeserializerBuilder()
        //     .WithNamingConvention(UnderscoredNamingConvention.Instance)
        //     .Build();

		// var p = deserializer.Deserialize<TilesetMeta>(reader);
		// Debug.Log(p.TextureImporter["spriteSheet"]);


    }

	private void GenerateShadowShapePath()
	{

	}

	public void Create()
	{
		//ReadPrefabYamlFile("C:\\Users\\Xande\\Documents\\Unity\\Mabworld\\Assets\\Prefabs\\Terrain\\Trees\\Tree.prefab");
		ReadPrefabYamlFile("C:\\Users\\Xande\\Documents\\Unity\\Mabworld\\Assets\\Resources\\Sprites\\Map Tilesets\\Grassland Tileset.png.meta", 597273058);
	}
	public void DestroyOldShadowCasters()
	{
	}
}

[CustomEditor(typeof(ShadowCaster2DCreator))]
public class ShadowCaster2DTileMapEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Create"))
		{
			var creator = (ShadowCaster2DCreator)target;
			creator.Create();
		}

		if (GUILayout.Button("Remove Shadows"))
		{
			var creator = (ShadowCaster2DCreator)target;
			creator.DestroyOldShadowCasters();
		}
		EditorGUILayout.EndHorizontal();
	}

}

#endif













































































