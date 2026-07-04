# glow-mind-api

API simples em ASP.NET Minimal API que recebe o nome de uma pessoa e devolve o que a Sara pensa dela.

## Requisitos

- .NET 8 SDK

## Rodando localmente

```bash
dotnet run
```

Exemplos:

```bash
GET /opiniao/joao
GET /opiniao/maria
GET /health
```

## Resposta

Nome conhecido:

```json
{
  "nome": "joao",
  "conhecido": true,
  "opiniao": "Sara acha o Joao muito legal."
}
```

Nome desconhecido:

```json
{
  "nome": "maria",
  "conhecido": false,
  "opiniao": "Sara ainda nao tem uma opiniao sobre maria."
}
```

## Deploy na Render

Crie um Web Service e use:

- Build Command: `dotnet publish -c Release -o out`
- Start Command: `dotnet out/glow-mind-api.dll`

A aplicacao ja esta preparada para usar a variavel de ambiente `PORT` da Render.
