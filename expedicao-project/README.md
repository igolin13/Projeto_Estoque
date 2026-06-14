# Módulo Expedição — Sistema de Estoque

> Parte do projeto acadêmico de Controle de Estoque e Expedição em ASP.NET Core MVC.

## O que este módulo entrega

- Entidade `Expedicao` com enum `StatusExpedicao`
- Controller completo com 5 actions + relatórios
- 5 Views Razor (Index, Create, Edit, Details, Delete, Relatorios)
- 3 consultas LINQ obrigatórias (Q1, Q2, Q3)
- Validações das 4 regras de negócio (RN1–RN4)

## Estrutura de arquivos

```
Models/
  Expedicao.cs              ← entidade principal
  StatusExpedicao.cs        ← enum com 7 estados

Controllers/
  ExpedicaoController.cs    ← 5 actions + Relatorios

Views/Expedicao/
  Index.cshtml              ← listagem com badges de status
  Create.cshtml             ← formulário de criação
  Edit.cshtml               ← edição com aviso de RN3
  Details.cshtml            ← dados completos + produtos do pedido
  Delete.cshtml             ← confirmação de exclusão
  Relatorios.cshtml         ← LINQ Q2 e Q3

Data/
  ApplicationDbContext_Expedicao_snippet.cs  ← linhas a adicionar no DbContext

RESEARCH.md                 ← especificação do módulo
PLAN.md                     ← plano de execução
AGENTS.md                   ← instruções para IA (GSD)
```

---

## Como integrar ao projeto dos outros membros

### Passo 1 — Copiar os arquivos

Copie as pastas abaixo para o projeto principal:
- `Models/Expedicao.cs`
- `Models/StatusExpedicao.cs`
- `Controllers/ExpedicaoController.cs`
- `Views/Expedicao/` (pasta inteira)

### Passo 2 — Atualizar o ApplicationDbContext

Abra `Data/ApplicationDbContext_Expedicao_snippet.cs` e siga as instruções comentadas.
As mudanças principais são:

```csharp
// 1. Adicionar DbSet
public DbSet<Expedicao> Expedicoes { get; set; }

// 2. Adicionar no OnModelCreating:
modelBuilder.Entity<Expedicao>()
    .HasOne(e => e.Pedido)
    .WithOne(p => p.Expedicao)
    .HasForeignKey<Expedicao>(e => e.PedidoId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Expedicao>()
    .Property(e => e.Status)
    .HasConversion<string>()
    .HasMaxLength(30)
    .IsRequired();
```

### Passo 3 — Adicionar navigation property no modelo Pedido

O modelo `Pedido` dos outros membros precisa ter:

```csharp
public Expedicao? Expedicao { get; set; }
```

### Passo 4 — Gerar e aplicar a Migration

```bash
dotnet ef migrations add AddExpedicao
dotnet ef database update
```

### Passo 5 — Adicionar link no menu (opcional)

Em `Views/Shared/_Layout.cshtml`, adicionar no `<nav>`:

```html
<li class="nav-item">
    <a class="nav-link" asp-controller="Expedicao" asp-action="Index">
        📦 Expedições
    </a>
</li>
```

---

## Regras de Negócio implementadas

| # | Regra | Onde |
|---|-------|------|
| RN1 | PedidoId obrigatório | `[Required]` + validação no POST |
| RN2 | DataEnvio >= DataPedido | Validação no Create POST e Edit POST |
| RN3 | Entregue não regride | Validação no Edit POST |
| RN4 | Pedido deve ter Cliente | Validação no Create POST |

## Consultas LINQ usadas

| # | Descrição | Técnica | Localização |
|---|-----------|---------|-------------|
| Q1 | Expedições com Pedido e Cliente | `Include` + `ThenInclude` | `Index()` |
| Q2 | Envios por transportadora | `GroupBy` | `Relatorios()` |
| Q3 | Transportadoras com mais de X envios | `GroupBy` + `Where` | `Relatorios()` |
