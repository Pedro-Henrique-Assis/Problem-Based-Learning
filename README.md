# Projeto PBL 🚀

Este projeto foi desenvolvido como parte do Projeto Baseado em Problemas (PBL) do curso de Engenharia da Computação, com foco em aplicações IoT, coleta e análise de dados e construção de sistemas robustos em ASP.NET Core 3.1 com SQL Server.


## 📝 Descrição Geral

O ProjetoPBL é uma aplicação web que permite o cadastro, edição, exclusão e consulta de sensores, além da integração com a plataforma FIWARE para o gerenciamento de sensores de temperatura. O sistema realiza validações, autenticação de usuários, coleta automática de dados, exibição em dashboards e operações CRUD com SQL Server.

## ✅ Funcionalidades

- 🧾 Cadastro, edição e exclusão de sensores  
- ✔️ Validação de dados (nome, descrição, local, valor, data de instalação)  
- 🔄 Integração com a API do FIWARE para criação, leitura e exclusão de sensores  
- 🌡️ Coleta automática de temperaturas  
- 🔐 Autenticação de usuários com sessões  
- 📊 Dashboard de temperaturas em tempo real  
- 📈 Regressão linear entre temperatura e voltagem  
- 🔎 Consulta avançada de sensores e usuários  
- 🛠️ Gerenciamento e resposta de chamados  
- 🛡️ Proteção de rotas com base em permissões de administrador  

## 🧰 Tecnologias Utilizadas

- 🖥️ ASP.NET Core 3.1 (MVC)  
- 💻 C#  
- 🗃️ SQL Server com Stored Procedures  
- 🌐 JavaScript / jQuery / AJAX  
- 📉 Chart.js  
- 🌍 FIWARE (NGSI, IoT Agent, Orion Context Broker, STH Comet)   

## 🔗 Integração com FIWARE

A integração do sistema com a plataforma FIWARE permite a comunicação com sensores IoT para obtenção e armazenamento de dados de temperatura em tempo real. A aplicação consome dados por meio da API STH-Comet utilizando requisições HTTP, realizando o tratamento e inserção das informações no banco de dados apenas se ainda não existirem, garantindo eficiência e integridade.

A URL utilizada para leitura é:  
`http://<ip_fiware>:8666/STH/v1/contextEntities/type/Temperature/id/urn:ngsi-ld:Temperature:001/attributes/temperature?lastN=100`

As requisições são autenticadas com os seguintes cabeçalhos obrigatórios:  
`Fiware-Service: smart`  
`Fiware-ServicePath: /`

Além disso, o sistema realiza automaticamente a coleta desses dados por meio de agendamentos ou chamadas diretas ao método responsável no controlador de temperatura, mantendo os dashboards sempre atualizados.

## 🔒 Segurança e Sessões

- Os usuários são autenticados via login e senha  
- A sessão guarda `Logado`, `IdUsuario`, `NomeUsuario` e `IsAdmin`  
- As áreas administrativas só podem ser acessadas por usuários com `IsAdmin = true`  
- A exclusão de usuários está protegida para não permitir apagar o último administrador nem o próprio usuário  

## 📈 Dashboards

### Dashboard de Temperaturas

- 📉 Exibe gráfico de linha com temperaturas em tempo real  
- 🔄 Atualização automática a cada 5 segundos com dados do endpoint `/Temperatura/Listar`  

### Dashboard com Regressão Linear

- 📊 Mostra a relação entre voltagem e temperatura  
- 🧮 Inclui tabela de medições, cálculos ponderados, resíduos e coeficientes da regressão linear  
- 📐 Representação gráfica com scatter plot e linha de regressão calculada com base em fórmulas estatísticas  

## 🔍 Consultas Avançadas

- **Usuários**: Filtro por nome, estado, sexo, datas e login  
- **Sensores**: Filtro por local, valores mínimo/máximo e datas  

## 🆘 Chamados

- 📩 Chamados podem ser abertos por usuários autenticados  
- 📝 Chamados têm título, descrição, status, data de abertura, usuário e resposta  
- 🛡️ A administração pode listar, responder, atualizar ou excluir chamados  
- 👤 Os chamados são protegidos por controle de permissões e podem ser listados por usuário ou por todos  

## ⚙️ Requisitos para Execução

1. 📦 .NET Core SDK 3.1  
2. 🛢️ SQL Server local ou remoto  
3. ☁️ Instância do FIWARE (IoT Agent + Orion + STH)  

## 🛠️ Configuração do Banco de Dados

- Banco: `projeto_pbl`  
- Tabelas: `usuarios`, `sexos`, `sensores`, `temperaturas`, `chamados`  
- Procedures:  
  - `spInsert_*`  
  - `spUpdate_*`  
  - `spDelete_*`  
  - `spConsultaAvancadaUsuarios`  
  - `spExisteRegistro`  
  - `spInserirTemperatura`  
  - `spListarTemperaturas`  
  - `spResponderChamado`  
  - `spConsultaChamados`  

## 🎓 Considerações Finais

A aplicação reflete os desafios de um cenário real, simulando um sistema de monitoramento remoto de temperatura em estufas, com coleta de dados confiável, visualização em tempo real, análise estatística e gerenciamento administrativo seguro.


