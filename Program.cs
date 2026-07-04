using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

const string CorsPolicyName = "GlowMindCors";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(port, out var parsedPort))
{
    app.Urls.Add($"http://0.0.0.0:{parsedPort}");
}

app.UseCors(CorsPolicyName);

var opinioesDaSara = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["brenda"] = "A Brenda e simplesmente perfeita, um arraso total.",
    ["ana"] = "A Ana e uma diva, delicada e encantadora demais.",
    ["karize"] = "A Karize e uma professora incrivel, elegante e admiravel.",
    ["nathan"] = "O Nathan e carismatico, alto astral e muito tranquilo.",
    ["jonathan"] = "O Jonathan e um parceiro engracado, leve e sempre rende risada.",
    ["beatriz"] = "A Beatriz e estilosa, chique e deslumbrante.",
    ["emerson"] = "O Emerson e divertido, espontaneo e bem de boa.",
    ["eduardo"] = "O Eduardo e muito simpatico, leve e cativante.",
    ["arthur machado"] = "O Arthur Machado e um bom amigo, confiavel e de presenca boa.",
    ["arthur"] = "O Arthur e tranquilo, gentil e facil de lidar.",
    ["diego"] = "O Diego e animado, divertido e a gente compartilha uns gostos muito especificos.",
    ["henrique"] = "O Henrique e dedicado, educado e bastante agradavel.",
    ["henrique delegrego"] = "O Henrique Delegrego e um professor elegante, observador e bem respeitavel.",
    ["katheriny"] = "A Katheriny e graciosa, estilosa e cheia de charme.",
    ["marcio"] = "O Marcio e um professor bem parceiro, legal e muito gente boa.",
    ["pablo"] = "O Pablo e quietao na dele, observador e de boa.",
    ["rafael"] = "O Rafael e centrado, respeitoso e inspirador.",
    ["maria leticia"] = "A Maria Leticia e doce, sofisticada e encantadora.",
    ["matheus araujo"] = "O Matheus Araujo tem uma personalidade forte e exige bastante paciencia no dia a dia."
};

var opinioesNormalizadas = opinioesDaSara.ToDictionary(
    item => NormalizeName(item.Key),
    item => item,
    StringComparer.Ordinal);

var encostos = new HashSet<string>(StringComparer.Ordinal)
{
    NormalizeName("matheus araujo")
};

app.MapMethods("/{*path}", new[] { "OPTIONS" }, () => Results.Ok())
    .RequireCors(CorsPolicyName);

app.MapGet("/", () => Results.Ok(new
{
    service = "glow-mind-api",
    endpoints = new[]
    {
        "/opiniao/{nome}",
        "/health"
    }
}))
    .RequireCors(CorsPolicyName);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .RequireCors(CorsPolicyName);

app.MapGet("/opiniao/{nome}", (string nome) =>
{
    var nomeLimpo = nome.Trim();
    if (string.IsNullOrWhiteSpace(nomeLimpo))
    {
        return Results.BadRequest(new { erro = "O nome precisa ser informado." });
    }

    var nomeNormalizado = NormalizeName(nomeLimpo);

    if (opinioesNormalizadas.TryGetValue(nomeNormalizado, out var opiniaoExata))
    {
        return Results.Ok(new OpiniaoResponse(
            FormatDisplayName(opiniaoExata.Key),
            true,
            encostos.Contains(nomeNormalizado),
            opiniaoExata.Value));
    }

    var melhorCorrespondencia = FindBestMatch(nomeNormalizado, opinioesNormalizadas.Keys);
    if (melhorCorrespondencia is not null &&
        opinioesNormalizadas.TryGetValue(melhorCorrespondencia, out var opiniaoParecida))
    {
        return Results.Ok(new OpiniaoResponse(
            FormatDisplayName(opiniaoParecida.Key),
            true,
            encostos.Contains(melhorCorrespondencia),
            opiniaoParecida.Value));
    }

    return Results.Ok(new OpiniaoResponse(
        nomeLimpo,
        false,
        false,
        $"Sara ainda nao tem uma opiniao sobre {nomeLimpo.ToLowerInvariant()}."));
})
    .RequireCors(CorsPolicyName);

app.Run();

static string NormalizeName(string value)
{
    var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder();
    var previousWasSpace = false;

    foreach (var character in normalized)
    {
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
        if (unicodeCategory == UnicodeCategory.NonSpacingMark)
        {
            continue;
        }

        if (char.IsLetterOrDigit(character))
        {
            builder.Append(character);
            previousWasSpace = false;
            continue;
        }

        if (!previousWasSpace)
        {
            builder.Append(' ');
            previousWasSpace = true;
        }
    }

    return builder.ToString().Trim();
}

static string? FindBestMatch(string input, IEnumerable<string> candidates)
{
    string? bestMatch = null;
    var bestDistance = int.MaxValue;

    foreach (var candidate in candidates)
    {
        var distance = LevenshteinDistance(input, candidate);
        var maxAllowedDistance = Math.Max(1, candidate.Length / 3);

        if (distance <= maxAllowedDistance && distance < bestDistance)
        {
            bestDistance = distance;
            bestMatch = candidate;
        }
    }

    return bestMatch;
}

static int LevenshteinDistance(string source, string target)
{
    if (source == target)
    {
        return 0;
    }

    if (source.Length == 0)
    {
        return target.Length;
    }

    if (target.Length == 0)
    {
        return source.Length;
    }

    var previous = new int[target.Length + 1];
    var current = new int[target.Length + 1];

    for (var j = 0; j <= target.Length; j++)
    {
        previous[j] = j;
    }

    for (var i = 1; i <= source.Length; i++)
    {
        current[0] = i;

        for (var j = 1; j <= target.Length; j++)
        {
            var substitutionCost = source[i - 1] == target[j - 1] ? 0 : 1;

            current[j] = Math.Min(
                Math.Min(current[j - 1] + 1, previous[j] + 1),
                previous[j - 1] + substitutionCost);
        }

        (previous, current) = (current, previous);
    }

    return previous[target.Length];
}

static string FormatDisplayName(string value)
{
    return string.Join(' ',
        value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
}

internal sealed record OpiniaoResponse(string Nome, bool Conhecido, bool Encosto, string Opiniao);
