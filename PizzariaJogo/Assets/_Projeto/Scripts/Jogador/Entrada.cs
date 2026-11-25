using UnityEngine;
using UnityEngine.InputSystem;

namespace Pizzaria.Jogador
{
    /// <summary>
    /// Ponto único de leitura de input (Input System). Mantém o resto do código
    /// independente de teclado/mouse, facilitando trocar por Input Actions depois.
    /// </summary>
    public static class Entrada
    {
        public static Vector2 Movimento()
        {
            var k = Keyboard.current;
            if (k == null) return Vector2.zero;

            float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                    - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
            float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                    - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);

            var v = new Vector2(x, y);
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        public static Vector2 OlharDelta()
        {
            var m = Mouse.current;
            return m != null ? m.delta.ReadValue() : Vector2.zero;
        }

        public static bool Correndo =>
            Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        /// <summary>Clique esquerdo ou tecla E.</summary>
        public static bool InteragirPressionado =>
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame);

        /// <summary>Clique direito ou tecla Q.</summary>
        public static bool InteragirSecundarioPressionado =>
            (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame);

        public static bool CliqueEsquerdo =>
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        /// <summary>Roda do mouse: -1 (para cima), +1 (para baixo) ou 0.</summary>
        public static int RodaMouse
        {
            get
            {
                var m = Mouse.current;
                if (m == null) return 0;
                float y = m.scroll.ReadValue().y;
                return y > 0.01f ? -1 : y < -0.01f ? 1 : 0;
            }
        }

        /// <summary>Índice (0-8) da tecla numérica 1-9 apertada neste frame, ou -1.</summary>
        public static int NumeroPressionado
        {
            get
            {
                var k = Keyboard.current;
                if (k == null) return -1;
                var teclas = new[] { k.digit1Key, k.digit2Key, k.digit3Key, k.digit4Key, k.digit5Key,
                                     k.digit6Key, k.digit7Key, k.digit8Key, k.digit9Key };
                for (int i = 0; i < teclas.Length; i++)
                    if (teclas[i].wasPressedThisFrame) return i;
                return -1;
            }
        }

        /// <summary>Esc ou P.</summary>
        public static bool PausarPressionado =>
            Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);

        public static bool SoltarCursorPressionado =>
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}
