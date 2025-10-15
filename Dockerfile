# 1. Escolha uma imagem base oficial do Node.js
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