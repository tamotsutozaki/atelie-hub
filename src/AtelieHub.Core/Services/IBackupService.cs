namespace AtelieHub.Core.Services;

public interface IBackupService
{
    /// <summary>Gera um backup do banco no caminho informado (formato custom do pg_dump).</summary>
    Task CriarBackupAsync(string caminhoDestino, CancellationToken ct = default);

    /// <summary>Nome de arquivo sugerido para o backup (inclui data/hora informada).</summary>
    string SugerirNomeArquivo(DateTime agora);
}
