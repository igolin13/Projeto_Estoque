# AGENTS.md — Instruções para IA (GSD Framework)

## Contexto do Projeto

Este é um projeto acadêmico ASP.NET Core MVC de controle de estoque.
Este repositório cobre **apenas o módulo Expedição**.
Os módulos de Produto, Cliente e Pedido são implementados por outros membros da equipe.

## Stack Obrigatória

- **Framework:** ASP.NET Core MVC (.NET 8)
- **ORM:** Entity Framework Core
- **Banco:** SQL Server (LocalDB em desenvolvimento)
- **Views:** Razor Pages (.cshtml)
- **Padrão:** MVC tradicional com scaffolding manual

## O que a IA DEVE fazer

- Gerar código C# válido para .NET 8 / ASP.NET Core 8
- Usar `async/await` em todas as actions do controller
- Usar `DbContext` injetado via construtor (nunca instanciar diretamente)
- Usar `ThenInclude` para carregar relacionamentos aninhados
- Usar `ModelState.IsValid` para validações antes de salvar
- Usar data annotations (`[Required]`, `[Display]`, `[DataType]`) nos models
- Usar `enum` para o campo `Status`
- Usar Bootstrap 5 nas views (já incluso no template MVC padrão)
- Usar `asp-for`, `asp-action`, `asp-controller` nas views Razor

## O que a IA NÃO deve fazer

- Criar um projeto do zero com `dotnet new mvc` — o projeto já existe
- Recriar entidades `Produto`, `Cliente`, `Pedido` — elas vêm de outros membros
- Usar AutoMapper sem justificativa explícita
- Colocar lógica de negócio nas views
- Usar `ViewBag` para transferir listas de lookup — usar `ViewData` ou ViewModel
- Fazer migrations automáticas no código — sempre via CLI
- Ignorar as regras de negócio RN1–RN4 do RESEARCH.md

## Estrutura de Pastas Esperada

```
/Controllers/ExpedicaoController.cs
/Models/Expedicao.cs
/Models/StatusExpedicao.cs
/Views/Expedicao/Index.cshtml
/Views/Expedicao/Create.cshtml
/Views/Expedicao/Edit.cshtml
/Views/Expedicao/Details.cshtml
/Views/Expedicao/Delete.cshtml
```

## Contexto Compartilhado (de outros módulos)

A IA pode assumir que o projeto já contém:

```csharp
// Models/Cliente.cs
public class Cliente {
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
}

// Models/Produto.cs
public class Produto {
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Categoria { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}

// Models/PedidoProduto.cs (tabela N:N)
public class PedidoProduto {
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; }
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; }
    public int Quantidade { get; set; }
}

// Models/Pedido.cs
public class Pedido {
    public int Id { get; set; }
    public DateTime DataPedido { get; set; }
    public decimal ValorTotal { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
    public ICollection<PedidoProduto> PedidoProdutos { get; set; }
    public Expedicao Expedicao { get; set; }
}

// Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext {
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoProduto> PedidoProdutos { get; set; }
    public DbSet<Expedicao> Expedicoes { get; set; } // adicionado por este módulo
}
```

## Regras de Negócio a Respeitar

- **RN1:** PedidoId é obrigatório
- **RN2:** DataEnvio >= DataPedido
- **RN3:** Status `Entregue` não pode regredir
- **RN4:** Pedido deve ter Cliente ao criar expedição

## Consultas LINQ Obrigatórias

Todas as 3 consultas do RESEARCH.md devem aparecer no código final:
- Q1: `Include` + `ThenInclude`
- Q2: `GroupBy` simples
- Q3: `GroupBy` + `Where` (Having)
