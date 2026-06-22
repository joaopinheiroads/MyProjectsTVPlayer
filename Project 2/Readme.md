# Projeto 2 — Cardápio Digital (Plataforma)

Plataforma web de **gerenciamento de cardápio digital** (back-office + cardápio do cliente final). É o sistema onde os estabelecimentos cadastram empresa, produtos, categorias, adicionais, promoções, mesas e acompanham os pedidos em tempo real; o cliente final acessa o cardápio por **QR Code** e monta o pedido.

> Aplicação real desenvolvida para a plataforma **EscolhaAÍ**. Este repositório é público e **não contém credenciais** (connection strings, chaves JWT, etc. ficam fora do versionamento).

## Visão geral

- **Painel administrativo** (back-office) para o estabelecimento gerenciar todo o cardápio.
- **Cardápio do cliente** acessível por QR Code, com **carrinho de compras** e envio de pedido.
- **Pedidos em tempo real** via SignalR (o painel recebe novos pedidos sem recarregar).
- **Multi-empresa (multi-tenant):** cada usuário/empresa enxerga apenas os próprios dados.

## Principais funcionalidades

- Cadastro e gestão de **empresas**, **usuários** e perfis de acesso.
- **Produtos**, **categorias**, **grupos de adicionais** e itens adicionais.
- **Promoções** e **horários** de disponibilidade (produto e promoção).
- **Mesas** e geração de **QR Code** personalizado (layout configurável) por empresa/mesa.
- **Carrinho de compras** e fluxo de **confirmação de pedido** (cardápio do cliente).
- **Autenticação** com JWT, login por **e-mail/SMS com código de verificação** e login Google.
- **Upload e processamento de imagens** (redimensionamento/thumbnail).
- **Exportação** de produtos.

## Stack

- **.NET 8** · **Blazor Web App** (render mode *Interactive Server*)
- **ASP.NET Core** Web API (controllers REST) + **Swagger/OpenAPI**
- **Entity Framework Core 8** + **SQL Server**
- **SignalR** (pedidos em tempo real)
- **Radzen.Blazor** (componentes de UI)
- **QRCoder** (geração de QR Code)
- **SixLabors.ImageSharp** / **SkiaSharp** (processamento de imagens)
- Autenticação **JWT (Bearer)** com `AuthenticationStateProvider` customizado
- Versionado com **SVN (TortoiseSVN)**

## Arquitetura

Solução com dois projetos:

| Projeto | Papel |
|---------|-------|
| `Cardápio` | Host/servidor: API REST, autenticação, SignalR, acesso a dados e renderização Blazor Server. |
| `Cardápio.Client` | Biblioteca Razor com as páginas, componentes, modais e serviços de UI. |

Organização em camadas dentro do servidor (`Cardápio/`):

```
Cardápio/
├─ Web/            → controllers REST por área (Auth, Product, Category, Pedido, QrCodeLayout, ...)
├─ Domain/         → serviços de regra de negócio (ProductService, PedidoService, QrCodeService, ...)
├─ Infra/
│  ├─ Data/        → AppDbContext (EF Core)
│  ├─ Model/       → entidades de domínio
│  ├─ Repositories/→ repositórios por entidade
│  ├─ Interfaces/  → contratos de repositórios e Unit of Work
│  ├─ UnitsOfWork/ → CardapioUnitOfWork (coordena os repositórios)
│  └─ Services/    → serviços de infraestrutura (caminhos, imagens, URLs)
├─ Hubs/           → PedidoHub (SignalR)
└─ Program.cs      → composição: DI, JWT, SignalR, Swagger, arquivos estáticos
```

Padrões aplicados: **Repository + Unit of Work**, **injeção de dependência**, serviços de domínio separados dos controllers, e DTOs para entrada/saída da API.

## Como executar (resumo)

Pré-requisitos: **.NET 8 SDK** e **SQL Server**.

```bash
# a partir de CardapioDigital/
dotnet build "Cardápio.sln"
dotnet run --project "Cardápio/Cardápio/Cardápio.csproj"
```

Configurar localmente (fora do versionamento) a connection string `DefaultConnection`, a URL base `DefaultConnectionHttp` e a seção `Jwt` (`Key`, `Issuer`, `Audience`). Em ambiente de desenvolvimento, a documentação da API fica em **`/api-docs`** (Swagger).
