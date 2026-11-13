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

Rotas disponíveis: `/`, `/status`, `/chamados/exemplo` e `/ambiente` (somente em Development).

## Decisões

1. **.NET 10 LTS como alvo.** Suporte até **14/11/2028**, cobrindo todo o curso. O .NET 8 (LTS) e o .NET 9 (STS) perdem suporte em 10/11/2026, e o .NET 11 ainda está em preview.
2. **Formato `.slnx` para a solução.** É o padrão do `dotnet new sln` no SDK 10: XML curto, sem GUIDs, mais fácil de ler e revisar em merge. Os comandos `dotnet sln` continuam os mesmos.

## Como testar

```bash
dotnet test
```

Os testes (xUnit) cobrem o domínio: um chamado novo nasce com status `Aberto` e título em branco lança `ArgumentException`.
