# Ateliê Hub — Documento de contexto para pesquisa de funcionalidades

> **Para que serve este documento:** dar a você (Gemini) o contexto completo do sistema **Ateliê Hub** para que você sugira **funcionalidades e melhorias de usabilidade úteis no dia a dia da operação** do cliente-alvo. O objetivo é descobrir o que sistemas parecidos (gestão para artesãos, ateliês, pequenos criadores que vendem sob encomenda) já oferecem de **prático e útil** — não apenas mais cadastros, listas e relatórios de números, mas **soluções que facilitam o trabalho real**.
>
> **O que eu quero de você:** ideias de funcionalidades/recursos que (a) **casem com o que o sistema já tem**, (b) sejam **úteis no dia a dia** de quem usa, e (c) **não estejam já no nosso plano futuro** (a seção "O que NÃO precisa sugerir" lista o que já está mapeado). Para cada ideia, diga: o que é, que dor resolve, e por que faz sentido para este perfil de cliente.

---

## 1. O que é o produto

Um sistema **desktop, 100% local** (roda em 1 PC, 1 usuária, **sem nuvem, sem internet obrigatória**) que **centraliza toda a operação** de quem vende arte/produtos criativos feitos sob encomenda e **toca o próprio negócio sozinho(a)**.

Hoje esse perfil de pessoa tem a informação espalhada (papel, cabeça, WhatsApp, caderno, planilha solta) e **nenhum lugar único** para enxergar e gerir o negócio. O Ateliê Hub quer ser esse lugar único.

É um produto **white-label** (genérico, pensado para ser revendido a vários ateliês/artistas). Existe um cliente **beta tester** real (uma marca de produtos artísticos personalizados) que valida na prática, mas **nada no sistema é amarrado a ela**.

### Filosofia do produto
- **Prático e rápido** — cada funcionalidade tem que entrar em uso real, sem fricção.
- **Leve e local** — roda num PC comum, sem depender de servidor/nuvem.
- **Para uma pessoa só** — não é ERP corporativo; é a ferramenta de quem faz tudo (vende, produz, embala, posta no correio, divulga, controla o dinheiro).
- **Bonito o suficiente** — é ferramenta de pessoa criativa; visual limpo importa.

---

## 2. Quem é o cliente-alvo (persona)

**Artista / artesão / pequeno criador que vende sob encomenda e personalizado**, sozinho ou quase. Exemplos do mundo real do perfil: quadros e ilustrações personalizadas, peças de papelaria/convites, produtos artesanais (resina, cerâmica, costura, bordado, velas, etc.), brindes e lembranças personalizadas, presentes sob medida.

**Como essa pessoa trabalha hoje:**
- Recebe pedidos pelo **WhatsApp e Instagram/Direct**.
- Faz **orçamento na conversa**, combina prazo e valor no olho.
- Produz **à mão / sob encomenda** — cada pedido é meio único, tem etapas, tem prazo.
- **Embala e envia** pelos Correios / transportadora, às vezes entrega em mãos.
- Recebe **parcelado, no Pix, às vezes metade na entrada**.
- **Divulga** ela mesma nas redes (post, story, anúncio).
- Compra **insumos/material** e às vezes o material acaba na hora errada.
- Faz tudo isso **sem sócio, sem funcionário, sem departamento**.

**Dores centrais que o sistema ataca:**
- Não saber **qual pedido priorizar**, o que está atrasado, o que sai pro correio amanhã.
- **Perder histórico de clientes** — sem base para reativar quem comprou há meses.
- Não ter **controle de quem pagou, quem deve, quem está parcelado**.
- **Estoque/material acabando sem aviso**.
- Esquecer **posts/anúncios** que precisava publicar.
- Misturar **prazo normal, urgência e parceria** sem clareza.

---

## 3. Stack técnica (contexto de viabilidade)

Importante para você calibrar as sugestões ao que é **factível neste tipo de app**:

- **Desktop Windows nativo** em **.NET 10 + WPF** (interface), padrão MVVM.
- Regras de negócio numa **biblioteca Core** isolada; dados via **EF Core + PostgreSQL local** (o Postgres roda como serviço do Windows na própria máquina).
- **Tudo offline / local.** Não há servidor próprio, não há API na nuvem **hoje**. Integrações externas (WhatsApp, Instagram, gateways de pagamento, rastreio dos Correios) **não existem ainda** e são consideradas "longo prazo".
- Há **backup em 1 clique** (gera um arquivo de dump do banco).
- Modelado com `EmpresaId` em tudo (multi-tenant futuro), mas **hoje é 1 empresa por instalação** = a própria usuária.

**Implicação para suas sugestões:** prefira ideias que funcionem **100% offline e dentro do próprio app** (visualizações, organização, alertas internos, geração de documentos/arquivos locais, fluxos que reduzem cliques). Integrações online podem ser citadas, mas **marque-as como "depende de internet/integração"** — elas têm prioridade menor agora.

---

## 4. O que o sistema JÁ TEM hoje (módulos construídos e funcionando)

Cada módulo já tem CRUD, busca e filtros. O que falta (e é o foco da pesquisa) são **funcionalidades de uso e visualização que vão além do cadastro**.

### 4.1 Onboarding / Empresa
- No primeiro uso, cria a **Empresa** (nome, logo, documento CNPJ/CPF, contato, endereço).
- A empresa "é" a usuária.

### 4.2 Clientes (CRM)
- Cadastro, busca, edição, **inativar/reativar**.
- Campos: nome, WhatsApp/telefone, e-mail, Instagram, **endereço estruturado para envio**, observações, origem.

### 4.3 Pedidos (o coração do sistema)
- Cada pedido tem: **número sequencial** (#1, #2…), cliente vinculado, título/descrição.
- **Tipo:** Venda ou **Parceria**.
- **Status de produção:** Orçamento → Aguardando pagamento → Em produção → Pronto → Enviado → Concluído (+ Cancelado).
- **Prioridade:** Baixa / Normal / Alta.
- **Urgente** (flag separada, para pedidos fora do prazo mínimo).
- **Datas:** data do pedido, **prazo de entrega**.
- **Valor total** e **situação de pagamento:** Pendente / Parcial / Pago.
- **Envio:** transportadora, código de rastreio, data de postagem.
- Observações.
- **Lista** com busca e filtro por status; **pedidos urgentes e com prazo mais próximo aparecem no topo**.

### 4.4 Financeiro
- **Lançamentos** de entrada e saída, com categoria, valor, data, descrição (opcionalmente ligado a um pedido).
- Cards de **total de entradas, total de saídas e saldo**.

### 4.5 Estoque
- Itens com **quantidade, estoque mínimo, unidade, custo**.
- **Alerta visual de estoque baixo** (quando abaixo do mínimo).
- Busca e filtros.

### 4.6 Marketing
- **Agenda de lembretes** de post/anúncio: rede social (Instagram, TikTok…), tipo (Post/Story/Anúncio), data prevista, status (Pendente/Feito).
- Concluir / reabrir tarefa.

### 4.7 Dashboard
- Números agregados reais: atrasados, a entregar, pendências financeiras, alertas de estoque, marketing do dia.

### 4.8 Backup
- **Backup em 1 clique** para um arquivo local.

---

## 5. O que NÃO precisa sugerir (já está no nosso plano futuro)

Para você focar no que é **novo e fora do nosso radar**, aqui está o que JÁ está mapeado nas próximas versões. **Não preciso de ideias sobre estes itens** (a menos que você proponha um ângulo realmente diferente):

**Médio prazo (já planejado):**
- Relatórios de faturamento por período, clientes que mais compram, produtos mais vendidos.
- Geração de **orçamento/recibo em PDF** para o cliente.
- **Timeline do cliente** + lista de "inativos há X meses" para reativação.
- **Estoque avançado:** movimentações de entrada/saída e **lista de compras automática** do que está baixo.
- **Anexos:** imagens de referência no pedido, comprovantes de pagamento.
- **Contas a receber / parcelamento detalhado.**
- **Calendário visual** unificando pedidos + marketing + envios.
- **Alertas internos** de prazos vencendo.

**Longo prazo (já planejado):**
- Virar **SaaS multi-tenant** (API + web/mobile + nuvem).
- App **mobile**.
- **Integrações:** WhatsApp, Pix/pagamento, rastreio dos Correios, Instagram.
- Automação de marketing (agendamento real de posts).
- BI/métricas e metas avançadas.
- Multiusuário por empresa com permissões.
- Modelo de venda white-label / licenciamento.

---

## 6. O que eu estou buscando (foco da sua pesquisa)

Quero **funcionalidades e melhorias de usabilidade úteis no dia a dia**, que **ainda não temos nem planejamos**, inspiradas em sistemas que atendem público parecido (gestão para artesãos/ateliês/pequenos criadores sob encomenda; ex.: ferramentas tipo Craftybase, Etsy seller tools, sistemas de pequenas confeitarias/ateliês de festa, gestão de encomendas personalizadas, etc.).

Penso em coisas como (apenas para te dar o "tom" — quero que você vá além):
- **Formas melhores de visualizar o trabalho** (ex.: quadro Kanban por status de produção, visão de carga de trabalho da semana, "o que produzir hoje").
- **Fluxos que reduzem trabalho manual** (ex.: transformar orçamento em pedido, gerar mensagem pronta para o cliente, checklist de etapas de produção por pedido).
- **Visões que ajudam a decidir** (ex.: rentabilidade real por pedido considerando material + tempo, precificação sugerida).
- **Pequenos recursos de QA do dia** (ex.: o que embalar/postar amanhã, etiqueta de envio, lembrete de cobrança de quem está com pagamento parcial).
- **Usabilidade**: atalhos, ações rápidas, reduzir cliques, entrada de dados mais ágil.

Para cada ideia que você sugerir, me dê:
1. **Nome curto** da funcionalidade.
2. **Que dor do cliente-alvo resolve.**
3. **Como funcionaria** (resumido), e em qual módulo existente encaixa.
4. **Depende de internet/integração externa?** (sim/não) — lembrando que offline é prioridade.
5. **Por que é útil para ESTE perfil** (artista que faz tudo sozinho, offline).

Pode organizar as sugestões por prioridade/impacto e por esforço estimado, se conseguir.

---
*Fim do documento de contexto.*
