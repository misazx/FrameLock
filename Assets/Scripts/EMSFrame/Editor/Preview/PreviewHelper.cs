using UnityEngine;
using UnityEditor;


public static class PreviewHelper
{
	public const int CullingLayer = 31;

	public static void SetEnabledRecursive(GameObject go, bool enabled)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = componentsInChildren[i];
			renderer.enabled = enabled;
		}
	}

	public static void SetLayerRecursive(GameObject go)
	{
        go.hideFlags = HideFlags.HideAndDontSave;
        //go.hideFlags = HideFlags.DontSave;
        go.layer = CullingLayer;
		foreach (Transform c in go.transform)
			SetLayerRecursive(c.gameObject);
	}

	public static Bounds GetBoundsRecurse(GameObject go)
	{
		// Do we have a mesh?
		Bounds bounds = new Bounds(go.transform.position, Vector3.zero);

		Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
		foreach (var item in renderers) {
			if (item) {
				// To prevent origo from always being included in bounds we initialize it
				// with renderer.bounds. This ensures correct bounds for meshes with origo outside the mesh.
				if (bounds.extents == Vector3.zero)
					bounds = item.bounds;
				else
					bounds.Encapsulate (item.bounds);
			}
		}

		return bounds;
	}


	public static GameObject InstantiateGameObject(GameObject original)
	{
		if (original == null)
			throw new System.Exception("The prefab you want to instantiate is null.");

//		GameObject go = InstantiateRemoveAllNonAnimationComponents(original, Vector3.zero, Quaternion.identity) as GameObject;

		GameObject go = Object.Instantiate(original) as GameObject;
		if(go == null)
			throw new System.Exception("The prefab you want to instantiate is not a GameObject Type.");
        go.name = original.name;
        FormatGameObject(go);
        return go;
	}

    public static void FormatGameObject(GameObject go) {
        //移除多余的MonoBehaviour
        //var monobehaviours = go.GetComponentsInChildren<MonoBehaviour>();
        //for (int k = 0; k < monobehaviours.Length; k++)
        //{
        //    Object.DestroyImmediate(monobehaviours[k]);
        //}
        go.tag = "Untagged";
        SetLayerRecursive(go);

        Animator[] animators = go.GetComponentsInChildren<Animator>();
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];

            animator.enabled = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.logWarnings = false;
            animator.fireEvents = false;
        }

        if (animators.Length == 0)
        {
            Animator animator = go.AddComponent<Animator>();
            animator.enabled = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.logWarnings = false;
            animator.fireEvents = false;
        }
    }



}