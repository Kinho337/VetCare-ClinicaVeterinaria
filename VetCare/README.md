# VetCare — Sistema de Gestão Veterinária

Sistema web acadêmico de gestão de clínica veterinária, desenvolvido em **ASP.NET Core MVC (.NET 8), C#, Entity Framework Core e MySQL**. A interface utiliza identidade visual azul royal e branco, layout responsivo e componentes Razor.

## Módulos

- **Autenticação e autorização:** login com cookie e perfis Administrador, Veterinário e Recepcionista.
- **Tutores:** cadastro, edição, consulta, foto de perfil e validação de CPF/e-mail duplicados.
- **Animais:** cadastro associado a tutor, espécie, raça, sexo, identificação, nascimento e foto; histórico clínico por paciente.
- **Veterinários:** cadastro profissional, CRMV, contato e foto; prevenção de e-mail/CRMV duplicados.
- **Serviços:** catálogo de procedimentos e preços.
- **Agendamentos:** associação de animal, profissional e serviço, com validação de horário ocupado.
- **Atendimentos:** queixa, diagnóstico, observações, medicações recomendadas e foto/anexo.
- **Relatório clínico:** dados do animal, tutor, veterinário e atendimento, com impressão ou opção de salvar como PDF pelo navegador.
- **Experiência de uso:** painel inicial, buscas, mensagens de retorno, formulários com validação e recorte de fotos no navegador.

## Requisitos

- Windows 10/11 ou sistema compatível com .NET 8
- .NET SDK 8.0
- MySQL 8.0 ou compatível (servidor local, XAMPP/WAMP ou serviço MySQL)
- Visual Studio 2022 com a carga de trabalho **Desenvolvimento ASP.NET e Web**, ou VS Code com extensão C#
- Navegador moderno e conexão com internet para Bootstrap via CDN

## Abrir no Visual Studio

1. Extraia o ZIP para uma pasta sem acentos ou caracteres especiais, se possível.
2. Abra `VetCare.sln` (ou, se preferir, `VetCare/VetCare.csproj`) no Visual Studio 2022.
3. Aguarde a restauração dos pacotes NuGet.
4. Abra `VetCare/appsettings.json` e configure a conexão MySQL (exemplo abaixo).
5. Confirme que o serviço MySQL está iniciado e que o banco está criado.
6. No menu **Compilação**, selecione **Recompilar Solução**. Corrija qualquer erro antes de executar.
7. Pressione `F5` ou `Ctrl+F5`. O Visual Studio abrirá a URL local exibida no navegador/saída.

## Configurar o MySQL

O arquivo `Database/vetcare_db.sql` contém o script completo de criação do esquema. Execute-o no MySQL Workbench **antes da primeira execução**, usando uma conta com permissão para criar banco e tabelas. O script cria o banco `vetcare_db`.

Depois, ajuste `VetCare/appsettings.json`. Exemplo para MySQL local com usuário `root`:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=vetcare_db;user=root;password=SUA_SENHA;"
}
```

Substitua `SUA_SENHA` pela senha real do seu MySQL. Não compartilhe nem publique credenciais reais no GitHub. A aplicação também reconhece a chave `LocalConnection` por compatibilidade, mas `DefaultConnection` é o nome recomendado.

> Se você já executou o script SQL, não precisa criar o banco de novo. Confirme apenas que o nome do banco na string é o mesmo que foi criado. O usuário MySQL precisa ter permissão de leitura, gravação e alteração de esquema para a inicialização acadêmica.

## Executar pelo terminal

Abra um terminal na pasta `VetCare` — a mesma que contém `VetCare.csproj` — e execute:

```powershell
dotnet --info
dotnet restore
dotnet build
dotnet run
```

Use a URL `http://localhost:...` ou `https://localhost:...` que aparecer no terminal. Se `dotnet --info` não funcionar, instale o .NET 8 SDK e reabra o terminal.

## Contas iniciais de demonstração

Na primeira inicialização, o sistema cria as contas de demonstração se a tabela de usuários estiver vazia:

| Perfil | Login | Senha inicial |
|---|---|---|
| Administrador | `admin@vetcare.local` | `VetCare@123` |
| Veterinário | `vet@vetcare.local` | `VetCare@123` |

O `DbSeeder` também cadastra serviços de exemplo e uma veterinária demonstrativa. Essas credenciais são apenas para demonstração local; altere-as antes de qualquer implantação compartilhada.

## Roteiro rápido de demonstração

1. Entre com a conta de veterinário.
2. Cadastre um tutor e tente repetir CPF/e-mail para demonstrar a validação.
3. Abra o tutor e cadastre um ou mais animais com fotos.
4. Confira o cadastro de veterinários e os serviços disponíveis.
5. Crie um agendamento para um animal, serviço e profissional.
6. Registre o atendimento com queixa, diagnóstico, plano/observações e medicações recomendadas.
7. Abra o relatório clínico e use a impressão do navegador para gerar PDF.

## Estrutura do projeto

- `Controllers/` — ações e fluxo HTTP MVC
- `Models/` — entidades de domínio
- `ViewModels/` — modelos específicos de formulários/telas
- `Data/` — contexto EF Core, carga inicial e serviço de fotos
- `Views/` — páginas Razor
- `wwwroot/css/` e `wwwroot/js/` — estilos e scripts do cliente
- `Database/vetcare_db.sql` — esquema MySQL
- `Program.cs` — ponto de entrada e configuração dos serviços

## Notas técnicas e limitações

- A aplicação usa `EnsureCreated` para facilitar a inicialização acadêmica sem migrations. Mudanças futuras no modelo não atualizam automaticamente um banco já criado; em desenvolvimento, use um banco recriável ou evolua o projeto para migrations EF Core.
- A verificação de conflito de agenda deve ser expandida para considerar duração e sobreposição de intervalos antes de uso real.
- Antes de produção, adicione gestão de contas e redefinição de senha, auditoria, backups, limites/validação de uploads, políticas de privacidade e revisão de LGPD e segurança.
- Este projeto é uma base acadêmica e demonstrativa, não substitui um sistema clínico validado para uso profissional.

## Git e histórico de desenvolvimento

O ZIP contém o código-fonte, não um histórico Git preexistente. Para iniciar um repositório, use `git init` e registre commits reais por etapa. Não simule datas nem histórico anterior. Sugestões de etapas: estrutura MVC; persistência MySQL; autenticação; tutores; animais; upload de fotos; agenda; prontuário; refinamento visual; documentação.
