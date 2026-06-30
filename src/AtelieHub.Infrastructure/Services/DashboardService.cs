using System.ComponentModel;
using System.Reflection;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace AtelieHub.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DashboardService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<DashboardResumo> ObterResumoAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // ===== Pedidos em aberto (urgentes primeiro, depois por prazo mais próximo) =====
        var abertos = await db.Pedidos
            .Where(p => p.Status != StatusPedido.Concluido && p.Status != StatusPedido.Cancelado)
            .OrderByDescending(p => p.Urgente)
            .ThenBy(p => p.PrazoEntrega == null)
            .ThenBy(p => p.PrazoEntrega)
            .Select(p => new { p.Titulo, Cliente = p.Cliente!.Nome, p.PrazoEntrega, p.Status, p.Urgente })
            .ToListAsync(ct);

        var pedidosEmAbertoItens = abertos
            .Select(p => new DashboardItem(
                $"{p.Cliente} — {p.Titulo}",
                (p.Urgente ? "Urgente · " : string.Empty) + Descricao(p.Status) + Prazo(p.PrazoEntrega)))
            .ToList();

        // ===== A entregar (status Pronto) =====
        var prontos = await db.Pedidos
            .Where(p => p.Status == StatusPedido.Pronto)
            .OrderBy(p => p.PrazoEntrega == null)
            .ThenBy(p => p.PrazoEntrega)
            .Select(p => new { p.Titulo, Cliente = p.Cliente!.Nome, p.PrazoEntrega })
            .ToListAsync(ct);

        var aEntregarItens = prontos
            .Select(p => new DashboardItem($"{p.Cliente} — {p.Titulo}", "Pronto" + Prazo(p.PrazoEntrega)))
            .ToList();

        // ===== Pendências de pagamento (maior valor primeiro) =====
        var pendencias = await db.Pedidos
            .Where(p => p.StatusPagamento != StatusPagamento.Pago && p.Status != StatusPedido.Cancelado)
            .OrderByDescending(p => p.ValorTotal)
            .Select(p => new { p.Titulo, Cliente = p.Cliente!.Nome, p.ValorTotal, p.StatusPagamento })
            .ToListAsync(ct);

        var pendenciasFinanceirasItens = pendencias
            .Select(p => new DashboardItem(
                $"{p.Cliente} — {p.Titulo}",
                $"{Descricao(p.StatusPagamento)} · {p.ValorTotal:C}"))
            .ToList();

        // ===== Estoque baixo (mais crítico primeiro) =====
        var estoque = await db.ProdutosEstoque
            .Where(ProdutoEstoque.EstoqueBaixoPredicado)
            .OrderBy(p => p.Quantidade)
            .Select(p => new { p.Nome, p.Quantidade, p.EstoqueMinimo, p.Unidade })
            .ToListAsync(ct);

        var estoqueBaixoItens = estoque
            .Select(p => new DashboardItem(
                p.Nome,
                $"{Numero(p.Quantidade)}{Unidade(p.Unidade)} em estoque · mín {Numero(p.EstoqueMinimo)}"))
            .ToList();

        // ===== Marketing pendente (com data prevista primeiro, mais próxima) =====
        var marketing = await db.TarefasMarketing
            .Where(t => !t.Concluido)
            .OrderBy(t => t.DataPrevista == null)
            .ThenBy(t => t.DataPrevista)
            .Select(t => new { t.Titulo, t.Rede, t.DataPrevista })
            .ToListAsync(ct);

        var marketingPendenteItens = marketing
            .Select(t => new DashboardItem(t.Titulo, Descricao(t.Rede) + Prazo(t.DataPrevista)))
            .ToList();

        // ===== Saldo em caixa =====
        var entradas = await db.LancamentosFinanceiros
            .Where(l => l.Tipo == TipoLancamento.Entrada)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0m;

        var saidas = await db.LancamentosFinanceiros
            .Where(l => l.Tipo == TipoLancamento.Saida)
            .SumAsync(l => (decimal?)l.Valor, ct) ?? 0m;

        return new DashboardResumo(
            pedidosEmAbertoItens.Count,
            aEntregarItens.Count,
            pendenciasFinanceirasItens.Count,
            estoqueBaixoItens.Count,
            marketingPendenteItens.Count,
            entradas - saidas,
            pedidosEmAbertoItens,
            aEntregarItens,
            pendenciasFinanceirasItens,
            estoqueBaixoItens,
            marketingPendenteItens);
    }

    private static string Prazo(DateTime? data) => data.HasValue ? $" · {data.Value:dd/MM/yyyy}" : string.Empty;

    private static string Numero(decimal valor) => valor.ToString("0.##");

    private static string Unidade(string? unidade) => string.IsNullOrWhiteSpace(unidade) ? string.Empty : $" {unidade}";

    /// <summary>Texto amigável do enum (atributo [Description]), igual ao usado nas telas.</summary>
    private static string Descricao(Enum valor)
    {
        var nome = valor.ToString();
        var membro = valor.GetType().GetMember(nome).FirstOrDefault();
        return membro?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? nome;
    }
}
