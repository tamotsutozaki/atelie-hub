using AtelieHub.Core.Common;

namespace AtelieHub.Core.Entities;

/// <summary>
/// Empresa = o próprio usuário do sistema (o ateliê). Hoje há uma por instalação,
/// mas tudo no domínio é amarrado ao Id dela para destravar multi-tenant quando virar SaaS.
/// Criada no onboarding do primeiro uso.
/// </summary>
public class Empresa : BaseEntity
{
    /// <summary>Nome do ateliê / negócio. Único campo obrigatório.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Nome da pessoa que toca o negócio (aparece nas saudações).</summary>
    public string? NomeResponsavel { get; set; }

    /// <summary>CNPJ ou CPF.</summary>
    public string? Documento { get; set; }

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string? Instagram { get; set; }

    /// <summary>Caminho local do arquivo de logo (imagem no disco).</summary>
    public string? LogoPath { get; set; }

    public string? Cep { get; set; }

    public string? Endereco { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }
}
