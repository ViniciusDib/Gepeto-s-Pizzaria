using UnityEngine;

namespace Pizzaria.Cozinha
{
    /// <summary>Referências compartilhadas pelos objetos criados em tempo de jogo (pizzas).</summary>
    public class RecursosCozinha : MonoBehaviour
    {
        public static RecursosCozinha Instancia { get; private set; }

        [SerializeField, Tooltip("Material URP Lit branco; a cor de cada parte vem por MaterialPropertyBlock.")]
        private Material materialBase;

        public static Material MaterialBase => Instancia != null ? Instancia.materialBase : null;

        private void Awake() => Instancia = this;

        private void OnDestroy()
        {
            if (Instancia == this) Instancia = null;
        }
    }
}
