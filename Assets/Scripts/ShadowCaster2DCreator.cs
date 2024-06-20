using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using YamlDotNet.RepresentationModel;

#if UNITY_EDITOR

public class ShadowCaster2DCreator : MonoBehaviour
{
	private ShadowCaster2D shadowCaster;
	private static BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;
	private static FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
	private static FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
	private static MethodInfo onEnableMethod = typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);

	private (string spriteSheetPath, int spriteID) FindSpriteSheet()
	{
		Debug.Log(gameObject.GetInstanceID());
		Debug.Log(AssetDatabase.GetAssetPath(gameObject.GetInstanceID()));
		using var reader = new StreamReader(AssetDatabase.GetAssetPath(gameObject.GetInstanceID()));
		var yaml = new YamlStream();
		yaml.Load(reader);

		var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
		var spriteRenderer = (YamlMappingNode)mapping.Children["SpriteRenderer"]["m_Sprite"];
		int fileID = int.Parse(spriteRenderer["fileID"].ToString());
		GUID guid = new GUID(spriteRenderer["guid"].ToString());

		return (AssetDatabase.GUIDToAssetPath(guid), fileID);
	}

	private void ReadPrefabYamlFile(string filePath, int guid)
	{
        using var reader = new StreamReader(filePath);
        var yaml = new YamlStream();
        yaml.Load(reader);

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
				float xHalfSizeDelta = rectTransform.sizeDelta.x / 2f;
				float yHalfSizeDelta = rectTransform.sizeDelta.y / 2f;

				foreach (YamlMappingNode point in outlinePoints.Children)
				{
					float ox = float.Parse(point["x"].ToString());
					float oy = float.Parse(point["y"].ToString());

					float x = ox * xScale + rectTransform.sizeDelta.x * (rectTransform.pivot.x - xPivot);
					float y = oy * yScale + rectTransform.sizeDelta.y * (rectTransform.pivot.y - yPivot);

					float factor = 0f;
					float offset = 0.1f;

					// First quadrant
					if (ox >= 0 && oy < 0)
					{
						x = x * (1 - factor) - offset;
						y = y * (1 + factor) + offset;
					}
					// Second quadrant...
					else if (ox < 0 && oy < 0)
					{
						x = x * (1 + factor) + offset;
						y = y * (1 + factor) + offset;
					}
					// Third...
					else if (ox < 0 && oy >= 0)
					{
						x = x * (1 + factor) + offset;
						y = y * (1 - factor) - offset;
					}
					// Fourth
					else
					{
						x = x * (1 - factor) - offset;
						y = y * (1 - factor) - offset;
					}

					Vector3 pos = new Vector3(x, y, 0);
					points[i] = pos;
					i++;
				}

				shapePathField.SetValue(shadowCaster, points);
				break;
			}
		}

		onEnableMethod.Invoke(shadowCaster, new object[0]);
    }

	public void Create()
	{
		(string spriteSheetPath, int spriteID) = FindSpriteSheet();
		Debug.Log($"{spriteSheetPath} {spriteID}");
		//ReadPrefabYamlFile("C:\\Users\\Xande\\Documents\\Unity\\Mabworld\\Assets\\Prefabs\\Terrain\\Trees\\Tree.prefab");
		//ReadPrefabYamlFile("C:\\Users\\Xande\\Documents\\Unity\\Mabworld\\Assets\\Resources\\Sprites\\Map Tilesets\\Grassland Tileset.png.meta", 597273058);
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