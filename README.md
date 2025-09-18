# Mottu Frota API

API RESTful para gerenciamento de **motos, filiais e manutenções**, desenvolvida em **.NET 7**, com documentação Swagger/OpenAPI e HATEOAS implementado.

---

## Integrantes
- Marcelo Scoleso  
- João Santis  
- João Paulo  

---

## Justificativa da Arquitetura

A API segue o modelo **RESTful**, utilizando **Controllers** para cada entidade (`Motos`, `Filiais`, `Manutencoes`) com endpoints CRUD completos.  

Principais escolhas arquiteturais:

- **.NET Web API**: framework robusto e consolidado para APIs REST.  
- **Entity Framework Core**: persistência de dados e gerenciamento de relacionamentos.  
- **HATEOAS**: links de navegação para facilitar consumo e descoberta de recursos.  
- **Swagger/OpenAPI**: documentação interativa com exemplos de payloads.  

Essa arquitetura garante **manutenção fácil, escalabilidade e clareza na comunicação** entre o front-end e a API.

---

## Tecnologias Utilizadas
- .NET 7  
- ASP.NET Core Web API  
- Entity Framework Core  
- Swagger/OpenAPI  
- Newtonsoft.Json  

---

## Estrutura do Projeto

Mottu.FrotaApi/
├── Controllers/
│ ├── MotosController.cs
│ ├── FiliaisController.cs
│ └── ManutencoesController.cs
├── Data/
│ └── AppDbContext.cs
├── Models/
│ ├── Moto.cs
│ ├── Filial.cs
│ └── Manutencao.cs
├── Program.cs
└── README.md

## Instruções de Execução

1. **Clonar o repositório**:

git clone https://github.com/MarceloScoleso/Mottu.FrotaApi.git
cd Mottu.FrotaApi

2. **Restaurar pacotes e construir o projeto**:

dotnet restore
dotnet build

3. **Executar a API**:

dotnet run

4. **Acessar o Swagger UI**:

https://localhost:5001/swagger

## Endpoints

- Entidade	Método	Rota	Descrição
- Motos	GET	/api/Motos	Lista motos (com paginação)
- Motos	GET	/api/Motos/{id}	Obter moto por ID
- Motos	POST	/api/Motos	Criar nova moto
- Motos	PUT	/api/Motos/{id}	Atualizar moto existente
- Motos	DELETE	/api/Motos/{id}	Deletar moto
- Filiais	GET	/api/Filiais	Lista filiais
- Filiais	GET	/api/Filiais/{id}	Obter filial por ID
- Filiais	POST	/api/Filiais	Criar nova filial
- Filiais	PUT	/api/Filiais/{id}	Atualizar filial existente
- Filiais	DELETE	/api/Filiais/{id}	Deletar filial
- Manutencoes	GET	/api/Manutencoes	Lista manutenções
- Manutencoes	GET	/api/Manutencoes/{id}	Obter manutenção por ID
- Manutencoes	POST	/api/Manutencoes	Criar nova manutenção
- Manutencoes	PUT	/api/Manutencoes/{id}	Atualizar manutenção existente
- Manutencoes	DELETE	/api/Manutencoes/{id}	Deletar manutenção

## Exemplos de Uso

Listar motos com paginação e HATEOAS
GET /api/Motos?page=1&pageSize=10


Resposta exemplo:

{
  "totalItems": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "data": [
    {
      "id": 1,
      "placa": "ABC-1234",
      "modelo": "Honda CG 160",
      "status": "Disponível",
      "filial": {
        "id": 1,
        "nome": "Filial Central"
      },
      "links": [
        { "rel": "self", "href": "/api/Motos/1" },
        { "rel": "update", "href": "/api/Motos/1" },
        { "rel": "delete", "href": "/api/Motos/1" }
      ]
    }
  ],
  "links": [
    { "rel": "self", "href": "/api/Motos?page=1&pageSize=10" },
    { "rel": "next", "href": "/api/Motos?page=2&pageSize=10" },
    { "rel": "prev", "href": null }
  ]
}

Criar uma nova moto
POST /api/Motos


Payload exemplo:

{
  "placa": "XYZ-9876",
  "modelo": "Honda CB 500",
  "status": "Disponível",
  "filialId": 1
}


Resposta: 201 Created com o objeto criado.
