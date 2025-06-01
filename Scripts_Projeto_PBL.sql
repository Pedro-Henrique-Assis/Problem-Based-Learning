-- 1. Criar o Banco de Dados
CREATE DATABASE projeto_pbl;
GO
USE projeto_pbl;
GO

-- 2. Criar Tabelas

-- Tabela sexos
CREATE TABLE sexos (
    id INT NOT NULL PRIMARY KEY,
    nome VARCHAR(MAX) NOT NULL
);
-- Inserindo valores na tabela sexos
INSERT INTO sexos VALUES
(1, 'Masculino'),
(2, 'Feminino'),
(3, 'Outro'),
(4, 'Prefiro não informar');
GO

-- Tabela usuarios
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
    IsAdmin BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (sexoId) REFERENCES sexos(id)
);
GO

-- Tabela sensor
CREATE TABLE sensor (
    id INT PRIMARY KEY,
    nomeSensor VARCHAR(100) NOT NULL,
    descricaoSensor VARCHAR(255),
    localInstalacao VARCHAR(100),
    valorInstalacao DECIMAL(10,2),
    dataInstalacao DATETIME
);
GO

-- Tabela Temperaturas
CREATE TABLE Temperaturas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SensorId VARCHAR(100) NULL,
    RecvTime DATETIME2 NOT NULL,
    Temperature FLOAT NOT NULL
);
GO

-- Tabela Chamados
CREATE TABLE chamados (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    descricao TEXT NOT NULL,
    status VARCHAR(50) NOT NULL,
    data_abertura DATETIME NOT NULL,
    usuario_id INT NOT NULL,
    resposta TEXT NULL
);
GO

-- 3. Stored Procedures Genéricas

-- Procedure para pegar o próximo ID de uma tabela
CREATE PROCEDURE spProximoId (@tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('SELECT ISNULL(MAX(id)+1, 1) AS MAIOR FROM ' + @tabela)
END
GO

-- Procedure para consultar dados por ID
CREATE PROCEDURE spConsulta (@id INT, @tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('SELECT * FROM ' + @tabela + ' WHERE id = ' + CAST(@id AS VARCHAR))
END
GO

-- Procedure para deletar um registro por ID
CREATE PROCEDURE spDelete (@id INT, @tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('DELETE FROM ' + @tabela + ' WHERE id = ' + CAST(@id AS VARCHAR))
END
GO

-- Procedure para listar todos os registros de uma tabela
CREATE PROCEDURE spListagem (@tabela VARCHAR(MAX)) AS
BEGIN
    EXEC('SELECT * FROM ' + @tabela)
END
GO

-- 4. Stored Procedures Específicas

-- Para usuários

-- Insere um novo usuário
CREATE PROCEDURE spInsert_usuarios (
    @id INT, @nome VARCHAR(MAX), @email VARCHAR(MAX), @data_nascimento DATETIME,
    @cep VARCHAR(MAX), @logradouro VARCHAR(MAX), @numero INT, @cidade VARCHAR(MAX),
    @estado VARCHAR(MAX), @loginUsuario VARCHAR(MAX), @senha VARCHAR(MAX),
    @sexoId INT, @imagem VARBINARY(MAX), @IsAdmin BIT
)
AS
BEGIN
    INSERT INTO usuarios VALUES
    (@id, @nome, @email, @data_nascimento, @cep, @logradouro, @numero, @cidade,
     @estado, @loginUsuario, @senha, @sexoId, @imagem, @IsAdmin)
END
GO

-- Atualiza um usuário existente
CREATE PROCEDURE spUpdate_usuarios (
    @id INT, @nome VARCHAR(MAX), @email VARCHAR(MAX), @data_nascimento DATETIME,
    @cep VARCHAR(MAX), @logradouro VARCHAR(MAX), @numero INT, @cidade VARCHAR(MAX),
    @estado VARCHAR(MAX), @loginUsuario VARCHAR(MAX), @senha VARCHAR(MAX),
    @sexoId INT, @imagem VARBINARY(MAX), @IsAdmin BIT
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
        imagem = @imagem,
        IsAdmin = @IsAdmin
    WHERE id = @id
END
GO

-- Consulta avançada de usuários
CREATE PROCEDURE spConsultaAvancadaUsuarios
( 
    @nome varchar(max), 
    @estado varchar(max),
    @sexoId int,
    @dataInicial datetime, 
    @dataFinal datetime,
    @login varchar(max)
) 
AS 
BEGIN 
    DECLARE @categIni INT 
    DECLARE @categFim INT 
    SET @categIni = CASE @sexoId WHEN 0 THEN 0 ELSE @sexoId END
    SET @categFim = CASE @sexoId WHEN 0 THEN 999999 ELSE @sexoId END 
    SELECT usuarios.*, sexos.nome AS 'NomeSexo' 
    FROM usuarios 
    INNER JOIN sexos ON usuarios.sexoId = sexos.Id 
    WHERE usuarios.nome LIKE '%' + @nome + '%' AND
    usuarios.loginUsuario LIKE '%' + @login + '%' AND
    usuarios.estado LIKE '%' + @estado + '%' AND
    usuarios.data_nascimento BETWEEN @dataInicial AND @dataFinal AND 
    usuarios.sexoId BETWEEN @categIni AND @categFim; 
END
GO

-- Para sensores

-- Verifica se já existe um sensor com o mesmo nome
CREATE PROCEDURE sp_verificar_sensor (
    @nomeSensor VARCHAR(100)
)
AS
BEGIN
    SELECT COUNT(*) AS cont
    FROM sensor
    WHERE nomeSensor = @nomeSensor;
END
GO

-- Insere um novo sensor
CREATE PROCEDURE spInsert_sensor (
    @id INT,
    @nomeSensor VARCHAR(100),
    @descricaoSensor VARCHAR(255),
    @localInstalacao VARCHAR(100),
    @valorInstalacao DECIMAL(10,2),
    @dataInstalacao DATETIME
)
AS
BEGIN
    INSERT INTO sensor (
        id,
        nomeSensor,
        descricaoSensor,
        localInstalacao,
        valorInstalacao,
        dataInstalacao
    )
    VALUES (
        @id,
        @nomeSensor,
        @descricaoSensor,
        @localInstalacao,
        @valorInstalacao,
        @dataInstalacao
    );
END
GO

-- Atualiza um sensor existente
CREATE PROCEDURE spUpdate_sensor (
    @id INT,
    @nomeSensor VARCHAR(100),
    @descricaoSensor VARCHAR(255),
    @localInstalacao VARCHAR(100),
    @valorInstalacao DECIMAL(10,2),
    @dataInstalacao DATETIME
)
AS
BEGIN
    UPDATE sensor
    SET
        nomeSensor = @nomeSensor,
        descricaoSensor = @descricaoSensor,
        localInstalacao = @localInstalacao,
        valorInstalacao = @valorInstalacao,
        dataInstalacao = @dataInstalacao
    WHERE id = @id;
END
GO

-- Consulta avançada de sensor
CREATE PROCEDURE spConsultaAvancadaSensores
    @local VARCHAR(100) = NULL,
    @valorMin DECIMAL(10,2) = NULL,
    @valorMax DECIMAL(10,2) = NULL,
    @dataInicial DATETIME = NULL,
    @dataFinal DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM sensor
    WHERE (@local IS NULL OR localInstalacao LIKE '%' + @local + '%')
      AND (@valorMin IS NULL OR valorInstalacao >= @valorMin)
      AND (@valorMax IS NULL OR valorInstalacao <= @valorMax)
      AND (@dataInicial IS NULL OR dataInstalacao >= @dataInicial)
      AND (@dataFinal IS NULL OR dataInstalacao <= @dataFinal)
END
GO

-- Para temperatura

-- Verifica se já existe um registro de temperatura
CREATE PROCEDURE spExisteRegistro
    @SensorId NVARCHAR(100),
    @RecvTime DATETIME,
    @Temperature FLOAT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1) AS RegistroExiste
    FROM Temperaturas
    WHERE SensorId = @SensorId
      AND RecvTime = @RecvTime
      AND Temperature = @Temperature;
END
GO

-- Insere um novo registro de temperatura
CREATE PROCEDURE spInserirTemperatura
    @SensorId NVARCHAR(100),
    @RecvTime DATETIME,
    @Temperature FLOAT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Temperaturas (SensorId, RecvTime, Temperature)
    VALUES (@SensorId, @RecvTime, @Temperature);
END
GO

-- Lista todos os registros de temperatura
CREATE PROCEDURE spListarTemperaturas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SensorId, RecvTime, Temperature
    FROM Temperaturas
    ORDER BY RecvTime ASC;
END
GO

-- 5. Chamados

-- Consulta chamado por ID
CREATE PROCEDURE spConsultaChamadoPorId
    @id INT
AS
BEGIN
    SELECT * FROM chamados WHERE id = @id;
END
GO

-- Consulta os chamados
CREATE PROCEDURE spConsultaChamados
    @usuario_id INT,
    @is_admin BIT
AS
BEGIN
    IF @is_admin = 1
    BEGIN
        SELECT *
        FROM chamados
        ORDER BY data_abertura DESC;
    END
    ELSE
    BEGIN
        SELECT *
        FROM chamados
        WHERE usuario_id = @usuario_id
        ORDER BY data_abertura DESC;
    END
END
GO

-- Deleta Chamado
CREATE PROCEDURE spDeleteChamados
    @id INT
AS
BEGIN
    DELETE FROM chamados WHERE id = @id;
END
GO

-- Insere novo chamado
CREATE PROCEDURE spInsertChamados
    @titulo VARCHAR(255),
    @descricao TEXT,
    @status VARCHAR(50),
    @data_abertura DATETIME,
    @usuario_id INT,
    @resposta TEXT = NULL
AS
BEGIN
    INSERT INTO chamados (titulo, descricao, status, data_abertura, usuario_id, resposta)
    VALUES (@titulo, @descricao, @status, @data_abertura, @usuario_id, @resposta);
END
GO

-- Lista todos os chamados
CREATE PROCEDURE spListagemChamados
AS
BEGIN
    SELECT * FROM chamados ORDER BY data_abertura DESC;
END
GO

-- Responder Chamado
CREATE PROCEDURE spResponderChamado
    @id INT,
    @resposta TEXT,
    @status VARCHAR(50)
AS
BEGIN
    UPDATE chamados
    SET resposta = @resposta,
        status = @status
    WHERE id = @id;
END
GO

-- Atualiza chamado
CREATE PROCEDURE spUpdateChamados
    @id INT,
    @titulo VARCHAR(255),
    @descricao TEXT,
    @status VARCHAR(50),
    @data_abertura DATETIME,
    @usuario_id INT,
    @resposta TEXT = NULL
AS
BEGIN
    UPDATE chamados
    SET titulo = @titulo,
        descricao = @descricao,
        status = @status,
        data_abertura = @data_abertura,
        usuario_id = @usuario_id,
        resposta = @resposta
    WHERE id = @id;
END
GO
