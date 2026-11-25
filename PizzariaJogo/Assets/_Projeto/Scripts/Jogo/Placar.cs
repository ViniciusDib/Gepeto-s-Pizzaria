using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pizzaria.Jogo
{
    /// <summary>
    /// Tabela de recordes (top 10) salva em PlayerPrefs como JSON — funciona também no navegador (WebGL).
    /// Ordem: mais dias completos primeiro; empate decidido pelo dinheiro.
    /// Cada partida tem um id: salvar de novo a mesma partida atualiza a linha em vez de duplicar.
    /// </summary>
    public static class Placar
    {
        public const int Maximo = 10;
        public const int TamanhoNome = 12;
        private const string Chave = "Pizzaria_Recordes";
        private const string ChaveNome = "Pizzaria_Nome";

        [Serializable]
        public class Recorde
        {
            public string partida;
            public string nome;
            public int dias;
            public int dinheiro;
            public string data;
        }

        [Serializable]
        private class Lista { public List<Recorde> itens = new List<Recorde>(); }

        private static Lista cache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Resetar() => cache = null;

        public static IReadOnlyList<Recorde> Recordes => Carregar().itens;

        public static string UltimoNome
        {
            get => PlayerPrefs.GetString(ChaveNome, "Pizzaiolo");
            set => PlayerPrefs.SetString(ChaveNome, value);
        }

        /// <summary>Posição (1..10) que esse resultado ocuparia, ou 0 se não entra na tabela.</summary>
        public static int PosicaoPara(string partida, int dias, int dinheiro)
        {
            int pos = 1;
            foreach (var r in Carregar().itens)
            {
                if (r.partida == partida) continue;
                if (Compara(r.dias, r.dinheiro, dias, dinheiro) <= 0) pos++;
            }
            return pos <= Maximo ? pos : 0;
        }

        public static bool JaSalvou(string partida)
        {
            foreach (var r in Carregar().itens) if (r.partida == partida) return true;
            return false;
        }

        /// <summary>Salva (ou atualiza) o resultado da partida. Devolve a posição alcançada (0 = ficou fora).</summary>
        public static int Salvar(string partida, string nome, int dias, int dinheiro)
        {
            nome = Limpar(nome);
            UltimoNome = nome;
            var lista = Carregar();
            lista.itens.RemoveAll(r => r.partida == partida);
            var novo = new Recorde { partida = partida, nome = nome, dias = dias, dinheiro = dinheiro, data = DateTime.Now.ToString("dd/MM/yy") };
            lista.itens.Add(novo);
            lista.itens.Sort((a, b) => Compara(a.dias, a.dinheiro, b.dias, b.dinheiro));
            if (lista.itens.Count > Maximo) lista.itens.RemoveRange(Maximo, lista.itens.Count - Maximo);
            PlayerPrefs.SetString(Chave, JsonUtility.ToJson(lista));
            PlayerPrefs.Save();
            int pos = lista.itens.IndexOf(novo);
            return pos >= 0 ? pos + 1 : 0;
        }

        public static void Apagar()
        {
            cache = new Lista();
            PlayerPrefs.DeleteKey(Chave);
            PlayerPrefs.Save();
        }

        public static string Limpar(string nome)
        {
            nome = (nome ?? "").Replace("\n", "").Replace("\r", "").Trim();
            if (nome.Length > TamanhoNome) nome = nome.Substring(0, TamanhoNome);
            return nome.Length == 0 ? "Pizzaiolo" : nome;
        }

        // negativo = a vem antes de b
        private static int Compara(int diasA, int dinheiroA, int diasB, int dinheiroB) =>
            diasA != diasB ? diasB.CompareTo(diasA) : dinheiroB.CompareTo(dinheiroA);

        private static Lista Carregar()
        {
            if (cache != null) return cache;
            cache = new Lista();
            string json = PlayerPrefs.GetString(Chave, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { cache = JsonUtility.FromJson<Lista>(json) ?? new Lista(); }
                catch (Exception) { cache = new Lista(); } // dado corrompido: começa do zero
                if (cache.itens == null) cache.itens = new List<Recorde>();
            }
            return cache;
        }
    }
}
