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

    /// <summary>Indica reposição necessária (não persistido).</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EstaBaixo => Ativo && Quantidade <= EstoqueMinimo;
}
