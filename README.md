"Problem Based Learning EC5" 

# 🧱 Configuração do Banco de Dados - Projeto PBL

Este projeto utiliza SQL Server com stored procedures para manipulação de dados. Abaixo estão os scripts SQL necessários para criar o banco, as tabelas e as procedures utilizadas.

---

## 1. Criar o Banco de Dados

```sql
CREATE DATABASE projeto_pbl;
GO
USE projeto_pbl;
```

---

## 2. Criar Tabelas

### 🔹 Tabela `sexos`

```sql
CREATE TABLE sexos (
    id INT NOT NULL PRIMARY KEY,
    nome VARCHAR(MAX) NOT NULL
);

INSERT INTO sexos VALUES
(1, 'Masculino'),
(2, 'Feminino'),
(3, 'Outro'),
(4, 'Prefiro não informar');
```

### 🔹 Tabela `usuarios`

```sql
CREATE TABLE usuarios (
    id INT PRIMARY KEY,
    nome VARCHAR(100),
    email VARCHAR(100),
    data_nascimento DATETIME,
    cep VARCHAR(20),
    logradouro VARCHAR(100),
    numero INT,
    cidade VARCHAR(100),
    estado VARCHAR(100),
    loginUsuario VARCHAR(50),
    senha VARCHAR(50),
    sexoId INT,
    imagem VARBINARY(MAX),
    FOREIGN KEY (sexoId) REFERENCES sexos(id)
);
```

### 🔹 Tabela `sensor`

```sql
CREATE TABLE sensor (
    id INT PRIMARY KEY,
    nomeSensor VARCHAR(100) NOT NULL,
    descricaoSensor VARCHAR(255),
    localInstalacao VARCHAR(100),
    valorInstalacao DECIMAL(10,2),
    dataInstalacao DATETIME
);
```

---

## 3. Stored Procedures Genéricas

```sql
CREATE PROCEDURE spProximoId (@tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('SELECT ISNULL(MAX(id)+1, 1) AS MAIOR FROM ' + @tabela)
END
GO

CREATE PROCEDURE spConsulta (@id INT, @tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('SELECT * FROM ' + @tabela + ' WHERE id = ' + CAST(@id AS VARCHAR))
END
GO

CREATE PROCEDURE spDelete (@id INT, @tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('DELETE FROM ' + @tabela + ' WHERE id = ' + CAST(@id AS VARCHAR))
END
GO

CREATE PROCEDURE spListagem (@tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('SELECT * FROM ' + @tabela)
END
GO
```

---

## 4. Stored Procedures Específicas

### 👤 `usuarios`

```sql
CREATE PROCEDURE spInsert_usuarios (
    @id INT, @nome VARCHAR(MAX), @email VARCHAR(MAX), @data_nascimento DATETIME,
    @cep VARCHAR(MAX), @logradouro VARCHAR(MAX), @numero INT, @cidade VARCHAR(MAX),
    @estado VARCHAR(MAX), @loginUsuario VARCHAR(MAX), @senha VARCHAR(MAX),
    @sexoId INT, @imagem VARBINARY(MAX)
)
AS
BEGIN
    INSERT INTO usuarios VALUES
    (@id, @nome, @email, @data_nascimento, @cep, @logradouro, @numero, @cidade,
     @estado, @loginUsuario, @senha, @sexoId, @imagem)
END
GO

CREATE PROCEDURE spUpdate_usuarios (
    @id INT, @nome VARCHAR(MAX), @email VARCHAR(MAX), @data_nascimento DATETIME,
    @cep VARCHAR(MAX), @logradouro VARCHAR(MAX), @numero INT, @cidade VARCHAR(MAX),
    @estado VARCHAR(MAX), @loginUsuario VARCHAR(MAX), @senha VARCHAR(MAX),
    @sexoId INT, @imagem VARBINARY(MAX)
)
AS
BEGIN
    UPDATE usuarios SET
        nome = @nome,
        email = @email,
        data_nascimento = @data_nascimento,
        cep = @cep,
        logradouro = @logradouro,
        numero = @numero,
        cidade = @cidade,
        estado = @estado,
        loginUsuario = @loginUsuario,
        senha = @senha,
        sexoId = @sexoId,
        imagem = @imagem
    WHERE id = @id
END
GO
```

### 🌡️ `sensor`

```sql
CREATE PROCEDURE sp_verificar_sensor (@nomeSensor VARCHAR(100))
AS
BEGIN
    SELECT COUNT(*) AS cont FROM sensor WHERE nomeSensor = @nomeSensor
END
GO

CREATE PROCEDURE spInsert_sensor (
    @id INT, @nomeSensor VARCHAR(100), @descricaoSensor VARCHAR(255),
    @localInstalacao VARCHAR(100), @valorInstalacao DECIMAL(10,2), @dataInstalacao DATETIME
)
AS
BEGIN
    INSERT INTO sensor VALUES (
        @id, @nomeSensor, @descricaoSensor,
        @localInstalacao, @valorInstalacao, @dataInstalacao
    )
END
GO

CREATE PROCEDURE spUpdate_sensor (
    @id INT, @nomeSensor VARCHAR(100), @descricaoSensor VARCHAR(255),
    @localInstalacao VARCHAR(100), @valorInstalacao DECIMAL(10,2), @dataInstalacao DATETIME
)
AS
BEGIN
    UPDATE sensor SET
        nomeSensor = @nomeSensor,
        descricaoSensor = @descricaoSensor,
        localInstalacao = @localInstalacao,
        valorInstalacao = @valorInstalacao,
        dataInstalacao = @dataInstalacao
    WHERE id = @id
END
GO
```


