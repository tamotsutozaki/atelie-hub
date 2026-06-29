using AtelieHub.Core.Common;

namespace AtelieHub.Core.Entities;

/// <summary>
/// Cliente do ateliê. Amarrado a uma Empresa (EmpresaId) para destravar multi-tenant futuro.
/// Endereço estruturado para facilitar etiqueta/envio pelos Correios.
/// Em vez de excluir, o cliente é inativado (Ativo=false) — preserva o histórico para reativação.
/// </summary>
public class Cliente : BaseEntity
{
    public Guid EmpresaId { get; set; }

    /// <summary>Único campo obrigatório.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Telefone / WhatsApp.</summary>
    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public string? Instagram { get; set; }

    /// <summary>Como o cliente conheceu o ateliê (indicação, Instagram, etc.).</summary>
    public string? Origem { get; set; }

    public string? Observacoes { get; set; }

    // ===== Endereço (para envio) =====
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    /// <summary>Cliente ativo aparece nas listas por padrão. Inativo fica oculto, mas é preservado.</summary>
    public bool Ativo { get; set; } = true;
}
