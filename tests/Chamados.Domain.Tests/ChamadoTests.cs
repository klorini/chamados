using Xunit;

namespace Chamados.Domain.Tests;

public class ChamadoTests
{
    private static readonly DateTimeOffset Abertura = new(2026, 11, 1, 8, 0, 0, TimeSpan.Zero);

    private static Chamado NovoChamadoAberto()
        => new("Trocar lâmpada do corredor", Prioridade.Media, "Luz queimada", Abertura);

    [Fact]
    public void ChamadoNovo_NasceAbertoComPadroes()
    {
        var chamado = new Chamado("Trocar lâmpada do corredor");

        Assert.Equal(StatusChamado.Aberto, chamado.Status);
        Assert.Equal(Prioridade.Media, chamado.Prioridade);
        Assert.Null(chamado.Descricao);
        Assert.Null(chamado.Responsavel);
        Assert.Empty(chamado.Historico);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TituloEmBranco_LancaArgumentException(string titulo)
    {
        Assert.Throws<ArgumentException>(() => new Chamado(titulo));
    }

    [Fact]
    public void IniciarAtendimento_TransicionaEDefineResponsavel()
    {
        var chamado = NovoChamadoAberto();

        chamado.IniciarAtendimento("  Equipe elétrica  ", Abertura.AddMinutes(10));

        Assert.Equal(StatusChamado.EmAtendimento, chamado.Status);
        Assert.Equal("Equipe elétrica", chamado.Responsavel);
        Assert.False(chamado.EstaEncerrado);
    }

    [Fact]
    public void Resolver_AposAtendimento_DefineSolucaoEEncerradoEm()
    {
        var chamado = NovoChamadoAberto();
        chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(10));

        var fim = Abertura.AddHours(2);
        chamado.Resolver("Lâmpada trocada", fim);

        Assert.Equal(StatusChamado.Resolvido, chamado.Status);
        Assert.Equal("Lâmpada trocada", chamado.Solucao);
        Assert.Equal(fim, chamado.EncerradoEm);
        Assert.True(chamado.EstaEncerrado);
    }

    [Fact]
    public void Cancelar_DeAberto_Encerra()
    {
        var chamado = NovoChamadoAberto();

        chamado.Cancelar("Aberto em duplicidade", Abertura.AddMinutes(5));

        Assert.Equal(StatusChamado.Cancelado, chamado.Status);
        Assert.True(chamado.EstaEncerrado);
    }

    [Fact]
    public void Cancelar_DeEmAtendimento_Encerra()
    {
        var chamado = NovoChamadoAberto();
        chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(10));

        chamado.Cancelar("Equipamento foi descartado", Abertura.AddMinutes(30));

        Assert.Equal(StatusChamado.Cancelado, chamado.Status);
    }

    [Fact]
    public void Resolver_SemAtendimento_LancaTransicaoInvalida()
    {
        var chamado = NovoChamadoAberto();

        var excecao = Assert.Throws<TransicaoInvalidaException>(
            () => chamado.Resolver("Resolvido na pressa", Abertura.AddMinutes(1)));

        Assert.Equal(chamado.Id, excecao.ChamadoId);
        Assert.Equal(StatusChamado.Aberto, excecao.Atual);
        Assert.Equal(StatusChamado.Resolvido, excecao.Pretendido);
        Assert.Equal(StatusChamado.Aberto, chamado.Status);
    }

    [Fact]
    public void TransicaoAposEncerramento_LancaTransicaoInvalida()
    {
        var chamado = NovoChamadoAberto();
        chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(10));
        chamado.Resolver("Lâmpada trocada", Abertura.AddHours(1));

        Assert.Throws<TransicaoInvalidaException>(
            () => chamado.Cancelar("Tarde demais", Abertura.AddHours(2)));
    }

    [Fact]
    public void Historico_RegistraCadaTransicao()
    {
        var chamado = NovoChamadoAberto();
        chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(10));
        chamado.Resolver("Lâmpada trocada", Abertura.AddHours(1));

        Assert.Equal(2, chamado.Historico.Count);

        var primeiro = chamado.Historico[0];
        Assert.Equal(StatusChamado.Aberto, primeiro.De);
        Assert.Equal(StatusChamado.EmAtendimento, primeiro.Para);
        Assert.Equal(Abertura.AddMinutes(10), primeiro.Em);
        Assert.Equal("Atribuído a Equipe elétrica", primeiro.Observacao);

        var segundo = chamado.Historico[1];
        Assert.Equal(StatusChamado.EmAtendimento, segundo.De);
        Assert.Equal(StatusChamado.Resolvido, segundo.Para);
    }

    [Fact]
    public void TempoAteEncerramento_NuloEnquantoAberto_CalculadoAoResolver()
    {
        var chamado = NovoChamadoAberto();
        Assert.Null(chamado.TempoAteEncerramento);

        chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(10));
        chamado.Resolver("Lâmpada trocada", Abertura.AddHours(3));

        Assert.Equal(TimeSpan.FromHours(3), chamado.TempoAteEncerramento);
    }

    [Fact]
    public void TransicaoComDataAnteriorAbertura_LancaArgumentOutOfRange()
    {
        var chamado = NovoChamadoAberto();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(-5)));
        Assert.Equal(StatusChamado.Aberto, chamado.Status);
        Assert.Empty(chamado.Historico);
    }

    [Fact]
    public void Historico_NaoPodeSerAlteradoPorCast()
    {
        var chamado = NovoChamadoAberto();
        chamado.IniciarAtendimento("Equipe elétrica", Abertura.AddMinutes(10));

        Assert.Null(chamado.Historico as List<RegistroHistorico>);
    }
}
