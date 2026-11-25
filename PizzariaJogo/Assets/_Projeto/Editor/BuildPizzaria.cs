using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Pizzaria.EditorTools
{
    /// <summary>Menu Pizzaria > Build: gera as versões para entrega (Windows obrigatório, WebGL para o itch.io).</summary>
    public static class BuildPizzaria
    {
        [MenuItem("Pizzaria/Build Windows", priority = 20)]
        public static void BuildWindows() =>
            Construir(BuildTarget.StandaloneWindows64, "Builds/Windows/Pizzaria3D.exe");

        [MenuItem("Pizzaria/Build WebGL", priority = 21)]
        public static void BuildWebGL() =>
            Construir(BuildTarget.WebGL, "Builds/WebGL");

        /// <summary>
        /// Linha de comando (sem abrir o Editor): gera a cena atualizada e faz a build Windows.
        /// Unity.exe -batchmode -quit -projectPath PizzariaJogo -executeMethod Pizzaria.EditorTools.BuildPizzaria.BuildWindowsLote
        /// </summary>
        public static void BuildWindowsLote()
        {
            GeradorCozinha.Gerar();
            var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { GeradorCozinha.CaminhoCena },
                locationPathName = "Builds/Windows/Pizzaria3D.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            }).summary;
            Debug.Log($"[Pizzaria] Build em lote: {r.result}, {r.totalErrors} erro(s), {r.totalSize / (1024 * 1024)} MB");
            EditorApplication.Exit(r.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static void Construir(BuildTarget alvo, string caminho)
        {
            if (!File.Exists(GeradorCozinha.CaminhoCena))
            {
                EditorUtility.DisplayDialog("Build", "Gere a cozinha primeiro (Pizzaria > Gerar Cozinha Greybox).", "OK");
                return;
            }

            var opcoes = new BuildPlayerOptions
            {
                scenes = new[] { GeradorCozinha.CaminhoCena },
                locationPathName = caminho,
                target = alvo,
                options = BuildOptions.None
            };

            var relatorio = BuildPipeline.BuildPlayer(opcoes);
            var r = relatorio.summary;
            if (r.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Pizzaria] Build {alvo} pronta em {caminho} ({r.totalSize / (1024 * 1024)} MB).");
                EditorUtility.RevealInFinder(caminho);
            }
            else
            {
                Debug.LogError($"[Pizzaria] Build {alvo} falhou: {r.result}, {r.totalErrors} erro(s). Veja o Console.");
            }
        }
    }
}
