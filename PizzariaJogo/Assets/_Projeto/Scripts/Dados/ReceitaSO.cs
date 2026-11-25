using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pizzaria.Dados
{
    [Serializable]
    public struct PassoReceita
    {
        public IngredienteSO ingrediente;
        [Min(1)] public int quantidade;

        public PassoReceita(IngredienteSO ingrediente, int quantidade)
        {
            this.ingrediente = ingrediente;
            this.quantidade = quantidade;
        }
    }

    /// <summary>
    /// Receita de pizza. A ordem dos passos é obrigatória na montagem
    /// (cada passo = N cliques do mesmo ingrediente).
    /// </summary>
    [CreateAssetMenu(fileName = "Receita", menuName = "Pizzaria/Receita")]
    public class ReceitaSO : ScriptableObject
    {
        public string nomeExibicao = "Pizza";
        public List<PassoReceita> passos = new List<PassoReceita>();

        [Tooltip("Tempo (s) até a pizza ficar pronta no forno.")]
        [Min(1f)] public float tempoForno = 8f;

        [Tooltip("Tempo extra (s) depois de pronta até queimar.")]
        [Min(0.5f)] public float toleranciaQueima = 6f;

        [Min(0)] public int preco = 20;

        public string Nome => string.IsNullOrEmpty(nomeExibicao) ? name : nomeExibicao;

        /// <summary>Total de cliques necessários para montar.</summary>
        public int TotalCliques
        {
            get
            {
                int t = 0;
                foreach (var p in passos) t += p.quantidade;
                return t;
            }
        }

        /// <summary>Ex.: "Massa 1 › Molho 2 › Queijo 3".</summary>
        public string DescricaoPassos(string separador = " › ")
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < passos.Count; i++)
            {
                if (i > 0) sb.Append(separador);
                var p = passos[i];
                sb.Append(p.ingrediente != null ? p.ingrediente.Nome : "?").Append(' ').Append(p.quantidade);
            }
            return sb.ToString();
        }
    }
}
