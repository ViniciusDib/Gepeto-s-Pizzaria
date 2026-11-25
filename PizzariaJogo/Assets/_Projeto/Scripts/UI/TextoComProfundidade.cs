using UnityEngine;

namespace Pizzaria.UI
{
    /// <summary>
    /// Mantém o material "Pizzaria/Texto3D" (texto que respeita profundidade) com a textura atual da fonte.
    /// Fontes dinâmicas recriam a textura quando aparecem letras novas, então ela é conferida a cada quadro.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMesh))]
    public class TextoComProfundidade : MonoBehaviour
    {
        private TextMesh texto;
        private MeshRenderer rend;

        private void OnEnable()
        {
            texto = GetComponent<TextMesh>();
            rend = GetComponent<MeshRenderer>();
            Sincronizar();
        }

        private void LateUpdate() => Sincronizar();

        private void Sincronizar()
        {
            if (texto == null || rend == null || texto.font == null) return;
            var mat = rend.sharedMaterial;
            if (mat == null) return;
            var tex = texto.font.material != null ? texto.font.material.mainTexture : null;
            if (tex != null && mat.mainTexture != tex) mat.mainTexture = tex;
        }
    }
}
