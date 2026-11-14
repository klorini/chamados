using Xunit;

namespace Chamados.Domain.Tests;

public class ChamadoServicoTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 11, 1, 8, 0, 0, TimeSpan.Zero);

    private static (ChamadoServico Servico, RelogioFixo Relogio) CriarServico()
    {
        var relogio = new RelogioFixo(Inicio);
        var repositorio = new RepositorioChamadosEmMemoria(relogio);
        return (new ChamadoServico(repositorio, relogio), relogio);
    }

    [Fact]
    public async Task AbrirAsync_UsaRelogioInjetadoEGuardaNoRepositorio()
    {
        var (servico, _) = CriarServico();

        var chamado = await servico.AbrirAsync(new NovoChamado("Tomada solta", Prioridade.Alta));

        Assert.Equal(Inicio, chamado.CriadoEm);
        var listados = await servico.ListarAsync(FiltroChamados.Todos);
        Assert.Single(listados);
        Assert.Equal(chamado.Id, listados[0].Id);
    }

    [Fact]
    public async Task ListarAsync_OrdenaPorPrioridadeDepoisPorAbertura()
    {
        var (servico, relogio) = CriarServico();
        await servico.AbrirAsync(new NovoChamado("Lâmpada", Prioridade.Baixa));
        relogio.Avancar(TimeSpan.FromMinutes(1));
        var critico1 = await servico.AbrirAsync(new NovoChamado("Vazamento", Prioridade.Critica));
        relogio.Avancar(TimeSpan.FromMinutes(1));
        var critico2 = await servico.AbrirAsync(new NovoChamado("Curto-circuito", Prioridade.Critica));

        var resultado = await servico.ListarAsync(FiltroChamados.Todos);

        Assert.Equal(
            new[] { critico1.Id, critico2.Id },
            resultado.Take(2).Select(r => r.Id));
        Assert.Equal("Lâmpada", resultado[^1].Titulo);
    }

    [Fact]
    public async Task ListarAsync_FiltraPorTextoNaDescricao_IgnorandoMaiusculas()
    {
        var (servico, _) = CriarServico();
        await servico.AbrirAsync(new NovoChamado("Ar-condicionado", Prioridade.Alta, "Desliga na sala 204"));
        await servico.AbrirAsync(new NovoChamado("Projetor", Prioridade.Media));

        var resultado = await servico.ListarAsync(new FiltroChamados(Texto: "SALA"));

        Assert.Single(resultado);
        Assert.Equal("Ar-condicionado", resultado[0].Titulo);
    }

    [Fact]
    public async Task ListarAsync_CombinaStatusEPrioridadeMinima()
    {
        var (servico, _) = CriarServico();
        var alta = await servico.AbrirAsync(new NovoChamado("Ar-condicionado", Prioridade.Alta));
        await servico.IniciarAtendimentoAsync(alta.Id, "Climatização");
        await servico.AbrirAsync(new NovoChamado("Tomada", Prioridade.Alta));
        var baixa = await servico.AbrirAsync(new NovoChamado("Lâmpada", Prioridade.Baixa));
        await servico.IniciarAtendimentoAsync(baixa.Id, "Elétrica");

        var resultado = await servico.ListarAsync(
            new FiltroChamados(StatusChamado.EmAtendimento, Prioridade.Alta));

        Assert.Single(resultado);
        Assert.Equal(alta.Id, resultado[0].Id);
    }

    [Fact]
    public async Task ObterEstatisticasAsync_TempoMedioComRelogioAvancado()
    {
        var (servico, relogio) = CriarServico();
        var chamado = await servico.AbrirAsync(new NovoChamado("Vazamento", Prioridade.Critica));
        await servico.IniciarAtendimentoAsync(chamado.Id, "Hidráulica");
        relogio.Avancar(TimeSpan.FromHours(2));
        await servico.ResolverAsync(chamado.Id, "Sifão substituído");

        var estatisticas = await servico.ObterEstatisticasAsync();

        Assert.Equal(TimeSpan.FromHours(2), estatisticas.TempoMedioResolucao);
    }

    [Fact]
    public async Task ObterEstatisticasAsync_PorStatusIncluiZerados_PorPrioridadeSoPresentes()
    {
        var (servico, _) = CriarServico();
        await servico.AbrirAsync(new NovoChamado("Tomada", Prioridade.Alta));

        var estatisticas = await servico.ObterEstatisticasAsync();

        Assert.Equal(1, estatisticas.Total);
        Assert.Equal(4, estatisticas.PorStatus.Count);
        Assert.Equal(0, estatisticas.PorStatus[StatusChamado.Resolvido]);
        Assert.Single(estatisticas.PorPrioridade);
        Assert.Null(estatisticas.TempoMedioResolucao);
    }

    [Fact]
    public async Task IniciarAtendimentoAsync_IdInexistente_LancaChamadoNaoEncontrado()
    {
        var (servico, _) = CriarServico();

        await Assert.ThrowsAsync<ChamadoNaoEncontradoException>(
            () => servico.IniciarAtendimentoAsync(Guid.NewGuid(), "Alguém"));
    }

    [Fact]
    public async Task ResolverAsync_SemAtendimento_PropagaTransicaoInvalida()
    {
        var (servico, _) = CriarServico();
        var chamado = await servico.AbrirAsync(new NovoChamado("Tomada", Prioridade.Alta));

        await Assert.ThrowsAsync<TransicaoInvalidaException>(
            () => servico.ResolverAsync(chamado.Id, "Sem passar pelo atendimento"));
    }

    [Fact]
    public async Task ListarAsync_TokenJaCancelado_LancaOperationCanceled()
    {
        var (servico, _) = CriarServico();
        var token = new CancellationToken(canceled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => servico.ListarAsync(FiltroChamados.Todos, token));
    }
}
