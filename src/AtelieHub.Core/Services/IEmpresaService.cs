using AtelieHub.Core.Entities;

namespace AtelieHub.Core.Services;

/// <summary>
/// Contrato da camada de negócio para a Empresa. Definido na Core (sem EF),
/// implementado na Infrastructure. É o que mantém a UI desacoplada do banco.
/// </summary>
public interface IEmpresaService
{
    /// <summary>Indica se já existe alguma empresa cadastrada (decide se mostra o onboarding).</summary>
    Task<bool> ExisteEmpresaAsync(CancellationToken ct = default);

    /// <summary>Retorna a empresa ativa (a primeira cadastrada), ou null se ainda não houver.</summary>
    Task<Empresa?> ObterAtualAsync(CancellationToken ct = default);

    /// <summary>Cria ou atualiza a empresa e devolve a entidade persistida.</summary>
    Task<Empresa> SalvarAsync(Empresa empresa, CancellationToken ct = default);
}
