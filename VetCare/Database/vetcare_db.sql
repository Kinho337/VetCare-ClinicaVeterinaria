CREATE DATABASE IF NOT EXISTS vetcare_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE vetcare_db;

CREATE TABLE IF NOT EXISTS Usuarios (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(120) NOT NULL,
  Email VARCHAR(180) NOT NULL,
  PasswordHash VARCHAR(500) NOT NULL,
  Perfil VARCHAR(30) NOT NULL DEFAULT 'Veterinario',
  FotoPerfil VARCHAR(500) NULL,
  CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY UX_Usuarios_Email (Email)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Tutores (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(120) NOT NULL,
  CPF VARCHAR(14) NOT NULL,
  Email VARCHAR(180) NOT NULL,
  Telefone VARCHAR(30) NOT NULL,
  Endereco VARCHAR(180) NULL,
  FotoPerfil VARCHAR(500) NULL,
  CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY UX_Tutores_CPF (CPF),
  UNIQUE KEY UX_Tutores_Email (Email)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Veterinarios (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(120) NOT NULL,
  Email VARCHAR(180) NOT NULL,
  CRmv VARCHAR(30) NOT NULL,
  Telefone VARCHAR(30) NOT NULL DEFAULT '',
  FotoPerfil VARCHAR(500) NULL,
  UNIQUE KEY UX_Veterinarios_Email (Email),
  UNIQUE KEY UX_Veterinarios_CRMV (CRmv)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Servicos (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(100) NOT NULL,
  Descricao VARCHAR(500) NULL,
  Preco DECIMAL(10,2) NOT NULL DEFAULT 0,
  Ativo TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Animais (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nome VARCHAR(100) NOT NULL,
  Especie VARCHAR(50) NOT NULL,
  Raca VARCHAR(100) NULL,
  Sexo VARCHAR(30) NULL,
  DataNascimento DATETIME NULL,
  Identificacao VARCHAR(80) NULL,
  FotoPerfil VARCHAR(500) NULL,
  TutorId INT NOT NULL,
  CONSTRAINT FK_Animais_Tutores FOREIGN KEY (TutorId) REFERENCES Tutores(Id) ON DELETE RESTRICT,
  INDEX IX_Animais_TutorId (TutorId)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Agendamentos (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DataHora DATETIME NOT NULL,
  Status VARCHAR(30) NOT NULL DEFAULT 'Agendado',
  Observacoes VARCHAR(500) NULL,
  AnimalId INT NOT NULL,
  VeterinarioId INT NOT NULL,
  ServicoId INT NOT NULL,
  CONSTRAINT FK_Agendamentos_Animais FOREIGN KEY (AnimalId) REFERENCES Animais(Id) ON DELETE RESTRICT,
  CONSTRAINT FK_Agendamentos_Veterinarios FOREIGN KEY (VeterinarioId) REFERENCES Veterinarios(Id) ON DELETE RESTRICT,
  CONSTRAINT FK_Agendamentos_Servicos FOREIGN KEY (ServicoId) REFERENCES Servicos(Id) ON DELETE RESTRICT,
  INDEX IX_Agendamentos_DataHora (DataHora),
  INDEX IX_Agendamentos_Veterinario_Data (VeterinarioId, DataHora)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Atendimentos (
  Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DataHora DATETIME NOT NULL,
  Queixa VARCHAR(2000) NOT NULL,
  Diagnostico VARCHAR(4000) NOT NULL,
  Observacoes VARCHAR(4000) NULL,
  MedicacoesRecomendadas VARCHAR(4000) NULL,
  FotoAtendimento VARCHAR(500) NULL,
  AnimalId INT NOT NULL,
  VeterinarioId INT NOT NULL,
  AgendamentoId INT NULL,
  CONSTRAINT FK_Atendimentos_Animais FOREIGN KEY (AnimalId) REFERENCES Animais(Id) ON DELETE RESTRICT,
  CONSTRAINT FK_Atendimentos_Veterinarios FOREIGN KEY (VeterinarioId) REFERENCES Veterinarios(Id) ON DELETE RESTRICT,
  CONSTRAINT FK_Atendimentos_Agendamentos FOREIGN KEY (AgendamentoId) REFERENCES Agendamentos(Id) ON DELETE SET NULL,
  UNIQUE KEY UX_Atendimentos_AgendamentoId (AgendamentoId),
  INDEX IX_Atendimentos_Animal_Data (AnimalId, DataHora)
) ENGINE=InnoDB;

-- Os usuários de demonstração são criados pelo DbSeeder, com hash de senha via PasswordHasher.
-- Não insira senhas em texto puro nesta tabela.
