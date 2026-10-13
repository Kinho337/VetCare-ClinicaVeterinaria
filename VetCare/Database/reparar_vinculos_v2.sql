-- VetCare | Reparo incremental do banco existente para vínculo familiar de pets.
-- Não apaga registros. Faça backup antes de executar.
-- Abra uma conexão com o MySQL Workbench e selecione o banco correto.
USE vetcare_db;
SET SQL_SAFE_UPDATES = 0;

DROP PROCEDURE IF EXISTS VetCare_RepararVinculosV2;
DELIMITER $$
CREATE PROCEDURE VetCare_RepararVinculosV2()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'Usuarios' AND column_name = 'TutorId'
    ) THEN
        ALTER TABLE Usuarios ADD COLUMN TutorId INT NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'Animais' AND column_name = 'CodigoVinculo'
    ) THEN
        ALTER TABLE Animais ADD COLUMN CodigoVinculo VARCHAR(24) NULL;
    END IF;

    CREATE TABLE IF NOT EXISTS AnimalTutores (
        AnimalId INT NOT NULL,
        TutorId INT NOT NULL,
        VinculadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        TipoVinculo VARCHAR(30) NOT NULL DEFAULT 'Tutor',
        PRIMARY KEY (AnimalId, TutorId),
        CONSTRAINT FK_AnimalTutores_Animais FOREIGN KEY (AnimalId) REFERENCES Animais(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AnimalTutores_Tutores FOREIGN KEY (TutorId) REFERENCES Tutores(Id) ON DELETE CASCADE,
        INDEX IX_AnimalTutores_TutorId (TutorId)
    ) ENGINE=InnoDB;

    CREATE TABLE IF NOT EXISTS SolicitacoesVinculoAnimal (
        Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
        AnimalId INT NOT NULL,
        TutorId INT NOT NULL,
        Status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
        Mensagem VARCHAR(500) NULL,
        CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        ResolvidoEm DATETIME NULL,
        ResolvidoPor VARCHAR(180) NULL,
        CONSTRAINT FK_SolicitacoesVinculoAnimal_Animais FOREIGN KEY (AnimalId) REFERENCES Animais(Id) ON DELETE CASCADE,
        CONSTRAINT FK_SolicitacoesVinculoAnimal_Tutores FOREIGN KEY (TutorId) REFERENCES Tutores(Id) ON DELETE CASCADE,
        INDEX IX_SolicitacoesVinculoAnimal_Animal_Tutor_Status (AnimalId, TutorId, Status)
    ) ENGINE=InnoDB;

    UPDATE Animais
    SET CodigoVinculo = UPPER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 16))
    WHERE CodigoVinculo IS NULL OR CodigoVinculo = '';

    ALTER TABLE Animais MODIFY COLUMN CodigoVinculo VARCHAR(24) NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.statistics
        WHERE table_schema = DATABASE() AND table_name = 'Animais' AND index_name = 'UX_Animais_CodigoVinculo'
    ) THEN
        CREATE UNIQUE INDEX UX_Animais_CodigoVinculo ON Animais (CodigoVinculo);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = DATABASE() AND table_name = 'Usuarios' AND constraint_name = 'FK_Usuarios_Tutores'
    ) THEN
        ALTER TABLE Usuarios ADD CONSTRAINT FK_Usuarios_Tutores
            FOREIGN KEY (TutorId) REFERENCES Tutores(Id) ON DELETE SET NULL;
    END IF;

    INSERT IGNORE INTO AnimalTutores (AnimalId, TutorId, TipoVinculo)
    SELECT Id, TutorId, 'Principal' FROM Animais WHERE TutorId IS NOT NULL;
END$$
DELIMITER ;

CALL VetCare_RepararVinculosV2();
DROP PROCEDURE VetCare_RepararVinculosV2;

-- Conferência final: estas tabelas devem aparecer no resultado.
SHOW TABLES LIKE 'AnimalTutores';
SHOW TABLES LIKE 'SolicitacoesVinculoAnimal';
SHOW COLUMNS FROM Animais LIKE 'CodigoVinculo';
SHOW COLUMNS FROM Usuarios LIKE 'TutorId';
