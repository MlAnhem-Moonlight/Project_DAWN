using UnityEngine;
using TMPro;

public class VertexWarp : MonoBehaviour
{
    public float amplitude = 5f;   // độ cao sóng
    public float frequency = 2f;   // khoảng cách chữ theo sóng
    public float speed = 2f;       // tốc độ chạy sóng

    private TMP_Text textMesh;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;

            float wave = Mathf.Sin((Time.time * speed) + (i * frequency)) * amplitude;

            Vector3 offset = new Vector3(0, wave, 0);

            verts[vertexIndex + 0] += offset;
            verts[vertexIndex + 1] += offset;
            verts[vertexIndex + 2] += offset;
            verts[vertexIndex + 3] += offset;
        }

        // cập nhật lại toàn bộ mesh sau khi thay đổi
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textMesh.UpdateGeometry(meshInfo.mesh, m);
        }
    }
}
