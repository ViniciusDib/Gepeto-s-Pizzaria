using UnityEngine;

namespace Pizzaria.Interacao
{
    /// <summary>
    /// Base de tudo que o jogador pode mirar e clicar. Cuida do destaque visual
    /// (clareia a cor dos renderers enquanto está sendo mirado).
    /// </summary>
    public abstract class Interagivel : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField, Tooltip("Vazio = todos os renderers filhos.")]
        private Renderer[] renderersDestaque;

        [SerializeField, Range(0f, 1f)] private float forcaDestaque = 0.35f;

        private MaterialPropertyBlock bloco;
        private Color[] coresOriginais;
        private bool destacado;

        protected virtual void Awake()
        {
            if (renderersDestaque == null || renderersDestaque.Length == 0)
                renderersDestaque = GetComponentsInChildren<Renderer>();

            bloco = new MaterialPropertyBlock();
            coresOriginais = new Color[renderersDestaque.Length];
            for (int i = 0; i < renderersDestaque.Length; i++)
            {
                var mat = renderersDestaque[i] != null ? renderersDestaque[i].sharedMaterial : null;
                coresOriginais[i] = mat != null && mat.HasProperty(BaseColorId) ? mat.GetColor(BaseColorId) : Color.clear;
            }
        }

        /// <summary>Texto mostrado na mira, ex.: "[Clique] Pegar Massa".</summary>
        public abstract string TextoInteracao(InteracaoJogador jogador);

        public abstract void Interagir(InteracaoJogador jogador);

        /// <summary>Ação secundária opcional (clique direito / Q).</summary>
        public virtual void InteragirSecundario(InteracaoJogador jogador) { }

        public void Destacar(bool ligar)
        {
            if (destacado == ligar || renderersDestaque == null) return;
            destacado = ligar;

            for (int i = 0; i < renderersDestaque.Length; i++)
            {
                var r = renderersDestaque[i];
                if (r == null || coresOriginais[i] == Color.clear) continue;

                if (ligar)
                {
                    r.GetPropertyBlock(bloco);
                    bloco.SetColor(BaseColorId, Color.Lerp(coresOriginais[i], Color.white, forcaDestaque));
                    r.SetPropertyBlock(bloco);
                }
                else
                {
                    r.SetPropertyBlock(null);
                }
            }
        }

        protected virtual void OnDisable() => Destacar(false);
    }
}
