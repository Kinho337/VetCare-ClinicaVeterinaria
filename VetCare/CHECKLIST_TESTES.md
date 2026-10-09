# Checklist manual antes da gravação

- [ ] Ajustar conexão MySQL em `appsettings.json`.
- [ ] Executar `dotnet restore` e `dotnet run`.
- [ ] Login de administrador e veterinário.
- [ ] Acesso sem login redireciona para Login.
- [ ] Cadastrar tutor com CPF e e-mail válidos.
- [ ] Repetir CPF e confirmar mensagem de duplicidade.
- [ ] Repetir e-mail e confirmar mensagem de duplicidade.
- [ ] Cadastrar dois animais para o mesmo tutor.
- [ ] Testar foto de tutor, animal e veterinário.
- [ ] Alterar proporção da foto e conferir prévia do recorte.
- [ ] Criar agendamento futuro.
- [ ] Tentar mesmo veterinário no mesmo horário e conferir bloqueio.
- [ ] Registrar atendimento a partir de um agendamento.
- [ ] Anexar foto no atendimento.
- [ ] Conferir tutor, animal, veterinário e diagnóstico no relatório.
- [ ] Imprimir ou salvar o relatório em PDF.
- [ ] Não enviar `appsettings.json` com senha real ao GitHub.
