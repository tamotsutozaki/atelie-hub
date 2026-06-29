# Ateliê Hub — Documento do Projeto

> **Status:** em definição · **Versão do doc:** 1.0 · **Data:** 2026-06-29
> Sistema de gestão para artistas/ateliês que tocam o próprio negócio sozinhos.
> Produto **white-label** (genérico, revendável). **Eternize** é o cliente **beta tester** que valida.
> Documento vivo — atualizar conforme a prática mostrar o que funciona.

---

## 1. Visão do produto

Um sistema **desktop, 100% local**, que **centraliza toda a operação** de quem vende arte/serviços criativos e faz tudo sozinho: vendas, atendimento, produção, financeiro, estoque, logística e divulgação.

Hoje essa pessoa tem a informação espalhada (papel, cabeça, WhatsApp, caderno, planilha solta) e **nenhum lugar único** pra enxergar e tocar o negócio. O Ateliê Hub é esse lugar único.

### Dores que resolve
- Não saber **qual pedido priorizar**, o que está atrasado, o que sai pro correio amanhã.
- **Perder histórico de clientes** — sem base pra reativar quem comprou há meses/um ano.
- Não ter **controle de quem pagou, quem deve, quem está parcelado**.
- **Estoque acabando sem aviso** e sem saber o que recomprar.
- Esquecer **posts/anúncios** que precisava publicar.
- Misturar **prazo normal, urgência e parceria** sem clareza.

### Princípios de produto
- **Prático e rápido** — entregar uma v1 funcional que já entra em uso.
- **Leve** — roda local, num PC, sem nuvem, sem deploy.
- **Bonito o suficiente** — é ferramenta de uma pessoa criativa; visual limpo importa.
- **Genérico desde o início** — nada amarrado à Eternize; pronto pra virar produto.
- **Portável pro futuro** — modelado pra um dia virar SaaS na nuvem sem reescrever a lógica.

---

## 2. Stack e arquitetura (decidido)

| Camada | Escolha | Por quê |
|---|---|---|
| **Interface** | **WPF** (.NET 10) + kit visual (WPF UI / Material Design) | Desktop nativo Windows, sem navegador. Moderno, estilizável, maduro. |
| **Padrão de UI** | **MVVM** (CommunityToolkit.Mvvm) | Separação tela × lógica, testável, padrão do mundo WPF. |
| **Regras de negócio** | Biblioteca **Core** (domínio + serviços) | Isolada da UI — é o que destrava virar API/SaaS depois sem reescrever. |
| **Acesso a dados** | **EF Core** + Npgsql | ORM padrão .NET; troca de Postgres local → Supabase é trivial. |
| **Banco** | **PostgreSQL local** (serviço do Windows) | Escolha pensando na revenda (sobe pro Supabase de boa). |
| **Injeção de dependência** | Microsoft.Extensions.DependencyInjection + Hosting | Liga as camadas, configuração e ciclo de vida. |
| **Backup** | Script `pg_dump` (1 clique) | Cópia do banco num arquivo — segurança contra perda local. |

### Arquitetura: monolito em camadas
Um **único executável** (`.exe`). UI, regras e acesso a banco vivem no mesmo processo — a UI chama um método C# que chama o banco, **sem API HTTP no meio**. O único processo de fundo é o **Postgres** (o banco), set-and-forget.

```
AtelieHub.Desktop   (WPF · telas · ViewModels · MVVM)
        │  depende de
AtelieHub.Core       (entidades de domínio · enums · serviços · interfaces)
        │  implementado por
AtelieHub.Infrastructure  (EF Core · DbContext · repositórios · migrations)
        │  fala com
PostgreSQL local (serviço Windows)
```

**Caminho de revenda (sem reescrever a Core):**
> Hoje: `WPF → Core → EF Core → Postgres local` (valida rápido com a Eternize).
> SaaS: embrulha a **mesma Core** numa **ASP.NET Core Web API** + front web/mobile + Supabase. A lógica de negócio não muda — só o empacotamento.

---

## 3. Identidade visual

- **Paleta artística**: base **marrom** + tons complementares (terrosos/creme/areia, acento mais quente).
- **Estilo**: abstrato, limpo, profissional — **sem nenhum vínculo com a Eternize**.
- Nome provisório do produto: **Ateliê Hub** (revisar antes de eventual venda).

---

## 4. Modelo de dados inicial (rascunho)

> Tudo amarrado a um **`EmpresaId`** desde já — de graça hoje, e é o que destrava multi-cliente (multi-tenant) quando virar SaaS. A empresa **é** o usuário (1 empresa por instalação agora).

- **Empresa** — `Id, Nome, Logo, Documento (CNPJ/CPF), Contato, Endereço, CriadoEm` · criada no **onboarding** do primeiro uso.
- **Cliente** — `Id, EmpresaId, Nome, WhatsApp/Telefone, Email, Instagram, Endereço (p/ correio), Observações, Origem, DataCadastro`.
- **Pedido** — `Id, EmpresaId, ClienteId, Número, Título/Descrição, Tipo (Venda | Parceria), Status, Prioridade, DataPedido, PrazoEntrega, Urgente (bool), ValorTotal, Observações`.
- **ItemPedido** *(simples na v1)* — `Id, PedidoId, Descrição, Quantidade, ValorUnitário`.
- **Pagamento** — `Id, PedidoId, Valor, Data, Forma, Status (Pendente | Parcial | Pago)`.
- **Envio** *(campos no Pedido na v1)* — `EndereçoEntrega, Transportadora, CódigoRastreio, DataPostagem`.
- **ProdutoEstoque** — `Id, EmpresaId, Nome, Quantidade, EstoqueMínimo, Unidade, Custo`.
- **LançamentoFinanceiro** — `Id, EmpresaId, Tipo (Entrada | Saída), Categoria, Valor, Data, Descrição, PedidoId?` → alimenta o **caixa/saldo**.
- **TarefaMarketing** — `Id, EmpresaId, Título, Descrição, RedeSocial (Instagram | TikTok | …), Tipo (Post | Story | Anúncio), DataPrevista, Status (Pendente | Feito)`.

**Status do Pedido (enum proposto):** `Orçamento → AguardandoPagamento → EmProdução → Pronto → Enviado → Concluído` (+ `Cancelado`).

---

## 5. Plano de ação

### 🟢 Curto prazo — v1 (MVP que mantém o negócio de pé)
Tudo que é essencial pra ela operar e que dá pra fazer rápido.

1. **Fundação** — solução em camadas, EF Core + Postgres, MVVM, DI, tema marrom, navegação base.
2. **Onboarding / Cadastro de Empresa** — primeiro uso cria a empresa + perfil; tudo passa a usar `EmpresaId`.
3. **Clientes (CRM)** — cadastro, busca, edição, dados de envio, observações.
4. **Pedidos / fluxo de produção** *(coração)* — criar pedido, status, prioridade, prazo, **urgência**, **parceria**, dados de envio; lista/quadro com filtros e visão **"o que sai amanhã" / "atrasados"**.
5. **Financeiro básico** — pagamento por pedido (pago/pendente/parcial), lançamentos de entrada/saída, **saldo de caixa**.
6. **Estoque básico** — itens, quantidade, **alerta de estoque mínimo**.
7. **Agenda de marketing (leve)** — lembretes de post/anúncio com **prazo + rede social** (sem integração).
8. **Dashboard inicial** — atrasados, a entregar, pendências financeiras, alertas de estoque, marketing do dia.
9. **Backup em 1 clique** — `pg_dump` pra um arquivo.

### 🟡 Médio prazo — v2 (importante, próxima versão)
- **Relatórios** — faturamento por período, clientes que mais compram, produtos mais vendidos.
- **Orçamento/recibo em PDF** — gerar documento pro cliente.
- **Timeline do cliente + reativação** — histórico e lista de "inativos há X meses".
- **Estoque avançado** — movimentações (entrada/saída) e **lista de compras automática** do que está baixo.
- **Anexos** — imagens de referência no pedido, comprovantes de pagamento.
- **Contas a receber / parcelamento detalhado**.
- **Calendário visual** unificando pedidos + marketing + envios.
- **Alertas internos** de prazos vencendo.

### 🔴 Longo prazo — v3+ (as fodas / futuro)
- **Virar SaaS multi-tenant** — extrair a Core pra **ASP.NET Core API**, front web/mobile, banco no **Supabase**.
- **App mobile (MAUI)** consumindo a mesma API.
- **Integrações** — WhatsApp, Pix/pagamento, rastreio dos Correios, Instagram.
- **Automação de marketing** — agendamento real de posts.
- **BI / métricas e metas** avançadas.
- **Multiusuário por empresa** com permissões.
- **Modelo de venda white-label** — licenciamento e onboarding self-service.

---

## 6. Decisões registradas

- **Stack:** .NET 10 · WPF + kit visual · MVVM · Core isolada · EF Core/Npgsql · **PostgreSQL local**.
- **Arquitetura:** **monolito em camadas** (sem API separada agora) — Core isolada pra portar pra SaaS depois.
- **Execução:** **100% local, 1 PC, sem nuvem/deploy**. Acesso só no PC dela.
- **Pagamentos:** **registro manual** (pago/pendente/parcial).
- **Produto:** **white-label**, desvinculado da Eternize; Eternize = **beta tester**.
- **Tenant:** **empresa = usuário**; 1 empresa por instalação, mas modelado com `EmpresaId` pra multi-tenant futuro.
- **Paleta:** artística, base **marrom** + tons complementares.

---

## 7. Próximos passos sugeridos
1. Validar este documento (ajustar prioridades da v1 se necessário).
2. Definir nome/paleta final do produto (provisório: Ateliê Hub).
3. Montar o esqueleto da solução (.NET 10, 3 projetos, EF Core, Postgres, tela de onboarding + dashboard vazio).
4. Implementar os módulos da v1 na ordem do plano (Fundação → Clientes → Pedidos → Financeiro → Estoque → Marketing → Dashboard → Backup).

---
*Fim do documento.*
