-- VetCare: atualização da base existente para suporte a contas de clientes e múltiplos tutores.
-- Faça backup do banco antes de executar. Execute uma única vez no banco usado pelo projeto.
USE vetcare_db;

ALTER TABLE Usuarios ADD COLUMN TutorId INT NULL;
ALTER TABLE Animais ADD COLUMN CodigoVinculo VARCHAR(24) NULL;

UPDATE Animais
SET CodigoVinculo = UPPER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 16))
WHERE CodigoVinculo IS NULL OR CodigoVinculo = '';

ALTER TABLE Animais MODIFY COLUMN CodigoVinculo VARCHAR(24) NOT NULL;
ALTER TABLE Animais ADD UNIQUE KEY UX_Animais_CodigoVinculo (CodigoVinculo);

ALTER TABLE Usuarios
  ADD CONSTRAINT FK_Usuarios_Tutores FOREIGN KEY (TutorId) REFERENCES Tutores(Id) ON DELETE SET NULL;

CREATE TABLE AnimalTutores (
  AnimalId INT NOT NULL,
  TutorId INT NOT NULL,
  VinculadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  TipoVinculo VARCHAR(30) NOT NULL DEFAULT 'Tutor',
  PRIMARY KEY (AnimalId, TutorId),
  CONSTRAINT FK_AnimalTutores_Animais FOREIGN KEY (AnimalId) REFERENCES Animais(Id) ON DELETE CASCADE,
  CONSTRAINT FK_AnimalTutores_Tutores FOREIGN KEY (TutorId) REFERENCES Tutores(Id) ON DELETE CASCADE,
  INDEX IX_AnimalTutores_TutorId (TutorId)
) ENGINE=InnoDB;

INSERT IGNORE INTO AnimalTutores (AnimalId, TutorId, TipoVinculo)
SELECT Id, TutorId, 'Principal' FROM Animais;

CREATE TABLE SolicitacoesVinculoAnimal (
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

-- Usuários que já existiam como tutores podem criar conta usando o mesmo CPF e e-mail.
-- O cadastro público vincula a nova conta ao Tutor existente sem duplicar o registro.
