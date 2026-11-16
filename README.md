# Chamados

Sistema de chamados (ordens de serviço) — projeto integrador da formação
*Desenvolvimento Web e Multiplataforma com .NET*. Estado: Módulo 3.

## Requisitos

- .NET SDK 10.0.1xx ou superior na linha 10.0 (fixado em `global.json`)
- Git 2.28+
- Visual Studio 2026 (Community serve) com a carga de trabalho **ASP.NET e desenvolvimento Web**

## Executar

No Visual Studio: abra `Chamados.slnx`, escolha o perfil **https** na barra de ferramentas e pressione `F5`.
O navegador abre no painel (`https://localhost:7080/`). Na primeira execução, aceite a confiança no certificado de desenvolvimento.

Pela CLI:

```bash
dotnet build
dotnet run --project src/Chamados.Web --launch-profile https
```

Portas fixas: http 5080, https 7080. Em Development a base em memória recebe 5 chamados de exemplo.

### Páginas (Razor Pages)

| Rota | Página | O que faz |
| --- | --- | --- |
| `/` | `Pages/Index` | Painel com totais por status (`ObterEstatisticasAsync`) |
| `/Chamados?status=&prioridadeMinima=&texto=` | `Pages/Chamados/Index` | Lista com filtros (`ListarAsync`) |
| `/Chamados/Novo` | `Pages/Chamados/Novo` | Formulário de abertura (`AbrirAsync`), validação e Post/Redirect/Get |
| `/Chamados/Detalhes/{id}` | `Pages/Chamados/Detalhes` | Detalhe, histórico e handlers Iniciar, Resolver, Cancelar |
| `/Erro` | `Pages/Erro` | Página de erro genérica, usada fora de Development |

### API JSON (movida para `/api` no Módulo 3)

| Rota | Função |
| --- | --- |
| `GET /api/chamados?status=&prioridadeMinima=&texto=` | Lista com filtros |
| `GET /api/chamados/estatisticas` | Totais e tempo médio de resolução |
| `GET /api/chamados/{id}` | Um chamado |
| `POST /api/chamados` | Abre chamado (corpo: `{"titulo": "...", "prioridade": "Alta"}`) |
| `POST /api/chamados/{id}/atendimento` | Inicia atendimento (corpo: `{"responsavel": "..."}`) |
| `GET /api/status`, `GET /api/ambiente` | Herdadas do Módulo 1 (`/api/ambiente` só em Development) |

Erros de domínio na API voltam como Problem Details (RFC 9457): 400, 404 ou 409.

### Configuração

`Simulacao:LatenciaMs` (0 a 10000) atrasa a listagem para observar cancelamento.
Valor fora da faixa impede a aplicação de iniciar (`ValidateOnStart`). Para testar sem editar arquivo:

```powershell
$env:Simulacao__LatenciaMs = "20000"; dotnet run --project src/Chamados.Web
```

## Testar

No Visual Studio: **Teste > Gerenciador de Testes > Executar Todos** (`Ctrl+R, A`). Pela CLI: `dotnet test`.

## Estrutura

| Projeto | Papel | Depende de |
| --- | --- | --- |
| `src/Chamados.Domain` | Entidades, regras e casos de uso | — |
| `src/Chamados.Web` | Páginas Razor, API JSON, composição e pipeline | Domain |
| `tests/Chamados.Domain.Tests` | Testes xUnit do domínio | Domain |

## Decisões

1. **.NET 10 LTS** como alvo: suporte até 14/11/2028; .NET 8 e 9 encerram em 10/11/2026.
2. **Formato `.slnx`** para a solução, padrão do SDK 10.
3. **Chamados.Web como projeto de inicialização**, declarado no `.slnx` com `DefaultStartup="true"`.
4. **Domain não referencia nenhum outro projeto**: as camadas externas dependem dele, nunca o contrário.
5. **Repositório como interface** (`IChamadoRepositorio`): a implementação em memória será trocada pelo EF Core no Módulo 7.
6. **`TimeProvider` injetado** em vez de `DateTimeOffset.UtcNow` direto: permite testar datas com um relógio controlado.
7. **Sem camada Application separada** por enquanto: os casos de uso (`ChamadoServico`) ficam no Domain.
8. **Razor Pages para os formulários** (Módulo 3): uma página por tela, com o `PageModel` chamando só o `ChamadoServico`, nunca o repositório.
9. **Validação em duas camadas**: Data Annotations no modelo do formulário (mensagem amigável, no servidor) e invariantes no domínio (palavra final). Os limites de tamanho são constantes do `Chamado`, reusadas nos atributos.
10. **Sem validação no cliente por enquanto**: nenhuma biblioteca JavaScript; o servidor valida sempre. Validação no cliente é conforto, não segurança (Módulo 4).
11. **API sob `/api`** com `MapGroup` e um filtro único de exceções de domínio, para não disputar a rota `/Chamados` com as páginas.
