# Feedico API — C# / .NET example

> Fetch **affiliate merchants** and **live coupon codes** from [Feedico](https://feedico.io) with a tiny `HttpClient` wrapper. No SDK required.

**Website:** [feedico.io](https://feedico.io) · **Documentation:** [feedico.io/docs](https://feedico.io/docs)

`feedico` · `coupon-api` · `affiliate-api` · `merchants` · `coupons` · `rest-api` · `csharp` · `dotnet` · `api-example`

---

## What this repo is

A minimal .NET console app that shows how to:

- authenticate with your Feedico **Bearer token** (`fdco_…`)
- `POST` to `/me/networks` and read merchant rows
- `POST` to `/me/coupons` and read coupon rows
- paginate and filter by network provider or merchant name

Same REST surface as the official [Feedico Sync WordPress plugin](https://github.com/feedico-io/feedico-wp-plugin).

---

## 30-second start

```bash
git clone https://github.com/feedico-io/feedico-api-csharp-example.git
cd feedico-api-csharp-example
cp .env.example .env
# Paste your fdco_… token into .env

dotnet run                      # merchants + coupons
dotnet run -- merchants         # merchants only
dotnet run -- coupons           # coupons only
```

**Requirements:** [.NET 8 SDK](https://dotnet.microsoft.com/download) (or retarget `net8.0` in the `.csproj`).

---

## Get your API token

1. Sign in at **[feedico.io](https://feedico.io)**
2. Copy the **Bearer token** from your dashboard (`fdco_…`)
3. Set `FEEDICO_API_TOKEN` in `.env`

Integration guides, network docs, and OpenAPI links: **[feedico.io/docs](https://feedico.io/docs)**.

---

## API endpoints used

| Resource | Method | URL |
|----------|--------|-----|
| Merchants (networks) | `POST` | `https://api.feedico.io/api/v1/me/networks` |
| Coupons | `POST` | `https://api.feedico.io/api/v1/me/coupons` |

Request body (JSON):

```json
{
  "page": 1,
  "pageSize": 50,
  "provider": "cj",
  "firmName": ""
}
```

Headers:

```
Authorization: Bearer fdco_your_token
Content-Type: application/json
Accept: application/json
```

---

## Use the client in your app

```csharp
using var client = new FeedicoClient(Environment.GetEnvironmentVariable("FEEDICO_API_TOKEN")!);

using var doc = await client.ListCouponsAsync(page: 1, pageSize: 100);
var coupons = FeedicoClient.ExtractRows(doc);

foreach (var coupon in coupons)
{
    // coupon is JsonElement — map to your DTO or read fields directly
}
```

`FeedicoClient` is a single file — drop it into ASP.NET, worker services, or MAUI projects.

---

## Environment variables

| Variable | Description |
|----------|-------------|
| `FEEDICO_API_TOKEN` | **Required.** Bearer token from dashboard |
| `FEEDICO_PAGE` | Page number (default `1`) |
| `FEEDICO_PAGE_SIZE` | Rows per page (default `10`, max `500`) |
| `FEEDICO_PROVIDER` | Optional network slug, e.g. `cj`, `awin` |
| `FEEDICO_FIRM_NAME` | Optional merchant name filter |

---

## Related projects

| Repo | Language |
|------|----------|
| [feedico-api-php-example](https://github.com/feedico-io/feedico-api-php-example) | PHP |
| **This repo** | C# / .NET |
| [feedico-wp-plugin](https://github.com/feedico-io/feedico-wp-plugin) | WordPress plugin |

---

## License

MIT — use in tutorials, internal tools, and production apps.

Questions? [feedico.io](https://feedico.io) · [Documentation hub](https://feedico.io/docs)
