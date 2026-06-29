using AtelieHub.Core.Common;
using AtelieHub.Core.Enums;

namespace AtelieHub.Core.Entities;

/// <summary>
/// Lembrete de divulgação (post/story/anúncio) com prazo e rede social.
/// Sem integração — serve como agenda do que precisa ser publicado.
/// </summary>
public class TarefaMarketing : BaseEntity
{
    public Guid EmpresaId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public RedeSocial Rede { get; set; } = RedeSocial.Instagram;

    public TipoPublicacao Tipo { get; set; } = TipoPublicacao.Post;

    public DateTime? DataPrevista { get; set; }

    public bool Concluido { get; set; }
}
