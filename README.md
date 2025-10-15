# Projeto - Cidades ESGInteligentes

## Como executar localmente com Docker

Descreva os passos para subir a aplicação localmente.

1. Clone o repositório.
2. Navegue até a pasta raiz do projeto.
3. Execute o comando: `docker-compose up --build`
4. Acesse `http://localhost:3000` no seu navegador.

---

## Pipeline CI/CD

A pipeline de integração e entrega contínua (CI/CD) foi implementada utilizando **GitHub Actions**.

O fluxo é disparado a cada `push` na branch `main` e consiste nas seguintes etapas:
- **build-and-test**: Constrói a imagem Docker da aplicação, faz o login no Docker Hub e envia a imagem para o registro.
- **deploy-staging**: Simula o deploy no ambiente de Staging.
- **deploy-production**: Simula o deploy no ambiente de Produção após o sucesso em Staging.

---

## Containerização

A aplicação é containerizada utilizando Docker. Abaixo está o conteúdo do `Dockerfile`:

```dockerfile# 1. Escolha uma imagem base oficial do Node.js
FROM node:22-slim

# 2. Defina o diretório de trabalho dentro do container
WORKDIR /app

# 3. Copie os arquivos de dependência primeiro (para aproveitar o cache do Docker)
COPY src/package.json ./

# 4. Instale as dependências da aplicação
RUN npm install

# 5. Copie o restante do código-fonte da aplicação
COPY src/ .

# 6. Exponha a porta que a aplicação usa dentro do container
EXPOSE 3000

# 7. Defina o comando para iniciar a aplicação quando o container for executado
CMD ["npm", "start"]