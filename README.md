# Gepeto's Pizzaria

Jogo 3D em primeira pessoa feito em **Unity 6 (URP)**. Você é o cozinheiro de uma pizzaria: lê as comandas, pega os ingredientes, monta as pizzas **na ordem da receita**, assa sem queimar e entrega no balcão. Sobreviva aos 3 dias do fim de semana sem deixar clientes demais irem embora bravos.

- **Jogar:** [_link do itch.io aqui_](https://viniciusdib.itch.io/gepetos-pizzaria)
- **GDD:** [`Docs/GDD.md`](Docs/GDD.md)

## Como jogar

| Ação | Tecla |
| --- | --- |
| Andar / correr | WASD / Shift |
| Olhar | Mouse |
| Usar (pegar ingrediente, colocar na pizza, pôr no forno, entregar) | Clique esquerdo ou **E** |
| Ação secundária (pegar a pizza, devolver ingrediente, tirar do forno antes da hora) | Clique direito ou **Q** |
| Escolher o ingrediente que está na mão | Roda do mouse ou **1–9** |
| Pausar | **Esc** ou **P** |

**Passo a passo:**
1. A comanda aparece no canto da tela e no painel **PEDIDOS**.
2. Pegue os ingredientes nas estações do fundo da cozinha.
3. Na **bancada**, clique uma vez por ingrediente, começando pela massa e na ordem da receita.
4. Pegue a pizza (botão direito), leve ao **forno** e tire quando apitar, antes que queime.
5. Deixe no **balcão**. O garçom leva ao cliente. Pizza errada volta para o balcão e vai para a **lixeira**.

A segunda-feira tem dicas na tela que guiam cada passo.

No fim da partida (vitória ou derrota), digite seu nome para entrar na **tabela de recordes** (menu → Recordes). Volumes de música e efeitos ficam em **Opções**.

## Rodando o projeto

1. Abra a pasta `PizzariaJogo/` no Unity Hub (versão **6000.6.6f1**).
2. Menu **Pizzaria > Gerar Cozinha Greybox** para gerar a cena `Assets/Scenes/Cozinha.unity` e os dados (ingredientes, receitas, dias).
3. Aperte Play.

**Builds:** **Pizzaria > Build Windows** (gera `Builds/Windows/GepetosPizzaria.exe`) ou **Pizzaria > Build WebGL**.

## Estrutura do código (`PizzariaJogo/Assets/_Projeto/Scripts`)

| Pasta | Conteúdo |
| --- | --- |
| `Jogador/` | Movimento em 1ª pessoa, entrada (Input System), inventário, mãos |
| `Interacao/` | Raycast de interação, base `Interagivel`, avisos globais |
| `Cozinha/` | Pizza (dados e visual), estações de ingrediente, bancada, forno, balcão, garçom, lixeira |
| `Pedidos/` | Comandas, paciência, pagamento/gorjeta, painel de pedidos |
| `Jogo/` | Máquina de estados (menu, jogando, pausa, fim do dia, derrota, vitória), dias, tutorial, placar de recordes |
| `Dados/` | ScriptableObjects: ingrediente, receita (o `DiaSO` fica em `Jogo/`) |
| `UI/` | HUD e telas (IMGUI) |
| `Audio/` | Música (tarantela), efeitos e sons de ambiente, todos sintetizados por código |
| `../Editor/` | Gerador da cena, texturas procedurais, menu de build |

## Créditos

**Equipe:** Vinicius Dib, Greta Faria, Thomas Rudge, Enzo Christofoletti

**Assets de terceiros:**
- _(nenhum até agora: a cena, as texturas e os sons são gerados por código no próprio projeto)_


**Ferramentas:** Unity 6 (URP), Input System. Parte do código foi escrita com auxílio do Claude Code (Anthropic).
