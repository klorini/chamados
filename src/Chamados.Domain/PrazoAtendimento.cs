namespace Chamados.Domain;

/// <summary>
/// Prazo de atendimento de cada prioridade, contado a partir da abertura do chamado.
/// É regra de negócio, por isso fica no domínio; a tela só decide como mostrar.
/// </summary>
public static class PrazoAtendimento
{
    public static TimeSpan Para(Prioridade prioridade) => prioridade switch
    {
        Prioridade.Critica => TimeSpan.FromHours(4),
        Prioridade.Alta => TimeSpan.FromHours(8),
        Prioridade.Media => TimeSpan.FromHours(24),
        Prioridade.Baixa => TimeSpan.FromHours(72),
        _ => throw new ArgumentOutOfRangeException(nameof(prioridade), prioridade, "Prioridade inexistente.")
    };

    /// <summary>Momento em que o prazo de atendimento termina.</summary>
    public static DateTimeOffset VenceEm(DateTimeOffset criadoEm, Prioridade prioridade)
        => criadoEm + Para(prioridade);
}
