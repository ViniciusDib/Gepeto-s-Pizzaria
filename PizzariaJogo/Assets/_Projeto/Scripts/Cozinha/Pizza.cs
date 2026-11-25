using System;
using System.Collections.Generic;
using System.Text;
using Pizzaria.Dados;
using UnityEngine;

namespace Pizzaria.Cozinha
{
    public enum EstadoPizza { Crua, Pronta, Queimada }

    /// <summary>
    /// Uma pizza em jogo (dados puros, sem visual). Guarda os ingredientes na ordem
    /// em que foram colocados e quanto tempo passou no forno.
    /// </summary>
    public class Pizza
    {
        public const float TempoFornoPadrao = 8f;
        public const float ToleranciaPadrao = 6f;

        private readonly List<IngredienteSO> camadas = new List<IngredienteSO>();

        public IReadOnlyList<IngredienteSO> Camadas => camadas;
        public float TempoAssado { get; private set; }
        public float TempoNecessario { get; private set; }
        public float Tolerancia { get; private set; }

        /// <summary>Recusada pelo cliente e devolvida pelo garçom.</summary>
        public bool Devolvida { get; set; }

        /// <summary>Disparado quando um ingrediente é adicionado.</summary>
        public event Action Mudou;

        public bool JaFoiAoForno => TempoAssado > 0f;

        public EstadoPizza Estado
        {
            get
            {
                if (TempoNecessario <= 0f || TempoAssado < TempoNecessario) return EstadoPizza.Crua;
                return TempoAssado < TempoNecessario + Tolerancia ? EstadoPizza.Pronta : EstadoPizza.Queimada;
            }
        }

        /// <summary>0 = crua, 1 = no ponto.</summary>
        public float ProgressoForno => TempoNecessario > 0f ? Mathf.Clamp01(TempoAssado / TempoNecessario) : 0f;

        /// <summary>0 = no ponto, 1 = queimada.</summary>
        public float ProgressoQueima =>
            TempoNecessario > 0f && Tolerancia > 0f ? Mathf.Clamp01((TempoAssado - TempoNecessario) / Tolerancia) : 0f;

        public void Adicionar(IngredienteSO ingrediente)
        {
            if (ingrediente == null) return;
            camadas.Add(ingrediente);
            Mudou?.Invoke();
        }

        /// <summary>Define o tempo de forno na primeira vez que entra no forno.</summary>
        public void DefinirTempoForno(float tempo, float tolerancia)
        {
            if (TempoNecessario > 0f) return;
            TempoNecessario = Mathf.Max(0.5f, tempo);
            Tolerancia = Mathf.Max(0.5f, tolerancia);
        }

        public void Assar(float dt) => TempoAssado += dt;

        /// <summary>Junta cliques seguidos do mesmo ingrediente: [Massa, Molho, Molho] → Massa 1, Molho 2.</summary>
        public List<PassoReceita> Agrupar()
        {
            var grupos = new List<PassoReceita>();
            foreach (var ing in camadas)
            {
                int ultimo = grupos.Count - 1;
                if (ultimo >= 0 && grupos[ultimo].ingrediente == ing)
                {
                    var p = grupos[ultimo];
                    p.quantidade++;
                    grupos[ultimo] = p;
                }
                else grupos.Add(new PassoReceita(ing, 1));
            }
            return grupos;
        }

        /// <summary>Montada exatamente como a receita (ingredientes, quantidades e ordem).</summary>
        public bool MontagemCorreta(ReceitaSO receita)
        {
            if (receita == null) return false;
            var grupos = Agrupar();
            if (grupos.Count != receita.passos.Count) return false;
            for (int i = 0; i < grupos.Count; i++)
            {
                if (grupos[i].ingrediente != receita.passos[i].ingrediente ||
                    grupos[i].quantidade != receita.passos[i].quantidade)
                    return false;
            }
            return true;
        }

        public ReceitaSO IdentificarReceita(IEnumerable<ReceitaSO> receitas)
        {
            if (receitas == null) return null;
            foreach (var r in receitas)
                if (MontagemCorreta(r)) return r;
            return null;
        }

        /// <summary>Quantos passos iniciais batem com a receita (para achar a comanda "mais parecida").</summary>
        public int PassosIniciaisCorretos(ReceitaSO receita)
        {
            if (receita == null) return 0;
            var grupos = Agrupar();
            int n = 0;
            while (n < grupos.Count && n < receita.passos.Count &&
                   grupos[n].ingrediente == receita.passos[n].ingrediente &&
                   grupos[n].quantidade == receita.passos[n].quantidade)
                n++;
            return n;
        }

        /// <summary>
        /// Próximo ingrediente a colocar para seguir a receita. Retorna null se já está completa
        /// (montada = true) ou se a montagem já saiu da receita (errada = true).
        /// </summary>
        public IngredienteSO ProximoIngrediente(ReceitaSO receita, out bool montada, out bool errada)
        {
            montada = errada = false;
            if (receita == null || receita.passos.Count == 0) { errada = true; return null; }

            var grupos = Agrupar();
            if (grupos.Count == 0) return receita.passos[0].ingrediente;

            int ultimo = grupos.Count - 1;
            for (int i = 0; i < grupos.Count; i++)
            {
                if (i >= receita.passos.Count || grupos[i].ingrediente != receita.passos[i].ingrediente) { errada = true; return null; }
                bool deveEstarCompleto = i < ultimo;
                if (grupos[i].quantidade > receita.passos[i].quantidade ||
                    (deveEstarCompleto && grupos[i].quantidade != receita.passos[i].quantidade)) { errada = true; return null; }
            }

            if (grupos[ultimo].quantidade < receita.passos[ultimo].quantidade) return grupos[ultimo].ingrediente;
            if (ultimo + 1 < receita.passos.Count) return receita.passos[ultimo + 1].ingrediente;
            montada = true;
            return null;
        }

        /// <summary>Explica por que a pizza não serve para a receita, ou null se está tudo certo.</summary>
        public string Diagnosticar(ReceitaSO receita)
        {
            if (Estado == EstadoPizza.Crua) return "a pizza está crua";
            if (Estado == EstadoPizza.Queimada) return "a pizza está queimada";
            if (receita == null) return "ninguém pediu essa pizza";
            if (MontagemCorreta(receita)) return null;

            var pedido = new Dictionary<IngredienteSO, int>();
            foreach (var p in receita.passos)
                if (p.ingrediente != null)
                    pedido[p.ingrediente] = (pedido.TryGetValue(p.ingrediente, out int q) ? q : 0) + p.quantidade;

            var colocado = new Dictionary<IngredienteSO, int>();
            foreach (var ing in camadas)
                colocado[ing] = (colocado.TryGetValue(ing, out int q) ? q : 0) + 1;

            foreach (var kv in colocado)
                if (!pedido.ContainsKey(kv.Key))
                    return $"{kv.Key.Nome} não vai na {receita.Nome}";

            foreach (var kv in pedido)
            {
                int c = colocado.TryGetValue(kv.Key, out int q) ? q : 0;
                if (c < kv.Value) return $"faltou {kv.Key.Nome} na {receita.Nome}";
                if (c > kv.Value) return $"{kv.Key.Nome} demais na {receita.Nome}";
            }

            return $"ingredientes fora de ordem na {receita.Nome}";
        }

        /// <summary>Ex.: "Massa › Molho ×2 › Queijo ×3".</summary>
        public string DescricaoIngredientes()
        {
            var grupos = Agrupar();
            if (grupos.Count == 0) return "vazia";
            var sb = new StringBuilder();
            for (int i = 0; i < grupos.Count; i++)
            {
                if (i > 0) sb.Append(" › ");
                sb.Append(grupos[i].ingrediente.Nome);
                if (grupos[i].quantidade > 1) sb.Append(" ×").Append(grupos[i].quantidade);
            }
            return sb.ToString();
        }

        public string DescricaoEstado()
        {
            if (Devolvida) return "<color=#ff7a6a>devolvida</color>";
            switch (Estado)
            {
                case EstadoPizza.Pronta: return "<color=#8fe388>assada</color>";
                case EstadoPizza.Queimada: return "<color=#ff7a6a>queimada</color>";
                default: return JaFoiAoForno ? $"assando {ProgressoForno:P0}" : "crua";
            }
        }
    }
}
