# Projeto - Cidades ESGInteligentes

API .NET 8 para monitoramento ESG (relatórios de sustentabilidade, fornecedores, pegada de carbono e alertas de conformidade). Esta entrega integra práticas de DevOps com CI/CD, containerização e orquestração.

## Como executar localmente com Docker

Pré‑requisitos: Docker e Docker Compose instalados.

```bash
# clonar o repositório
# cp ".env.example" ".env"  # opcional
docker compose up -d --build
# a API ficará disponível em http://localhost:8080/swagger
```

## Pipeline CI/CD

**Ferramenta**: GitHub Actions  
**Etapas**:
1. **Build & Test**: restaura dependências, compila a solução e executa testes xUnit.
2. **Image**: build da imagem Docker e push no GHCR (ghcr.io).
3. **Deploy Staging**: SSH em VM de staging e sobe `docker compose` com imagem da revisão.
4. **Deploy Produção**: (manual via `workflow_dispatch`) faz o mesmo procedimento apontando para produção.

**Secrets necessários**: `SSH_HOST_STAGING`, `SSH_HOST_PROD`, `SSH_USER`, `SSH_KEY` (chave privada), além do `GITHUB_TOKEN` padrão para o GHCR.

## Containerização

**Dockerfile (multi-stage)**: compila, testa e publica a API, depois executa em runtime `aspnet:8.0`. Porta exposta: **8080**.  
**Compose**: orquestra **api** + **PostgreSQL** com rede e volume persistente. A conexão padrão usa `Host=db;Port=5432;Database=esgdb;Username=esg;Password=esgpass`.

## Prints do funcionamento

Inclua aqui suas evidências locais e de deploy (staging e produção). Exemplos:
- `docker compose ps`
- `docker logs <container>`
- Screenshot do Swagger local e dos ambientes (URLs de staging/produção).

## Tecnologias utilizadas

- .NET 8 (ASP.NET Core Web API), xUnit
- Docker, Docker Compose
- GitHub Actions (CI/CD)
- PostgreSQL 16 (base de orquestração)

## Estrutura do projeto (mínima)

```
Trabalho Facul/
├── Dockerfile
├── docker-compose.yml
├── .env.example
├── src/
│   ├── ESGMonitoring.API/
│   └── ESGMonitoring.Tests/
└── .github/workflows/ci-cd.yml
```

## Checklist de Entrega

| Item | OK |
|---|---|
| Projeto compactado em .ZIP com estrutura organizada | ☐ |
| Dockerfile funcional | ☐ |
| docker-compose.yml ou arquivos Kubernetes | ☐ |
| Pipeline com etapas de build, teste e deploy | ☐ |
| README.md com instruções e prints | ☐ |
| Documentação técnica com evidências (PDF ou PPT) | ☐ |
| Deploy realizado nos ambientes staging e produção | ☐ |

---

> Observação: Os testes incluídos são de _smoke_ (mínimo viável). Recomenda‑se ampliar a cobertura com testes de controllers/serviços específicos.