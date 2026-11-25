using UnityEngine;

namespace Pizzaria.Dados
{
    /// <summary>Como o ingrediente aparece na pizza.</summary>
    public enum FormaIngrediente
    {
        /// <summary>A massa: abre uma pizza nova na bancada.</summary>
        Base,
        /// <summary>Cobre a pizza inteira (molho, queijo). Cada clique espalha mais.</summary>
        Camada,
        /// <summary>Pedaços espalhados por cima (tomate, manjericão...).</summary>
        Cobertura
    }

    /// <summary>Modelo 3D usado para desenhar o ingrediente (na pizza e na mão do cozinheiro).</summary>
    public enum VisualIngrediente
    {
        Generico, Massa, Molho, Queijo, Tomate, Manjericao, Calabresa, Cebola, Azeitona
    }

    [CreateAssetMenu(fileName = "Ingrediente", menuName = "Pizzaria/Ingrediente")]
    public class IngredienteSO : ScriptableObject
    {
        public string nomeExibicao = "Ingrediente";
        public Color cor = Color.white;
        public FormaIngrediente forma = FormaIngrediente.Cobertura;
        [Tooltip("Qual modelo desenhar: rodela de tomate, folha de manjericão, anel de cebola...")]
        public VisualIngrediente visual = VisualIngrediente.Generico;

        [Tooltip("Quantas unidades desse ingrediente o jogador consegue carregar de uma vez.")]
        [Min(1)] public int maxNaMao = 10;

        public string Nome => string.IsNullOrEmpty(nomeExibicao) ? name : nomeExibicao;
    }
}
