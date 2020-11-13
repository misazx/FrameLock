using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;



public class PreviewWindow2D : PreviewWindow
{

    public override void InitPreview()
    {
        base.InitPreview();
        //重置摄像机初始值
        camera.orthographic = true;
        camera.nearClipPlane = -128;
        camera.farClipPlane = 128;
        camera.transform.position = Vector3.zero;
        camera.transform.eulerAngles = new Vector3(0, 0, 0);
        //		camera.orthographicSize =6f;
        //m_ZoomFactor = 1.5f;
    }

    protected override void UF_OnUpdateCamera()
    {
        camera.transform.position = m_PivotPositionOffset;
        camera.orthographicSize = m_ZoomFactor * 1.2f;

    }


    protected override void OnDrawFloor()
    {
        if (m_FloorMaterial == null || m_FloorTexture == null || m_FloorPlane == null)
            return;
        Vector3 position = new Vector3(0f, 0f, 64f);
        Quaternion quater = Quaternion.Euler(new Vector3(90, 180, 0));
        Material floorMaterial = m_FloorMaterial;
        Matrix4x4 matrix2 = Matrix4x4.TRS(position, quater, Vector3.one * 3);
        floorMaterial.mainTextureOffset = -Vector2.zero * 0.08f ;
        floorMaterial.SetVector("_Alphas", new Vector4(0.1f * 1f, 0.3f * 1f, 0f, 0f));
        Graphics.DrawMesh(m_FloorPlane, matrix2, floorMaterial, PreviewHelper.CullingLayer, camera, 0);
    }

}
