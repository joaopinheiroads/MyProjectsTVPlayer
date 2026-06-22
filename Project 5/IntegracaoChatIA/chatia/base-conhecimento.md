# Base de Conhecimento — Painel

> Documento que alimenta o assistente de ajuda (IA). Ele responde **apenas** com base no que está aqui.
> Tema: **como o painel funciona, onde encontrar cada função e como configurar**. O assistente NÃO consulta dados do cliente (lista de terminais, campanhas, etc.) — apenas orienta.
> Público principal: **cliente** (usuário que opera o painel no dia a dia).

---

## 0. Como navegar no painel

- O painel tem uma **barra lateral (menu)** à esquerda, organizada em grupos. Ao clicar num item, a tela correspondente abre na **área de conteúdo** à direita.
- No topo da lateral ficam seu **avatar, nome e perfil**. Em "Editar dados" / "Meus Dados" você altera seus dados de usuário.
- O **sininho** (🔔) mostra novidades da plataforma.
- No rodapé da lateral: engrenagem de **Configurações** (cor do menu, modo escuro), "Meus Dados" e o botão **Sair**.
- Recolher/expandir o menu: ícone de seta no topo / ícone de "três barras".

Grupos do menu (cliente): **Gerenciamento, Conteúdo, Plugins, Relatórios, Ferramentas, Suporte, Ajuda, Downloads**.

**Glossário rápido:**
- **Player / Terminal:** o equipamento (Windows ou Android) que exibe o conteúdo na tela/TV.
- **Mídia:** arquivo de vídeo ou imagem enviado ao repositório.
- **Programação:** conjunto ordenado de mídias/plugins que o player exibe, agendado por horário.
- **Campanha:** conjunto de mídias agrupado, que é inserido dentro de uma ou mais programações por um período.
- **Timeline:** a linha do tempo de exibição de um player específico, onde as programações são **chamadas/agendadas** — é a **única** forma de levar uma programação ao player.
- **Comunicar:** o player "comunica" quando está online e conversando com o servidor.

---

## 1. Gerenciamento

### 1.1 Dashboard
Menu: **Gerenciamento › Dashboard**. Painel visual com a situação geral da sua operação.
- **Cartões no topo:** "Quantidade de players" (contratados + em pausa + em demonstração), "Arquivos no repositório" (com barra de espaço usado/limite) e "Arquivos não usados". Os cartões são clicáveis e levam ao Painel de Controle / Repositório.
- **Gráficos:** Status da conexão (Offline / Online / Comunicou hoje) para players contratados, aguardando e em demonstração; Plataforma (Windows × Android); Versão do player (Atualizado × Desatualizado) com botão "info" que lista os desatualizados; Tipo de conexão (WiFi × Ethernet); comunicação nos últimos 7 dias; e "Status da mídia nos players" (Baixada × Não baixada).

### 1.2 Painel de Controle
Menu: **Gerenciamento › Painel de Controle**. Tela central para **gerenciar todos os players** e enviar comandos remotos.
- Players listados em grade, **agrupados por Empresa e por Status** (Online/Offline). Ícones mostram: tipo de conexão (WiFi/Ethernet), plataforma (Windows/Android), status (bolinha verde = comunicando), "% HD" (espaço livre em disco).
- Colunas: "ID", "Player", "Empresa", "Última Conexão", "Instalação" (data + dias), "Versão" (player e agente), "Cidade", "Status do Player".
- Botão **"Opções"** (com players selecionados pelo checkbox) envia comandos: **Reiniciar players**, **Reiniciar máquinas**, **Desligar máquinas**, **Excluir conteúdo**, **Apagar plugins e rss**, **Enviar email**, e **Bloquear/Desbloquear player** (somente admin). Sempre pede confirmação.
- Botões: "Expandir"/"Recolher" (grupos), "Atualizar", "Exportar" (PDF/DOCX/XLSX), e toggle **"Ativos / Excluídos"**.
- Por player, na coluna de ações: ícone **info** (detalhes do player), ícone **email** (receber alerta quando o player não comunicar por mais de 1h) e ícone **play** ("Programação" — abre as programações daquele player).
- **"Adicionar player"** (no menu Opções) registra um novo player — não aparece para contas em demonstração.

### 1.3 Contratar
Menu: **Gerenciamento › Contratar** (aparece apenas quando há player em demonstração). Leva aos dados de contrato para efetivar a contratação.

### 1.4 Configurações (submenu)
- **Controle de Brilho** — agenda o brilho da tela por horário (ver 1.4.1). *Apenas Windows.*
- **Divisão de tela** — cria layouts que dividem a tela em áreas (ver 1.4.2).
- **Envio de email** — configura o envio de e-mails (relatórios/avisos).
- **Tela de bloqueio** — tela exibida quando o player está bloqueado. *(somente admin da conta)*
- **Funcionamento Player** — horários em que o player liga/desliga (ver 1.4.3). *Apenas Windows.*
- **Layout do Player** — monta o layout visual do player (ver 1.4.4).
- **Layout do RSS** — aparência da barra/feed de RSS.
- **Logo do Relatório** — logo que aparece nos relatórios.

#### 1.4.1 Controle de Brilho
1. Clique em **"Novo"** (+) e dê uma **"Identificação"** (nome do controle).
2. Expanda a linha e adicione horários: **"Hora Início"**, **"Hora Fim"** e o nível de **"Brilho"** (0 a 100). Salve cada horário.
3. Clique em **"Aplicar Controle"** e selecione os players que receberão esse controle.
4. O botão **"Listagem dos Players"** mostra quais players já usam o controle. *(Disponível apenas para players Windows.)*

#### 1.4.2 Divisão de tela
1. Clique em **"Novo"** (+); abre a tela de edição.
2. Defina o **"Nome do Layout de Divisão de Tela"** e a **quantidade de divisões** (áreas) da tela.
3. Salve e clique em **"Aplicar Layout"** para escolher os players. (Colunas da lista: "Nome do Layout", "Qtd. de Divisões", "Data de Cadastro".)

#### 1.4.3 Funcionamento Player
1. A tela lista seus players Windows.
2. Clique em **"Editar"** no player desejado e defina **Hora Inicial** e **Hora Final** (horário em que o player liga/desliga). *(Disponível apenas para players Windows.)*

#### 1.4.4 Layout do Player
1. Clique em **"Novo layout"** (+) e escolha o modelo: **"Novo Layout do Player"** ou **"...para Android 5.6+ (Modelo Novo)"**.
2. Configure os elementos (Exibe Relógio, Exibe Barra, Exibe Logo, Áudio) e dê um nome.
3. Salve e clique em **"Aplicar Layout"** para escolher os players. Use **"Filtrar por player"** para localizar.

### 1.5 Controle de acesso (submenu — somente admin da conta)
- **Empresas** — cadastro/gestão das empresas da conta.
- **Usuários** — gestão de usuários do painel.
- **Perfis** — perfis de permissão (o que cada usuário pode ver/fazer).
- **Player por empresa** — move/associa terminais a empresas.
- **Mensageiro** — chaves do Mensageiro.

### 1.6 Monitoramento (submenu)
- **Gráfico por player** — monitoramento visual (thumbnails/estado) por player.
- **Gráfico da rede** — monitoramento da rede de players.

---

## 2. Conteúdo

Grupo para enviar mídia e montar o que toca em cada player.

### 2.1 Enviar vídeos/imagens
Menu: **Conteúdo › Enviar vídeos/imagens**.
- Mostra o **"Espaço disponível em disco"** e os **formatos suportados**:
  - **Windows:** jpg, jpeg, jpe, png, bmp, wmv, mp4, avi, mpeg, mpg, mkv, webm.
  - **Android:** jpg, jpeg, png, mp4.
- **Tamanho máx. por arquivo:** 200 MB (pode variar conforme o limite da conta). **Nome do arquivo:** até 45 caracteres, sem os caracteres `\ / | ? * : < >`.
- Passos:
  1. (Opcional) marque "Definir em qual(is) programação(ões)/campanha(s) será inserido ao final do upload" e/ou "Fazer a substituição de mídia ao final do upload".
  2. Escolha a **"Pasta para upload dos arquivos"**.
  3. (Se tiver várias empresas) clique em "Definir a qual(is) empresa(s) o vídeo pertencerá".
  4. Clique em **"Procurar..."** ou arraste os arquivos; use **"Adicionar"** para mais arquivos.
  5. Clique em **"Enviar"** e acompanhe a barra de progresso.

### 2.2 Enviar áudio
Menu: **Conteúdo › Enviar áudio**.
- Formatos: **.mp3 e .m3u**, até **8 MB por arquivo** (pode variar pela conta).
- Passos: (opcional) marque **"Definir playlist ao final"**, escolha a pasta, adicione os arquivos (**"Procurar"/"Adicionar"/"Remover"**) e clique em **"Enviar"**.

### 2.3 Repositório
Menu: **Conteúdo › Repositório**. Biblioteca das mídias enviadas, em árvore de pastas.
- Topo: **"Espaço utilizado"**, **"Quantidade total de mídias"** e botão **"Arquivos não usados"** (limpeza).
- Barra de ferramentas: **Criar (F7)** pasta, **Renomear (F2)**, **Mover (F6)**, **Excluir (Del)**, **download**, **Atualizar**, alternar visão **Thumbnails / Detalhes**, e filtros **Exibir todos / apenas com uso / apenas sem uso**.
- Colunas (modo Detalhes): "Nome", "Data Upload", "Tamanho", "Duração", "Resolução", "Formato", "Em Uso".
- **Clique com o botão direito sobre uma mídia** para as ações: **Tempo de Vida** (duração de exibição), **Não exibir mídia no player**, **Vincular mídia em programação/campanha**, **Remover de todas programações/campanhas**, **Agendamento individual**, **Visualizar**, **Players com essa mídia** e **Substituir mídia**.
- Regras: só apaga pastas vazias; pastas do sistema não podem ser excluídas; visualizar/substituir valem para MP4 e imagens.

### 2.4 Campanhas
Menu: **Conteúdo › Campanhas**. Lista as campanhas com colunas "Campanha", "Categoria", e marcadores **"Vencida" / "Ativa" / "Futura"**.
- **Criar:** clique em **"Novo"**; escolha **"Nova Campanha"** ou **"Campanha baseada em outra já existente"** e clique em **"Continuar"**. Abre o editor de conteúdo (ver 2.6).
- **Editar / Excluir:** ícones na linha. **Vincular** a uma programação: ícone "Vincular campanha em uma programação".
- Expandindo a linha, você vê em quais players e programações a campanha está, com "Início", "Fim" e "Quantidade", e pode remover de uma programação.
- **Tipo da campanha** (no editor): "Exibir 1 item da campanha por vez" ou "Exibir todos os itens em sequência", com data de Início/Fim opcional.

### 2.5 Programações
Menu: **Conteúdo › Programações**. Lista as programações (colunas "Programação", "Categoria"); há filtro por player no topo.
- **Criar:** **"Novo"** → "Nova Programação" ou "baseada em outra existente" → **"Continuar"**. Abre o editor de conteúdo (ver 2.6).
- Expandindo a linha, vê os agendamentos por player: "Hora Início", "Hora Fim", **"Recorrência"** (Sem Recorrência / Dia da Semana / Todos os Dias / Dia Excluído), **"Loop"** e **"Fura-Fila"**. É possível excluir um agendamento específico sem apagar a programação.
  - **Loop:** repete a programação continuamente. **Fura-Fila:** interrompe programações de menor prioridade.
- **Importante:** montar/editar a programação aqui **não** a coloca no player. Para ela tocar, é preciso **chamá-la na Timeline** do player (ver 2.7) — essa é a **única** forma de levá-la ao terminal. Nesta tela você apenas **vê/remove** onde ela já está agendada.

### 2.6 Editor de Conteúdo (usado por Campanhas e Programações)
Campanhas e Programações abrem o mesmo editor:
1. Preencha o **"Nome"** (obrigatório) e, opcionalmente, a **"Categoria"** (o botão "+" cria categoria nova).
2. Em **"Conteúdo disponível"** (lista de vídeos, imagens, RSS, plugins) **arraste** os itens para **"Conteúdo Selecionado"**.
3. **Reordene** arrastando; use "Alterar posição", "Excluir Selecionados" ou "Limpar Programação". Cada item mostra Ordem, Nome, Tempo (duração) e Tamanho, além de alertas (mídia pesada, campanha expirada).
4. Dá para adicionar vídeo do **YouTube**, **plugin de Futebol** e ajustar o **"Tempo do conteúdo"** (segundos) de plugins.
5. Clique em **"Salvar"** (exige ao menos 1 item).

### 2.7 Timeline
Menu: **Conteúdo › Timeline**. É **aqui** que você **chama uma programação para tocar num player** — a **única** forma de levar uma programação montada até o terminal. Edita a linha do tempo de **um player específico**.
1. Selecione o **"Player"** e a **"Timeline"**.
2. Aparecem os painéis **"Programações Agendadas"** (grade) e **"Timeline"** (agenda visual de 24h).
3. Na agenda, selecione um horário, clique com o botão direito e escolha **"Editar Programação"** para definir o agendamento: **qual programação** vai tocar, hora início/fim, recorrência, Loop e Fura-Fila. Também é possível **arrastar** os agendamentos para mudar o horário.
4. **"Copiar Agendamentos"** replica a timeline para outros players; **"Apagar todos agendamentos"** limpa tudo (com confirmação); **"Alterar nome"** renomeia a timeline.

### 2.8 Agendamento de Playlist
Menu: **Conteúdo › Agendamento de Playlist**. *(Funciona em players Windows 4.2+ e Android 3.9+.)*
1. Clique em **"Novo"** e dê um **"Nome do agendamento"**.
2. Selecione playlists no combo (cada uma mostra "Nome: Duração"), clique em **"Adicionar"**; reordene arrastando na lista "Playlists selecionadas".
3. Use o ícone de engrenagem para **"Criar/Editar configurações"** por dia/hora de cada playlist.
4. Salve e clique em **"Vincular"** para aplicar o agendamento aos players.

### 2.9 Playlist de áudio
Menu: **Conteúdo › Playlist de áudio**. Monta playlists com os áudios enviados.

### 2.10 Vídeos por período
Menu: **Conteúdo › Vídeos por período**. Define vídeos que tocam apenas em determinado período.

---

## 3. Plugins

Conteúdos dinâmicos extras exibidos no player.

### 3.1 Barra de Texto Rotativo (submenu)
- **Conteúdo próprio** — texto rotativo escrito pelo próprio cliente.
- **Feed RSS** — barra de texto alimentada por um feed RSS.

### 3.2 Tela Inteira (submenu) — plugins que ocupam a tela
- **Feed RSS** — notícias via RSS em tela cheia.
- **Futebol** — jogos/placar de futebol.
- **HTML5** e **HTML5 - Feeds** — plugins em HTML5 (estáticos ou alimentados por feed).
- **Redes Sociais** — conteúdo de redes sociais.
- **Relógio Analógico** — relógio na tela.
- **Yahoo Clima** / **Nova Previsão do Tempo** — previsão do tempo.
- **Website** — exibe um site/URL no player.
- **Plugin Balança** — integração com balança.
- **Plugin Combustível** — exibe preços de combustível.
- **Plugin Screen Capture** — captura de tela como conteúdo.

---

## 4. Relatórios

### 4.1 Comunicação (submenu) — players online/comunicando
- **Dias comunicando** — dias em que cada player comunicou, por período.
- **Dias sem comunicar** — calendário por player mostrando os dias offline no período. Filtros: **Período (De/Até)**, Sistema Operacional, Tipo de Conexão, Player.
- **Tempo de comunicação** / **Tempo consolidado** — tempo total de comunicação dos players.
- **Última comunicação** — lista com a **data da última comunicação** e a coluna **"Dias OFF"**. Filtros: S.O., Tipo de Conexão, Player, **"Dias sem comunicar (≥)"** e ordenação. Colunas: "Player", "S.O", "Conexão", "Data", "Dias OFF", "Versão".

### 4.2 Exibições por mídia (submenu)
- **Detalhado / Detalhado 2** — quantas vezes cada mídia foi exibida.
- **Consolidado / Consolidado 2** — exibições por mídia, consolidado.

### 4.3 Exibições por player (submenu)
- **Detalhado / Consolidado** — exibições por player.

### 4.4 Mídia (submenu)
- **Mídia em uso** — mídias atualmente em uso.
- **Mídia excluída** — mídias removidas.
- **Mídia não baixada** — mídias que os players ainda não baixaram.
- **Status da mídia** — escolhe uma mídia e mostra os players agrupados em **"Baixado"** (verde) × **"Não Baixado"** (vermelho), com a data/hora da última comunicação de cada um.

- **Painel de senhas** — relatório do chamador de senhas.

> Todos os relatórios permitem **imprimir** e **exportar** (PDF, XLS/XLSX, RTF, HTML, TXT, CSV, PNG).

---

## 5. Ferramentas

- **Criador de conteúdo HTML5** — cria peças em HTML5.
- **Gerador de Chaves** — gera uma nova chave de ativação do player. O botão de gerar a chave **só fica disponível se o player estiver sem contato com o servidor há pelo menos 30 minutos**. Ver a FAQ "Como fazer o player pedir a chave de ativação novamente?".
- **Mensageiro** — templates do Mensageiro.
- **Mensagem de alerta offline** — mensagem exibida quando o player está offline.
- **Substituição de mídia** — troca uma mídia por outra em massa.
- **Editor de templates** — editor de modelos prontos.

---

## 6. Suporte

### 6.1 Abrir Chamado
Menu: **Suporte › Abrir Chamado**.
1. Preencha **"Email de contato"** e **"Telefone de contato"** (obrigatórios).
2. Selecione o **"Player"** afetado.
3. (Opcional) **"Anexar imagem"** (jpg, jpeg, jpe, png, bmp).
4. Escreva a **"Descrição do problema"** e clique em **"Enviar"**. O chamado vai para a equipe de suporte.

### 6.2 Suporte WhatsApp
Menu: **Suporte › Suporte WhatsApp**. Abre o atendimento via WhatsApp.

### 6.3 Críticas & Sugestões
Menu: **Suporte › Críticas & Sugestões**. Envia uma crítica ou sugestão.

---

## 7. Ajuda

- **Log de alterações** — histórico de mudanças/novidades da plataforma.
- **Tutoriais** — vídeos tutoriais.

---

## 8. Downloads

Instaladores e ferramentas para baixar:
- **Chamador de senhas** (Windows e Android)
- **Conversor de mídia**
- **Instalador do player** (Android / Windows)
- **Instalador do agente** (Android)
- **Mensageiro**
- **Instalador Player/Agente Led TB**

---

## 9. Perguntas frequentes (FAQ)

**Como envio um vídeo ou imagem para o player?**
1. Em **Conteúdo › Enviar vídeos/imagens**, escolha a pasta, adicione os arquivos e clique em "Enviar".
2. Monte uma **Programação** em **Conteúdo › Programações** e inclua a mídia nela (a programação também pode conter **Campanhas**).
3. Vá em **Conteúdo › Timeline**, selecione o **player** e **chame essa programação** no horário desejado. **Essa é a única forma de levar a programação até o terminal.**
O player baixa a mídia na próxima comunicação.

**Como crio uma campanha e mando para os players?**
A campanha **não vai direto** para o player — ela entra **dentro de uma programação**:
1. Em **Conteúdo › Campanhas**, clique em "Novo", monte o conteúdo (arrastando mídias) e salve.
2. Use o ícone **"Vincular campanha em uma programação"** para inseri-la numa **programação**.
3. Garanta que essa **programação esteja chamada na Timeline** do(s) player(s) (**Conteúdo › Timeline**) — é assim que ela chega ao terminal.

**Qual a diferença entre Programação, Campanha e Timeline?**
A **Campanha** é um pacote de mídias que você insere **dentro de uma Programação** (por um período); a **Programação** é a lista de conteúdos que toca; a **Timeline** é onde você **chama a Programação** no horário de cada player. **A única forma de uma Programação tocar num player é sendo chamada na Timeline.**

**Por que meu player está offline / não comunica?**
Siga este roteiro nos relatórios:
1. **Relatórios › Comunicação › Última comunicação** — confirme se o player está offline (coluna "Dias OFF" > 0) e veja a data da última comunicação.
2. **Relatórios › Comunicação › Dias sem comunicar** — defina um período e veja desde quando ele parou e se é intermitente.
3. Verifique no **Painel de Controle** o status, a "% HD" (disco cheio pode travar) e a versão do player.
4. Se persistir, **Suporte › Abrir Chamado** informando o player, "offline desde [data]" e os "Dias OFF".
*(Causas comuns externas: internet/energia no local, player desligado fora do horário de funcionamento, disco cheio.)*

**Como vejo se a mídia já foi baixada pelo player?**
Use **Relatórios › Mídia › Status da mídia**: selecione a mídia e veja os players em "Baixado" (verde) ou "Não Baixado" (vermelho). O Dashboard também mostra "Status da mídia nos players".

**Como configuro o horário em que a tela liga/desliga?**
Em **Gerenciamento › Configurações › Funcionamento Player**, clique em "Editar" no player e defina Hora Inicial e Final. *(Apenas players Windows.)*

**Como reinicio ou desligo um player remotamente?**
No **Painel de Controle**, selecione o(s) player(s) pelo checkbox, clique em **"Opções"** e escolha "Reiniciar players", "Reiniciar máquinas" ou "Desligar máquinas".

**Como recebo aviso quando um player cair?**
No **Painel de Controle**, no ícone de **email** da linha do player, ative o alerta para ser avisado quando ele ficar mais de 1h sem comunicar.

**Como fazer o player pedir a chave de ativação novamente? / Como reativar um player?**
Para o player/terminal voltar a pedir a chave de ativação na tela, é preciso **reinstalar o aplicativo** (mesmo que ele estivesse funcionando normalmente).
Para gerar a nova chave em **Ferramentas › Gerador de Chaves**, o sistema **só libera o botão de gerar a chave se o player estiver sem contato com o servidor há pelo menos 30 minutos**.
Se o player teve contato com o servidor (ou estava ligado), o botão não aparece — nesse caso, **solicite uma nova chave pelo Suporte** (**Suporte › Abrir Chamado**) e refaça a reinstalação.

---

## 10. Sobre o sistema — negócio, hardware e requisitos (FAQ geral)

**O que é o Player?**
O Player é a versão do software que roda no local de exibição (stand alone). Ele se comunica com um servidor que roda a versão WEB, onde você faz a gestão de conteúdo da sua rede.

**Tenho mais de uma TV no mesmo estabelecimento — preciso de mais licenças?**
Não necessariamente. Se for possível distribuir o sinal da máquina onde o Player está instalado, não precisa de licença extra (ex.: um cabo VGA "Y" para 2 monitores, ou um divisor de sinal para mais TVs).

**Consigo gerenciar o conteúdo da rede pela web?**
Sim. O gestor de conteúdo é 100% WEB — você gerencia de qualquer computador ou dispositivo móvel com acesso à internet.

**Qual a configuração de máquina para rodar o Player?**
Há uma configuração mínima recomendada (consulte os requisitos técnicos). Se suas máquinas tiverem outra configuração, fale com o suporte — é possível testar/homologar antes.

**Como solicito a versão demonstrativa?**
Pelo cadastro de demonstração (rápido e simples). Em caso de dúvida, fale com o suporte.

**O que preciso para montar um ponto de Digital Signage?**
Uma máquina (PC, netbook, nettop ou notebook) que atenda à configuração mínima, uma TV e internet no local.

**Funciona em telões de LED?**
Sim. O pré-requisito é que a máquina ligada ao telão esteja conectada à internet para a gestão de conteúdo.

**Se a internet do local cair, o conteúdo para de ser exibido?**
Não. O software tem inteligência embarcada: usa a rede apenas para atualizar o conteúdo. Depois de atualizado, o conteúdo roda localmente — se a internet cair, ele continua exibindo o que já foi baixado, dentro da programação criada.

**O software consome muita banda de internet?**
Não. A internet é usada apenas para atualizações; o conteúdo já baixado roda localmente, reduzindo o tráfego de rede.

**Dá para monitorar se os players estão ligados ou desligados?**
Sim. Há uma ferramenta de Keep Alive que mostra o status de cada player, com data e hora da última comunicação, e permite acompanhar o que está sendo exibido na rede (Mídia Indoor / TV Corporativa).

**Qual o custo do hardware para rodar o Player?**
Depende de fatores como layout do local, espaço, cabeamento e distâncias. Em locais onde não se pode alterar o layout, costumam-se usar máquinas menores (nettop, netbook).

**O software tem relatórios?**
Sim. Há relatórios para gestão e controle — úteis, por exemplo, para enviar a anunciantes (no caso de Mídia Indoor) com horários e pontos de exibição das mídias.
