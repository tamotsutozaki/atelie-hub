using System.Diagnostics;
using AtelieHub.Core.Services;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AtelieHub.Infrastructure.Services;

/// <summary>Gera backup do banco chamando o pg_dump.exe instalado com o PostgreSQL.</summary>
public class BackupService : IBackupService
{
    private readonly string _connectionString;

    public BackupService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AtelieHub")
            ?? throw new InvalidOperationException("Connection string 'AtelieHub' não configurada.");
    }

    public string SugerirNomeArquivo(DateTime agora) => $"ateliehub-backup-{agora:yyyy-MM-dd-HHmm}.dump";

    public async Task CriarBackupAsync(string caminhoDestino, CancellationToken ct = default)
    {
        var csb = new NpgsqlConnectionStringBuilder(_connectionString);

        var pgDump = LocalizarPgDump()
            ?? throw new InvalidOperationException(
                "pg_dump não foi encontrado. Verifique se o PostgreSQL está instalado (pasta bin).");

        var psi = new ProcessStartInfo
        {
            FileName = pgDump,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
        };
        psi.ArgumentList.Add("-h");
        psi.ArgumentList.Add(string.IsNullOrWhiteSpace(csb.Host) ? "localhost" : csb.Host);
        psi.ArgumentList.Add("-p");
        psi.ArgumentList.Add(csb.Port.ToString());
        psi.ArgumentList.Add("-U");
        psi.ArgumentList.Add(csb.Username ?? "postgres");
        psi.ArgumentList.Add("-d");
        psi.ArgumentList.Add(csb.Database ?? "ateliehub");
        psi.ArgumentList.Add("-F");
        psi.ArgumentList.Add("c"); // formato custom (compactado/restaurável)
        psi.ArgumentList.Add("-f");
        psi.ArgumentList.Add(caminhoDestino);
        psi.Environment["PGPASSWORD"] = csb.Password ?? string.Empty;

        using var processo = Process.Start(psi)
            ?? throw new InvalidOperationException("Não foi possível iniciar o pg_dump.");

        var erro = await processo.StandardError.ReadToEndAsync(ct);
        await processo.WaitForExitAsync(ct);

        if (processo.ExitCode != 0)
        {
            throw new InvalidOperationException($"O backup falhou (pg_dump): {erro}");
        }
    }

    private static string? LocalizarPgDump()
    {
        const string exe = "pg_dump.exe";

        // 1) PATH
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in path.Split(Path.PathSeparator))
        {
            try
            {
                var candidato = Path.Combine(dir, exe);
                if (File.Exists(candidato))
                {
                    return candidato;
                }
            }
            catch
            {
                // ignora entradas inválidas do PATH
            }
        }

        // 2) Instalação padrão: C:\Program Files\PostgreSQL\<versao>\bin\pg_dump.exe (maior versão)
        foreach (var programFiles in new[]
                 {
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                 })
        {
            var raiz = Path.Combine(programFiles, "PostgreSQL");
            if (!Directory.Exists(raiz))
            {
                continue;
            }

            // Ordena pela versão NUMÉRICA da pasta (major, depois minor), não pela string do
            // caminho: lexicograficamente "9.6" > "16" (porque '9' > '1'), o que escolheria um
            // pg_dump mais antigo que o servidor e faria o dump falhar por "server version mismatch".
            var encontrado = Directory.GetDirectories(raiz)
                .Select(d => new { Bin = Path.Combine(d, "bin", exe), Versao = ExtrairVersao(Path.GetFileName(d)) })
                .Where(x => File.Exists(x.Bin))
                .OrderByDescending(x => x.Versao)
                .Select(x => x.Bin)
                .FirstOrDefault();

            if (encontrado is not null)
            {
                return encontrado;
            }
        }

        return null;
    }

    /// <summary>Converte o nome da pasta de instalação ("16", "15", "9.6") em versão comparável; inválido vira 0.0.</summary>
    private static Version ExtrairVersao(string nomePasta)
    {
        var partes = nomePasta.Split('.');
        var major = partes.Length > 0 && int.TryParse(partes[0], out var ma) ? ma : 0;
        var minor = partes.Length > 1 && int.TryParse(partes[1], out var mi) ? mi : 0;
        return new Version(major, minor);
    }
}
