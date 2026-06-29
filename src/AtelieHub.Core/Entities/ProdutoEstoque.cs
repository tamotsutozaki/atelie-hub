using System.Linq.Expressions;
using AtelieHub.Core.Common;

namespace AtelieHub.Core.Entities;

/// <summary>Item de estoque (insumo/material). Alerta de reposição quando Quantidade &lt;= EstoqueMinimo.</summary>
public class ProdutoEstoque : BaseEntity
{
    public Guid EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Unidade de medida (un, m, kg, folha...).</summary>
    public string? Unidade { get; set; }

    public decimal Quantidade { get; set; }

    public decimal EstoqueMinimo { get; set; }

    public decimal? Custo { get; set; }

    public string? Observacoes { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Indica reposição necessária (não persistido). Regra: item ativo, com mínimo definido
    /// (&gt; 0) e quantidade no/abaixo do mínimo. Exigir mínimo &gt; 0 evita falso alerta em itens
    /// novos (quantidade e mínimo no default 0) ou cujo mínimo a usuária nunca configurou.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EstaBaixo => Ativo && EstoqueMinimo > 0 && Quantidade <= EstoqueMinimo;

    /// <summary>Mesma regra de <see cref="EstaBaixo"/> como predicado traduzível pelo EF — fonte única para as queries de "estoque baixo" (lista, contador e dashboard).</summary>
    public static readonly Expression<Func<ProdutoEstoque, bool>> EstoqueBaixoPredicado =
        p => p.Ativo && p.EstoqueMinimo > 0 && p.Quantidade <= p.EstoqueMinimo;
}
