using Chamados.Domain;
using Chamados.Web.Apresentacao;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Chamados.Web.TagHelpers;

/// <summary>
/// &lt;prazo criado-em="@c.CriadoEm" prioridade="@c.Prioridade" status="@c.Status" /&gt; vira
/// &lt;span class="selo selo-no-prazo"&gt;Vence em 3 h 10 min&lt;/span&gt; ou
/// &lt;span class="selo selo-atrasado"&gt;Atrasado há 45 min&lt;/span&gt;.
/// O TimeProvider vem por injeção, como no ChamadoServico: a hora atual nunca é lida direto.
/// </summary>
[HtmlTargetElement("prazo", Attributes = "criado-em,prioridade,status", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PrazoTagHelper(TimeProvider relogio) : TagHelper
{
    public DateTimeOffset CriadoEm { get; set; }

    public Prioridade Prioridade { get; set; }

    public StatusChamado Status { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Chamado encerrado não tem prazo a cumprir: nada é gerado.
        if (Status is StatusChamado.Resolvido or StatusChamado.Cancelado)
        {
            output.SuppressOutput();
            return;
        }

        // A regra (quanto tempo cada prioridade tem) é do domínio; aqui só se mostra o resultado.
        var restante = PrazoAtendimento.VenceEm(CriadoEm, Prioridade) - relogio.GetUtcNow();

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (restante >= TimeSpan.Zero)
        {
            output.Attributes.SetAttribute("class", "selo selo-no-prazo");
            output.Content.SetContent("Vence em " + restante.Duracao());
        }
        else
        {
            output.Attributes.SetAttribute("class", "selo selo-atrasado");
            output.Content.SetContent("Atrasado há " + restante.Negate().Duracao());
        }
    }
}
