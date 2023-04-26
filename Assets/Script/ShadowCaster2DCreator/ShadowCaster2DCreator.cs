
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CompositeCollider2D))]
public class ShadowCaster2DCreator : MonoBehaviour
{
	[SerializeField]
	private bool selfShadows = true;
	//[SerializeField]
	//private string shadowLayerName = "ShadowLayer";

	private CompositeCollider2D tilemapCollider;

	static readonly FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly FieldInfo shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo generateShadowMeshMethod = typeof(ShadowCaster2D)
									.Assembly
									.GetType("UnityEngine.Rendering.Universal.ShadowUtility")
									.GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);

	public void Create()
	{
		DestroyOldShadowCasters();
		tilemapCollider = GetComponent<CompositeCollider2D>();

		//int shadowLayer = LayerMask.NameToLayer(shadowLayerName);

		Vector2 tmanchor = GetComponent<Tilemap>().tileAnchor;
		Vector2 tpos = (Vector2)transform.position + tmanchor;

		for (int i = 0; i < tilemapCollider.pathCount; i++)
		{
			Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
			tilemapCollider.GetPath(i, pathVertices);
			GameObject shadowCaster = new GameObject("shadow_caster_" + i);
			//shadowCaster.layer = shadowLayer;
			shadowCaster.transform.parent = gameObject.transform;
			ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
			shadowCasterComponent.selfShadows = this.selfShadows;

			Vector3[] testPath = new Vector3[pathVertices.Length];
			for (int j = 0; j < pathVertices.Length; j++)
			{
				testPath[j] = tpos + pathVertices[j] - tmanchor;
			}

			shapePathField.SetValue(shadowCasterComponent, testPath);
			shapePathHashField.SetValue(shadowCasterComponent, Random.Range(int.MinValue, int.MaxValue));
			meshField.SetValue(shadowCasterComponent, new Mesh());
			generateShadowMeshMethod.Invoke(shadowCasterComponent,
			new object[] { meshField.GetValue(shadowCasterComponent), shapePathField.GetValue(shadowCasterComponent) });
		}
	}
	public void DestroyOldShadowCasters()
	{

		var tempList = transform.Cast<Transform>().ToList();
		foreach (var child in tempList)
		{
			DestroyImmediate(child.gameObject);
		}
	}
}

