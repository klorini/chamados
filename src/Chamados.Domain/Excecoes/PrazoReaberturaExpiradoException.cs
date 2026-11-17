namespace Chamados.Domain;

/// <summary>
/// Tentativa de reabrir um chamado resolvido depois do prazo de reabertura.
/// Limite informa até quando a reabertura era permitida.
/// </summary>
public sealed class PrazoReaberturaExpiradoException(Guid chamadoId, DateTimeOffset limite)
    : InvalidOperationException($"O prazo para reabrir o chamado {chamadoId} terminou em {limite:dd/MM/yyyy HH:mm} (UTC).")
{
    public Guid ChamadoId { get; } = chamadoId;

    public DateTimeOffset Limite { get; } = limite;
}
