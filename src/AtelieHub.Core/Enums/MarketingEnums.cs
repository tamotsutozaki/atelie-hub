using System.ComponentModel;

namespace AtelieHub.Core.Enums;

public enum RedeSocial
{
    [Description("Instagram")] Instagram = 0,
    [Description("TikTok")] TikTok = 1,
    [Description("Facebook")] Facebook = 2,
    [Description("WhatsApp")] WhatsApp = 3,
    [Description("YouTube")] YouTube = 4,
    [Description("Outro")] Outro = 5,
}

public enum TipoPublicacao
{
    [Description("Post")] Post = 0,
    [Description("Story")] Story = 1,
    [Description("Reels")] Reels = 2,
    [Description("Anúncio")] Anuncio = 3,
    [Description("Outro")] Outro = 4,
}
