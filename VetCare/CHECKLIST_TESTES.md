# Checklist — VetCare com portal de tutores

## Preparação
- [ ] Fazer backup do banco.
- [ ] Configurar a string MySQL em `appsettings.json`.
- [ ] Executar `Database/upgrade_v1_vinculos.sql` uma única vez no banco existente.
- [ ] Recompilar a solução no Visual Studio.

## Conta de cliente
- [ ] Cadastro com CPF e e-mail novos.
- [ ] Bloqueio de e-mail já usado por outra conta.
- [ ] Associação a um cadastro de tutor já existente com o mesmo CPF e e-mail.
- [ ] Login leva ao portal do cliente.
- [ ] Cliente não acessa o painel operacional nem rotas de equipe.

## Um animal, vários tutores
- [ ] Animal existente recebe código de vínculo.
- [ ] Cliente solicita vínculo pelo código.
- [ ] Solicitação duplicada pendente é bloqueada.
- [ ] Funcionário aprova o pedido.
- [ ] Animal aparece no portal da segunda conta.
- [ ] A ficha clínica permanece com o mesmo ID e sem duplicação.
- [ ] Histórico e agendamentos ficam visíveis apenas a contas vinculadas.
- [ ] Solicitação pode ser recusada.

## Regressão da clínica
- [ ] Criar/editar tutor.
- [ ] Criar/editar animal e fotos.
- [ ] Criar agendamento e alterar status.
- [ ] Registrar atendimento e abrir relatório.
- [ ] Verificar cadastro e unicidade do CRMV.
