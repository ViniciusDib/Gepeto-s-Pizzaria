using System;
using System.Collections.Generic;
using Pizzaria.Cozinha;
using Pizzaria.Dados;
using UnityEngine;

namespace Pizzaria.Jogador
{
    /// <summary>
    /// O que o cozinheiro está carregando: ingredientes (vários tipos, com limite por tipo)
    /// e no máximo uma pizza.
    /// </summary>
    public class InventarioJogador : MonoBehaviour
    {
        private readonly Dictionary<IngredienteSO, int> itens = new Dictionary<IngredienteSO, int>();
        private readonly List<IngredienteSO> ordemChegada = new List<IngredienteSO>();

        public event Action Mudou;

        public IReadOnlyList<IngredienteSO> Tipos => ordemChegada;

        /// <summary>Ingrediente que vai ser usado no próximo clique na bancada (roda do mouse / 1-9 troca).</summary>
        public IngredienteSO Selecionado { get; private set; }

        /// <summary>Pizza nas mãos (ou null).</summary>
        public Pizza Pizza { get; private set; }

        /// <summary>Sem ingredientes (a pizza não conta).</summary>
        public bool Vazio => ordemChegada.Count == 0;

        public int Quantidade(IngredienteSO ingrediente) =>
            ingrediente != null && itens.TryGetValue(ingrediente, out int q) ? q : 0;

        public int EspacoLivre(IngredienteSO ingrediente) =>
            ingrediente == null ? 0 : Mathf.Max(0, ingrediente.maxNaMao - Quantidade(ingrediente));

        /// <summary>Adiciona até <paramref name="qtd"/> unidades. Retorna quantas couberam.</summary>
        public int Adicionar(IngredienteSO ingrediente, int qtd = 1)
        {
            int adicionado = Mathf.Min(qtd, EspacoLivre(ingrediente));
            if (adicionado <= 0) return 0;

            if (!itens.ContainsKey(ingrediente)) ordemChegada.Add(ingrediente);
            itens[ingrediente] = Quantidade(ingrediente) + adicionado;
            Selecionado = ingrediente;
            Mudou?.Invoke();
            return adicionado;
        }

        /// <summary>Remove <paramref name="qtd"/> unidades se houver o suficiente.</summary>
        public bool Remover(IngredienteSO ingrediente, int qtd = 1)
        {
            int atual = Quantidade(ingrediente);
            if (qtd <= 0 || atual < qtd) return false;

            if (atual == qtd)
            {
                int indice = ordemChegada.IndexOf(ingrediente);
                itens.Remove(ingrediente);
                ordemChegada.Remove(ingrediente);
                if (Selecionado == ingrediente)
                    Selecionado = ordemChegada.Count > 0 ? ordemChegada[Mathf.Clamp(indice, 0, ordemChegada.Count - 1)] : null;
            }
            else
            {
                itens[ingrediente] = atual - qtd;
            }
            Mudou?.Invoke();
            return true;
        }

        /// <summary>Joga fora os ingredientes (a pizza fica).</summary>
        public void Esvaziar()
        {
            if (Vazio) return;
            itens.Clear();
            ordemChegada.Clear();
            Selecionado = null;
            Mudou?.Invoke();
        }

        /// <summary>Troca o ingrediente selecionado (+1 próximo, -1 anterior).</summary>
        public void Ciclar(int direcao)
        {
            if (ordemChegada.Count < 2 || direcao == 0) return;
            int i = Mathf.Max(0, ordemChegada.IndexOf(Selecionado));
            i = (i + direcao % ordemChegada.Count + ordemChegada.Count) % ordemChegada.Count;
            Selecionado = ordemChegada[i];
            Mudou?.Invoke();
        }

        public void Selecionar(int indice)
        {
            if (indice < 0 || indice >= ordemChegada.Count || Selecionado == ordemChegada[indice]) return;
            Selecionado = ordemChegada[indice];
            Mudou?.Invoke();
        }

        /// <summary>Pega uma pizza. Falha se já estiver segurando outra.</summary>
        public bool SegurarPizza(Pizza pizza)
        {
            if (pizza == null || Pizza != null) return false;
            Pizza = pizza;
            Mudou?.Invoke();
            return true;
        }

        /// <summary>Solta a pizza das mãos e a devolve (null se não havia).</summary>
        public Pizza SoltarPizza()
        {
            var p = Pizza;
            if (p == null) return null;
            Pizza = null;
            Mudou?.Invoke();
            return p;
        }
    }
}
