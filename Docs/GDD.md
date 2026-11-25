# Jogo da Pizzaria 3D — Documento de Produção

## Visão geral

Jogo 3D em primeira pessoa, feito em **Unity**, em que o jogador é o cozinheiro de uma pizzaria. Ele anda livremente pela cozinha, com as mãos visíveis na tela, recebe pedidos, monta as pizzas, assa no forno e coloca no balcão. O objetivo de cada dia é **sobreviver até a pizzaria fechar** sem deixar os clientes bravos demais. O jogo será publicado no **itch.io**.

- **Engine:** Unity
- **Gráficos:** 3D, visual bonito e agradável
- **Câmera:** primeira pessoa, com as mãos do cozinheiro aparecendo
- **Gênero:** simulação/gerenciamento de tempo em restaurante
- **Publicação:** itch.io

## Estrutura de níveis

O jogo é um fim de semana de trabalho dividido em três dias, cada um mais difícil que o anterior.

| Dia | Clientes | Sabores | Paciência dos clientes | Observações |
| --- | --- | --- | --- | --- |
| Segunda-feira | Poucos | Fáceis (mussarela, marguerita) | Alta | Funciona como tutorial: tudo bem explicado |
| Sábado | Mais clientes | Sabores mais diferentes e um pouco mais difíceis | Média | Aumenta o ritmo |
| Domingo | Muitos clientes | Sabores complicados (podem ser inventados pela equipe) | Baixa: ficam bravos mais rápido | Dia mais difícil |

### Números de cada dia (definidos em 23/11)

Valores iniciais, a ajustar com testes. Ficam nos assets `Assets/_Projeto/Dados/Dias/*.asset` e podem ser editados no Inspector sem mexer no código.

| | Segunda | Sábado | Domingo |
| --- | --- | --- | --- |
| Duração até fechar | 3:00 | 3:30 | 4:00 |
| Intervalo entre clientes | 28–38 s | 18–26 s | 13–20 s |
| Paciência do cliente | 130 s | 95 s | 75 s |
| Comandas abertas ao mesmo tempo | 2 | 3 | 4 |
| Clientes perdidos para a derrota | 3 | 3 | 4 |
| Dicas de tutorial na tela | sim | não | não |
| Cardápio | Mussarela, Marguerita | + Calabresa, Napolitana | Marguerita, Calabresa, Napolitana, Pizza do Chef, Domingueira |

### Receitas

A ordem é obrigatória; o número é a quantidade de cliques.

| Pizza | Receita | Forno | Preço |
| --- | --- | --- | --- |
| Mussarela | Massa 1 › Molho 2 › Queijo 3 | 8 s | R$ 20 |
| Marguerita | Massa 1 › Molho 2 › Queijo 2 › Tomate 3 › Manjericão 2 | 9 s | R$ 25 |
| Calabresa | Massa 1 › Molho 2 › Queijo 2 › Calabresa 3 › Cebola 2 | 9 s | R$ 28 |
| Napolitana | Massa 1 › Molho 2 › Queijo 3 › Tomate 2 › Azeitona 2 | 9 s | R$ 28 |
| Pizza do Chef *(inventada)* | Massa 1 › Molho 1 › Queijo 2 › Calabresa 2 › Cebola 1 › Azeitona 2 › Manjericão 1 | 10 s | R$ 38 |
| Domingueira *(inventada)* | Massa 1 › Molho 3 › Queijo 1 › Tomate 2 › Calabresa 2 › Manjericão 2 | 10 s | R$ 36 |

Depois do ponto, a pizza tem **6 s** até queimar.

## Loop principal de jogo

1. Um pedido chega no **painel de pedidos** da cozinha.
2. O jogador consulta a **receita** da pizza pedida.
3. Vai até os pontos da cozinha e **pega os ingredientes**.
4. **Monta a pizza na bancada**, um clique por ingrediente, **na ordem da receita**.
5. Coloca a pizza no **forno** e liga o timer.
6. Quando o **apito** toca, a pizza está pronta e deve ser retirada.
7. Coloca a pizza no **balcão**.
8. Um **garçom** (NPC) pega a pizza e leva ao cliente.
9. O cliente aceita e paga, ou recusa a pizza.

## Pedidos e receitas

- Cada pedido aparece no painel de pedidos, no lugar onde os cozinheiros veem as comandas.
- Cada receita especifica:
  - Quantidade de **massa**
  - Quantidade de **molho**
  - Quantidade de **queijo** e demais ingredientes
  - **Tempo de forno**
- A ordem dos ingredientes na receita é obrigatória na montagem.

## Cozinha e ingredientes

- O jogador anda livremente pela cozinha em 3D.
- O local de cada ingrediente deve ser **muito bem sinalizado** (placas, ícones ou destaque visual), para o jogador sempre saber onde pegar cada coisa.
- O jogador pode pegar ingredientes em quantidade, por exemplo bastante massa de uma vez, para fazer várias pizzas seguidas.

## Montagem

- A montagem acontece na **bancada**.
- Cada ingrediente é colocado com **um clique**.
- É preciso seguir a **ordem da receita**; pizza montada fora do padrão conta como errada.

## Forno

- O jogador coloca a pizza no forno e ela entra num **timer**.
- Um **apito** avisa quando a pizza está pronta.
- Se ficar tempo demais, a pizza **queima** e conta como errada.

## Entrega e clientes

- A pizza pronta vai para o **balcão**; o garçom faz a entrega.
- **Pizza errada** (ingrediente faltando, ordem errada ou queimada): o cliente recusa, o garçom **devolve** a pizza e o jogador precisa **refazer**.
- Cada cliente tem uma **paciência**; se esperar demais, **vai embora**.
- A paciência diminui mais rápido nos dias mais difíceis.

## Economia e pontuação

- O jogador ganha **dinheiro** por pizza entregue corretamente.
- Clientes mais satisfeitos (atendidos rápido e corretamente) dão **gorjeta maior**.
- Não há meta de dinheiro para passar de dia.

- Gorjeta: até **50% do preço**, proporcional à paciência que o cliente ainda tinha na hora da entrega.

### Tabela de recordes

- Top 10 salvo no próprio computador (PlayerPrefs; no navegador também funciona).
- Ordem: **dias completos** primeiro, desempate pelo **dinheiro total**.
- Na vitória ou na derrota o jogador digita o nome (até 12 letras) e clica em **Salvar recorde**. Se for o 1º lugar, aparece "NOVO RECORDE!".
- Cada partida tem um id. Se o jogador perder, tentar o dia de novo e salvar outra vez, a mesma linha é atualizada (não duplica).
- O botão **Recordes** do menu mostra a tabela, com a partida atual destacada.

## Condições de vitória e derrota

- **Vitória do dia:** sobreviver até a pizzaria fechar. Quando o relógio zera, **param de chegar clientes**, mas as comandas abertas ainda precisam ser atendidas (ou os clientes vão embora). O dia termina quando não sobra nenhuma comanda.
- **Vitória do jogo:** completar os três dias.
- **Derrota:** quando o número de **clientes que foram embora** chega ao limite do dia (3, 3 e 4). Pizza recusada não conta direto, mas faz perder tempo. Na derrota o jogador pode **tentar o mesmo dia de novo** ou recomeçar o fim de semana, para não precisar repetir os dias que já venceu.

## Estados do jogo e tratamento de erros

```
Menu ──Jogar──► Jogando ◄──Continuar── Pausado
                  │  ▲                     ▲
                  │  └──── Esc/P, perder foco ou cursor ┘
                  ├── relógio zerou e sem comandas ──► Fim do dia ──Próximo dia──► Jogando (dia seguinte)
                  ├── no último dia ──► Vitória ──► Jogar de novo / Menu
                  └── clientes perdidos ≥ limite ──► Derrota ──► Tentar o dia de novo / Recomeçar / Menu
```

- Toda tela tem saída (continuar, reiniciar ou voltar ao menu); nunca é preciso fechar o jogo.
- Reiniciar recarrega a cena: nenhuma pizza, pedido ou garçom fica num estado inválido.
- Prevenção de becos sem saída:
  - ingredientes são infinitos, e a lixeira esvazia as mãos ou joga a pizza fora;
  - pizza errada é devolvida ao balcão com o motivo ("faltou Queijo", "fora de ordem", "queimada");
  - forno e balcão cheios avisam o jogador, e o garçom espera vaga para devolver;
  - pizza tirada crua do forno pode voltar e continua de onde parou;
  - se o jogador cair do cenário, volta ao ponto inicial.
- **Tutorial (segunda-feira):** uma dica na parte de baixo da tela olha o estado da cozinha e diz o próximo passo da comanda mais antiga.

## Direção de arte

**Cantina italiana aconchegante, low-poly estilizado.** Formas simples e arredondadas, sem texturas realistas. O clima vem da luz e da cor.

- **Hora do dia:** fim de tarde. O sol baixo e dourado entra pela janela da cozinha e desenha um retângulo no chão. Dentro, quem ilumina são as luminárias pendentes e o fogo do forno a lenha, que tremula.
- **Paleta:** creme (paredes, azulejo metrô, papel), terracota (piso xadrez, cúpula do forno, vasos), verde-garrafa esmaltado (barrado de azulejo, luminárias, janela), madeira escura (vigas, molduras, lambri) e pontos de **vermelho-tomate** (punho do dólmã, latas, toalhas, botões da interface). O verde-branco-vermelho aparece em detalhes: bandeirinhas, faixa do balcão e um quadro.
- **Leitura de jogo primeiro:** cada estação mostra o próprio ingrediente modelado (bolas de massa, tigela de molho, tomates, vaso de manjericão...) na cor dele, e é isso que acende ao mirar. As placas coloridas continuam grandes e com moldura.
- **Vida no cenário:** fagulhas saindo do forno, luzes tremulando, garçom de bigode que balança ao andar, clientes no salão que respiram e olham em volta, letreiro neon "PIZZARIA".
- **Pós-processamento:** tonemapping ACES, bloom (lâmpadas, brasa, neon), leve vinheta e temperatura quente, além de SSAO para dar volume às formas simples.
- **Interface:** a mesma paleta. Botões vermelho-tomate arredondados, caixas de madeira translúcida com filete dourado, comandas como papel creme e dicas em verde-manjericão.

Tudo é gerado por código (malhas, texturas, materiais e cena), então qualquer ajuste é refeito com **Pizzaria > Gerar Cozinha Greybox**. Os valores de luz e pós ficam em `Assets/_Projeto/Configuracoes/PosCantina.asset` e podem ser mexidos no Inspector.

## Som e música

Tudo é sintetizado por código na hora (nenhum arquivo de áudio), em `Scripts/Audio`.

- **Música:** uma tarantela em lá menor (6/8), com bandolim em trêmolo, acordeão no "pum-pá-pá", baixo e pandeiro. Ela toca sem parar e muda com o estado:
  - abaixa na pausa;
  - some enquanto toca a fanfarra de fim de dia, vitória ou derrota;
  - fica um pouco mais rápida a cada dia e nos últimos 30 segundos antes de fechar.
- **Efeitos:**
  - pegar ingrediente;
  - colocar na pizza;
  - pá do forno;
  - apito de pizza pronta;
  - pizza queimando;
  - sininho do balcão;
  - "ding-dong" de nova comanda;
  - caixa registradora no pagamento;
  - cliente indo embora;
  - lixeira;
  - passos;
  - clique dos botões.
- **Ambiente 3D em loop:** fogo crepitando no forno e burburinho com talheres no salão. O som fica mais alto perto da fonte.
- **Opções:** volumes de música e de efeitos, mais a sensibilidade do mouse. Fica no menu e na pausa e é salvo entre sessões.

## Controles

| Ação | Tecla |
| --- | --- |
| Andar / correr / olhar | WASD / Shift / mouse |
| Usar | Clique esquerdo ou E |
| Ação secundária (pegar pizza, devolver ingrediente, tirar do forno antes da hora) | Clique direito ou Q |
| Escolher ingrediente na mão | Roda do mouse ou 1–9 |
| Pausar | Esc ou P |

Sensibilidade do mouse e volumes de música/efeitos na tela **Opções** (menu e pausa), salvos entre sessões.

## Histórico de mudanças

- **23/11:** loop principal implementado (bancada, forno, balcão, garçom, comandas). Definidos os 3 dias, receitas novas (Calabresa, Napolitana, Pizza do Chef, Domingueira), ingredientes novos (Calabresa, Cebola, Azeitona), a regra de derrota, o fechamento da pizzaria, o menu, a pausa e o tutorial. Texturas procedurais nos materiais.

- **23/11 (tarde):** fase de arte. Definida a direção de arte (cantina italiana low-poly ao fim de tarde). Forno a lenha com cúpula, fogo e fagulhas, estações com ingredientes modelados, janela com sol, prateleiras, panelas, salão com mesas, clientes animados, letreiro neon, pós-processamento e interface com a mesma paleta.
- **23/11 (noite):** mais detalhes. Ingredientes com modelo próprio na pizza (rodela de tomate com sementes, folha de manjericão, calabresa com gordurinha, anel de cebola, azeitona fatiada, borda estufada da massa, queijo com bolhas) e na mão do cozinheiro. Estações com montagens caprichadas, bancadas com portas almofadadas, puxadores, borda de mármore e utensílios. Personagens com rosto (olhos que piscam, sobrancelhas, boca, bochechas), 5 cortes de cabelo, óculos, barba, boina, copo na mão (gesto de beber). O garçom ganhou colete, gravata-borboleta, bigode e pano no braço.

- **24/11:**
  - **Correções:** textos 3D agora respeitam profundidade, com o shader `Pizzaria/Texto3D` (o letreiro não atravessa mais a parede), e os nomes longos nas placas encolhem para caber.
  - **Áudio:** trilha sonora (tarantela), efeitos novos e sons de ambiente.
  - **Telas:** tela de Opções com volumes e tabela de recordes (top 10) com nome do jogador.

## Pontos em aberto

- Upgrades da cozinha ficam para depois da entrega.
- Sons de verdade (CC0) são opcionais: a trilha e os efeitos já são gerados por código. Modelos importados também são opcionais, porque o cenário já tem identidade própria.
- Balancear os números dos dias com playtests.
