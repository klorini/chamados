# Chamados

Sistema de chamados (ordens de serviço) para uma empresa de manutenção predial — projeto integrador da formação **Desenvolvimento Web e Multiplataforma com .NET** (Módulo 1).

## Requisitos

- **.NET SDK 10.0.402** (fixado em `global.json` com `rollForward: latestFeature`)
- Git 2.28 ou superior

## Como executar

```bash
dotnet build
dotnet run --project src/Chamados.Web
```

Rotas: `/`, `/status`, `/chamados` (filtros `status`, `prioridadeMinima`, `texto`), `/chamados/estatisticas`, `/chamados/{id}`, `POST /chamados/{id}/atendimento` e `/ambiente` (somente em Development). O arquivo `src/Chamados.Web/Chamados.Web.http` tem uma requisição pronta para cada rota.

## Decisões

1. **.NET 10 LTS como alvo.** Suporte até **14/11/2028**, cobrindo todo o curso. O .NET 8 (LTS) e o .NET 9 (STS) perdem suporte em 10/11/2026, e o .NET 11 ainda está em preview.
2. **Formato `.slnx` para a solução.** É o padrão do `dotnet new sln` no SDK 10: XML curto, sem GUIDs, mais fácil de ler e revisar em merge. Os comandos `dotnet sln` continuam os mesmos.

## Como testar

```bash
dotnet test
```

Os testes (xUnit) cobrem o ciclo de vida do domínio: transições válidas e inválidas, histórico, tempo até o encerramento e proteção da lista de histórico.
3. **Interface de repositório (`IChamadoRepositorio`).** Existe porque a implementação vai trocar: hoje `RepositorioChamadosEmMemoria`, no Módulo 7 Entity Framework Core. O serviço não muda quando a persistência mudar.
4. **`TimeProvider` injetado.** Nenhum código de produção chama `DateTimeOffset.UtcNow` diretamente; nos testes, o `RelogioFixo` torna datas e durações previsíveis.
5. **Casos de uso no próprio `Chamados.Domain`, sem camada Application por enquanto.** Com cinco casos de uso e um modelo, um projeto a mais só adicionaria referências; a separação vem quando a infraestrutura (EF Core) precisar ficar fora do domínio.
