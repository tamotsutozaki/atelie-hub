using System.ComponentModel;

namespace AtelieHub.Core.Enums;

public enum TipoPedido
{
    [Description("Venda")] Venda = 0,
    [Description("Parceria")] Parceria = 1,
}

public enum StatusPedido
{
    [Description("Orçamento")] Orcamento = 0,
    [Description("Aguardando pagamento")] AguardandoPagamento = 1,
    [Description("Em produção")] EmProducao = 2,
    [Description("Pronto")] Pronto = 3,
    [Description("Enviado")] Enviado = 4,
    [Description("Concluído")] Concluido = 5,
    [Description("Cancelado")] Cancelado = 6,
}

public enum PrioridadePedido
{
    [Description("Baixa")] Baixa = 0,
    [Description("Normal")] Normal = 1,
    [Description("Alta")] Alta = 2,
}

public enum StatusPagamento
{
    [Description("Pendente")] Pendente = 0,
    [Description("Parcial")] Parcial = 1,
    [Description("Pago")] Pago = 2,
}
