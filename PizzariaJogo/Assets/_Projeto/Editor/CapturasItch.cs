using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pizzaria.EditorTools
{
    /// <summary>
    /// Tira screenshots da cozinha para a página do itch.io (1920x1080 + capa 630x500).
    /// Menu Pizzaria > Capturas para o itch.io, ou pela linha de comando:
    /// Unity.exe -batchmode -quit -projectPath PizzariaJogo -executeMethod Pizzaria.EditorTools.CapturasItch.Capturar
    /// As imagens vão para a pasta Itch/ ao lado do projeto.
    /// </summary>
    public static class CapturasItch
    {
        private static readonly (string nome, Vector3 pos, Vector3 alvo, float fov)[] Tomadas =
        {
            ("01_cozinha",      new Vector3(-4.2f, 2.3f, -3.2f), new Vector3(1.2f, 0.9f, 1.8f), 62f),
            ("02_ingredientes", new Vector3(-1.6f, 1.6f, 1.3f),  new Vector3(1.4f, 1.0f, 3.6f), 60f),
            ("03_forno",        new Vector3(1.0f, 1.55f, -1.6f), new Vector3(4.2f, 1.15f, 0.5f), 60f),
            ("04_letreiro",     new Vector3(2.0f, 1.6f, -4.7f),  new Vector3(-0.6f, 1.9f, -9f), 60f),
            ("05_balcao_salao", new Vector3(0.3f, 1.65f, -2.4f), new Vector3(-0.4f, 1.1f, -8f), 64f),
            ("06_salao",        new Vector3(4.3f, 2.1f, -8.6f),  new Vector3(-1f, 0.8f, -5f), 62f),
        };

        [MenuItem("Pizzaria/Capturas para o itch.io", priority = 40)]
        public static void Capturar()
        {
            string pasta = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Itch"));
            Directory.CreateDirectory(pasta);

            EditorSceneManager.OpenScene(GeradorCozinha.CaminhoCena, OpenSceneMode.Single);
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[Pizzaria] Sem câmera principal na cena."); return; }

            // a câmera do jogador vira câmera de foto: sem mãos na frente
            cam.transform.SetParent(null, true);
            foreach (var r in cam.GetComponentsInChildren<Renderer>(true)) r.enabled = false;

            foreach (var t in Tomadas)
                Salvar(cam, t.pos, t.alvo, t.fov, 1920, 1080, Path.Combine(pasta, t.nome + ".png"));
            var capa = Tomadas[0];
            Salvar(cam, capa.pos + new Vector3(0.6f, 0f, 0.4f), capa.alvo, 56f, 1260, 1000, Path.Combine(pasta, "00_capa_630x500.png"), 630, 500);

            Debug.Log($"[Pizzaria] Capturas salvas em {pasta}");
            // não salva a cena: a câmera movida fica só na memória
            EditorSceneManager.OpenScene(GeradorCozinha.CaminhoCena, OpenSceneMode.Single);
        }

        private static void Salvar(Camera cam, Vector3 pos, Vector3 alvo, float fov, int w, int h, string arquivo, int finalW = 0, int finalH = 0)
        {
            cam.transform.position = pos;
            cam.transform.rotation = Quaternion.LookRotation(alvo - pos);
            cam.fieldOfView = fov;

            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
            var antiga = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.Render(); // segunda passada: efeitos temporais/estado inicial do pós já assentados
            cam.targetTexture = antiga;

            var leitura = rt;
            if (finalW > 0)
            {
                leitura = new RenderTexture(finalW, finalH, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(rt, leitura);
            }
            int lw = leitura.width, lh = leitura.height;
            var ativa = RenderTexture.active;
            RenderTexture.active = leitura;
            var tex = new Texture2D(lw, lh, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, lw, lh), 0, 0);
            tex.Apply();
            RenderTexture.active = ativa;

            File.WriteAllBytes(arquivo, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            if (leitura != rt) Object.DestroyImmediate(leitura);
            Object.DestroyImmediate(rt);
        }
    }
}
