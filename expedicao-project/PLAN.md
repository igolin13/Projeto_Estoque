# PLAN.md — Módulo Expedição

## Objetivo

Implementar o módulo `Expedicao` dentro de um projeto ASP.NET Core MVC existente, entregando:
- Entidade `Expedicao` com enum `StatusExpedicao`
- `Migration` para criar a tabela
- `ExpedicaoController` com 5 actions (Index, Create, Edit, Details, Delete)
- Views Razor para cada action
- 3 consultas LINQ documentadas
- Validações respeitando as regras de negócio

---

## Estrutura de Arquivos a Criar

```
Models/
  Expedicao.cs
  StatusExpedicao.cs        ← enum

Controllers/
  ExpedicaoController.cs

Views/Expedicao/
  Index.cshtml
  Create.cshtml
  Edit.cshtml
  Details.cshtml
  Delete.cshtml

Data/
  ApplicationDbContext.cs   ← adicionar DbSet<Expedicao>
                              (os outros membros já devem ter este arquivo)

Migrations/
  *_AddExpedicao.cs         ← gerada via EF Core

RESEARCH.md                 ← este documento de especificação
PLAN.md                     ← plano de execução
AGENTS.md                   ← instruções para IA
```

---

## Plano de Execução — Passo a Passo

### Fase 1 — Modelos

**Passo 1.1** — Criar `StatusExpedicao.cs`
- Enum com os 7 estados definidos no RESEARCH.md

**Passo 1.2** — Criar `Expedicao.cs`
- Propriedades conforme especificação
- Data annotations para validação
- Referência ao enum `StatusExpedicao`
- Navigation property para `Pedido`

### Fase 2 — Banco de Dados

**Passo 2.1** — Registrar no `ApplicationDbContext`
- Adicionar `DbSet<Expedicao> Expedicoes`
- Configurar relacionamento com `Pedido` via Fluent API (ou data annotation)

**Passo 2.2** — Gerar Migration
```bash
dotnet ef migrations add AddExpedicao
dotnet ef database update
```

### Fase 3 — Controller

**Passo 3.1** — Criar `ExpedicaoController`
- Injetar `ApplicationDbContext` via construtor
- Implementar `Index()` com LINQ Q1 (Include de Pedido + Cliente)
- Implementar `Create()` GET/POST com validação RN1/RN2/RN4
- Implementar `Edit()` GET/POST com validação RN3
- Implementar `Details()` com Include completo (Pedido, Cliente, PedidoProduto, Produto)
- Implementar `Delete()` GET/POST

**Passo 3.2** — Criar ação `Relatorios()` (opcional/bônus)
- LINQ Q2: GroupBy transportadora
- LINQ Q3: GroupBy + Where Having

### Fase 4 — Views

**Passo 4.1** — `Index.cshtml`
- Tabela com colunas: ID, Pedido#, Cliente, Transportadora, DataEnvio, Status
- Badges coloridos por status
- Links: Detalhes, Editar, Excluir

**Passo 4.2** — `Create.cshtml`
- Dropdown de Pedidos disponíveis
- Campo Transportadora
- DatePicker DataEnvio
- Dropdown de Status (enum)

**Passo 4.3** — `Edit.cshtml`
- Mesmos campos do Create
- Validação RN3 exibida via `ModelState`

**Passo 4.4** — `Details.cshtml`
- Seção: dados da expedição
- Seção: dados do cliente
- Tabela de produtos do pedido (Nome, Qtd, Preço)
- Valor total do pedido

**Passo 4.5** — `Delete.cshtml`
- Resumo dos dados para confirmação
- Botão de confirmação + cancelar

### Fase 5 — Consultas LINQ

**Q1** — Pedidos com seus clientes (no Index):
```csharp
_context.Expedicoes
    .Include(e => e.Pedido)
    .ThenInclude(p => p.Cliente)
    .ToList()
```

**Q2** — GroupBy transportadora (em Relatorios ou Index):
```csharp
_context.Expedicoes
    .GroupBy(e => e.Transportadora)
    .Select(g => new { Transportadora = g.Key, Total = g.Count() })
    .ToList()
```

**Q3** — Having (filtro pós-agrupamento):
```csharp
_context.Expedicoes
    .GroupBy(e => e.Transportadora)
    .Where(g => g.Count() > 5)
    .Select(g => new { Transportadora = g.Key, Total = g.Count() })
    .ToList()
```

---

## Critérios de Conclusão

- [ ] `Expedicao.cs` com enum e data annotations
- [ ] DbSet registrado no contexto
- [ ] Migration gerada e aplicada
- [ ] 5 actions no controller funcionando
- [ ] 5 views Razor renderizando sem erro
- [ ] RN1, RN2, RN3 validadas no controller
- [ ] 3 consultas LINQ utilizadas
- [ ] Projeto compila (`dotnet build`) sem erros
