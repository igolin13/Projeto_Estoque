# RESEARCH.md — Módulo Expedição

## Contexto do Projeto

Sistema Web de Controle de Estoque e Expedição em **ASP.NET Core MVC** com **Entity Framework Core** e **SQL Server**.

O projeto é dividido entre membros da equipe. Este repositório cobre exclusivamente o módulo **Expedição**.

## Escopo deste Módulo

A `Expedicao` é a última etapa do fluxo logístico. Ela **não controla estoque nem vendas** — apenas registra o envio de pedidos já criados por outros módulos.

### Dependências externas (de outros membros da equipe)
- `Pedido` (com `DataPedido`, `ValorTotal`, `ClienteId`)
- `Cliente` (com `Nome`)
- `PedidoProduto` (tabela intermediária N:N)
- `Produto` (com `Nome`, `Preco`)

Este módulo **consome** essas entidades via `DbContext` compartilhado — não as redefine.

## Entidade Principal

```csharp
public class Expedicao
{
    public int Id { get; set; }
    public DateTime DataEnvio { get; set; }
    public string Transportadora { get; set; }
    public string Status { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; }
}
```

## Estados da Expedição (enum)

| Valor | Descrição |
|-------|-----------|
| `Pendente` | Expedição registrada, aguardando início |
| `EmSeparacao` | Produtos sendo separados no estoque |
| `ProntoParaEnvio` | Pronto para despachar |
| `Despachado` | Entregue à transportadora |
| `EmTransporte` | Em rota de entrega |
| `Entregue` | Entrega confirmada |
| `Cancelado` | Expedição cancelada |

## Regras de Negócio

| # | Regra |
|---|-------|
| RN1 | `PedidoId` é obrigatório — expedição sem pedido é inválida |
| RN2 | `DataEnvio >= DataPedido` — não pode enviar antes do pedido existir |
| RN3 | Pedido com status `Entregue` não pode regredir para `Pendente`, `EmSeparacao` ou `ProntoParaEnvio` |
| RN4 | Ao criar uma expedição, o pedido deve estar associado a um `Cliente` |

## Consultas LINQ Obrigatórias

| # | Objetivo | Técnica |
|---|----------|---------|
| Q1 | Pedidos e seus clientes | Join simples (`Include`) |
| Q2 | Quantidade de pedidos por transportadora | `GroupBy` |
| Q3 | Transportadoras com mais de X envios | `GroupBy` + `Where` (Having) |

## Telas Previstas

- **Index** — tabela com ID, Pedido, Cliente, Transportadora, Data de Envio, Status
- **Create** — formulário (Pedido, Transportadora, DataEnvio, Status)
- **Edit** — editar status e demais campos
- **Details** — Cliente, Produtos do Pedido, Quantidade, Valor Total, dados da Expedição
- **Delete** — confirmação de exclusão
