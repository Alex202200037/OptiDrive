# GitHub Actions - validação remota de 07/09/2026

## Integração contínua

- Workflow: `OptiDrive CI`
- Execução aprovada: `34164627808`
- Commit: `0459b4ab53865dc9154d4a475696d32137e2b68b`
- URL: https://github.com/Alex202200037/OptiDrive/actions/runs/34164627808
- Resultado: `success`
- Etapas aprovadas: checkout, .NET 8, restore, build, formatação, auditoria NuGet, 16 testes, upload TRX, publicação, upload do artefacto web e build Docker.

## Correção observada durante a execução

A primeira execução (`34164480946`) revelou que o runner Linux partilhado necessitava de 18,886 s para criar os 500 utilizadores e 500 veículos, contra 8,518 s no ambiente local. O limite local de 10 s foi preservado e o limite de segurança do CI foi ajustado para 30 s, evitando que diferenças de capacidade do runner fossem confundidas com uma regressão funcional. A execução seguinte aprovou os 16 testes.

## Entrega Azure

- Workflow: `OptiDrive Azure Deploy`
- Execução: `34164714698`
- URL: https://github.com/Alex202200037/OptiDrive/actions/runs/34164714698
- Restore e publicação: aprovados.
- Deploy: condicionado; o repositório não possui credenciais Azure configuradas.

Não foram adicionadas credenciais ao código ou ao histórico Git. O bloqueio é externo e deve ser resolvido com `AZURE_WEBAPP_NAME` e `AZURE_WEBAPP_PUBLISH_PROFILE` num ambiente Azure ativo.
