using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(port, out var parsedPort))
{
    app.Urls.Add($"http://0.0.0.0:{parsedPort}");
}

var opinioesDaSara = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["joao"] = "Sara acha o Joao muito legal.",
    ["ana"] = "Sara acha a Ana super querida.",
    ["carlos"] = "Sara acha o Carlos muito gente boa."
};

app.MapGet("/", () => Results.Ok(new
{
    service = "glow-mind-api",
    endpoints = new[]
    {
        "/opiniao/{nome}",
        "/health"
    }
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/opiniao/{nome}", (string nome) =>
{
    var nomeLimpo = nome.Trim();
    if (string.IsNullOrWhiteSpace(nomeLimpo))
    {
        return Results.BadRequest(new { erro = "O nome precisa ser informado." });
    }

    if (opinioesDaSara.TryGetValue(nomeLimpo, out var opiniaoConhecida))
    {
        return Results.Ok(new OpiniaoResponse(nomeLimpo, true, opiniaoConhecida));
    }

    return Results.Ok(new OpiniaoResponse(
        nomeLimpo,
        false,
        $"Sara ainda nao tem uma opiniao sobre {nomeLimpo.ToLowerInvariant()}."));
});

app.Run();

internal sealed record OpiniaoResponse(string Nome, bool Conhecido, string Opiniao);
