# VetCare — plataforma de cuidado veterinário

Aplicação MVC para gestão da clínica e acompanhamento dos pacientes pelos seus tutores. Desenvolvida com ASP.NET Core MVC (.NET 8), C#, Entity Framework Core e MySQL.

## O que esta versão acrescenta

- Interface responsiva mais acolhedora, com linguagem visual leve, azul, menta e cartões adaptados para celular.
- Cadastro público de clientes/tutores, login e área **Meu espaço**.
- Portal do tutor com os pets associados à sua conta, próximos agendamentos, histórico recente de atendimentos e solicitações de vínculo.
- Relação muitos-para-muitos entre tutores e animais: um tutor pode acompanhar vários pets e um pet pode ter vários tutores, sem duplicar a ficha clínica.
- Código de vínculo por animal e fluxo de solicitação com aprovação pela equipe. O cliente não recebe acesso à ficha de outra família apenas por saber o nome do pet.
- Proteção de rotas por perfil: `Cliente` acessa o portal; `Administrador`, `Veterinario` e `Recepcionista` acessam a área operacional conforme as permissões dos controllers.
- Validação de identificação/microchip repetida no cadastro de animais.
- Padronização da propriedade `CRMV`.

## Tecnologias e requisitos

- .NET SDK 8.0
- MySQL 8 ou compatível
- Visual Studio 2022 com a carga de trabalho **Desenvolvimento ASP.NET e Web**
- Conexão com a internet para Bootstrap, fontes e validação JavaScript via CDN

## Importante se você já tem o banco de dados criado

Esta versão adiciona colunas e tabelas; `EnsureCreated()` não atualiza tabelas existentes. **Faça backup do banco antes de continuar.**

1. Abra `VetCare/appsettings.json` e confira `Database=vetcare_db`, usuário e senha. A senha foi substituída por um marcador neste pacote; preencha a sua localmente.
2. No MySQL Workbench, selecione o banco que a aplicação realmente usa.
3. Abra `VetCare/Database/upgrade_v1_vinculos.sql`. O script está direcionado ao banco `vetcare_db`; se o seu banco tiver outro nome, ajuste a linha `USE vetcare_db;` para o nome correto.
4. Execute o script **uma única vez** no banco existente. Ele adiciona o vínculo da conta de cliente, cria códigos para os animais já cadastrados, cria as tabelas de relacionamento e preserva a ficha dos pets existentes.
5. Se o MySQL informar que uma coluna/tabela já existe, pare e confira o estado do banco antes de tentar executar de novo. Não execute o script de atualização repetidamente.
6. Não rode `Database/vetcare_db.sql` sobre a base existente. Esse é o script completo para uma instalação nova.

Para uma instalação nova, execute `Database/vetcare_db.sql` no MySQL Workbench; depois configure a string de conexão e inicie a aplicação.

## Configurar conexão

Em `VetCare/appsettings.json`, ajuste a string para as credenciais locais:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=vetcare_db;Uid=root;Pwd=SUA_SENHA;"
  }
}
```

Não publique senhas reais no GitHub. A aplicação também aceita a chave `LocalConnection` por compatibilidade, mas `DefaultConnection` é a recomendada.

## Executar pelo Visual Studio

1. Extraia o ZIP para uma pasta nova. Não misture os arquivos com a cópia antiga.
2. Abra `VetCare.sln`.
3. Aguarde a restauração dos pacotes NuGet.
4. Confira a conexão com MySQL.
5. Use **Compilação → Recompilar Solução**.
6. Execute com `Ctrl+F5` ou `F5`.

A primeira execução mantém o seed dos perfis de demonstração se a tabela de usuários estiver vazia.

## Contas iniciais

| Perfil | E-mail | Senha inicial |
|---|---|---|
| Administrador | `admin@vetcare.local` | `VetCare@123` |
| Veterinário | `vet@vetcare.local` | `VetCare@123` |

O tutor cria sua própria conta em **Criar conta**. Se o tutor já existir na base da clínica, ele pode usar o mesmo CPF e e-mail para associar a conta ao cadastro existente, evitando duplicidade. Credenciais demonstrativas devem ser alteradas antes de publicar a aplicação.

## Fluxo de vínculo familiar

1. A equipe abre o perfil do animal e fornece o **código de vínculo** à pessoa autorizada.
2. O familiar cria uma conta com seus próprios dados.
3. No portal, informa o código e envia a solicitação.
4. A equipe abre **Vínculos** e aprova ou recusa.
5. Após a aprovação, o animal aparece no portal do familiar com a mesma ficha clínica, histórico e agendamentos. Nenhum novo animal é criado.

A aprovação pela equipe é intencional: compartilhar o mesmo animal deve compartilhar o histórico de saúde, então o acesso precisa ser autorizado.

## Roteiro de teste manual

- [ ] Entre como administrador e confira o painel operacional.
- [ ] Crie um animal e confirme que ele recebeu um código de vínculo.
- [ ] Cadastre uma conta de cliente com CPF e e-mail inéditos.
- [ ] Entre no portal do cliente e envie uma solicitação usando o código do animal.
- [ ] Saia e entre como administrador/veterinário; abra **Vínculos** e aprove a solicitação.
- [ ] Entre novamente como cliente e confirme que o mesmo animal aparece no portal.
- [ ] Crie outra conta de cliente com outro CPF/e-mail e solicite o mesmo código.
- [ ] Aprove a segunda solicitação e confirme que o animal continua com o mesmo ID e uma única ficha.
- [ ] Confira se o cliente não consegue abrir as rotas administrativas.
- [ ] Confira cadastros, agendamentos, atendimentos e relatório clínico existentes.

## Estrutura

- `Controllers/` — ações MVC e controle de acesso
- `Models/` — entidades de domínio e relacionamentos
- `ViewModels/` — modelos de formulários e do portal
- `Data/` — contexto EF Core, seed e serviço de fotos
- `Views/` — páginas Razor
- `wwwroot/css/site.css` — identidade visual responsiva
- `Database/vetcare_db.sql` — esquema completo para base nova
- `Database/upgrade_v1_vinculos.sql` — atualização de uma base anterior
- `Program.cs` — inicialização e configuração dos serviços

## Limitação de validação

O pacote foi revisado estruturalmente, mas a compilação e os fluxos de MySQL precisam ser executados no seu ambiente com o SDK .NET e o banco configurado. A checklist acima é o roteiro de validação após abrir a solução.
