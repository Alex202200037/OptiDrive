# OptiDrive - Documento de Gestão de Erros

| Campo | Valor |
| --- | --- |
| Projeto | OptiDrive |
| Data | 30/06/2026 |
| Estado | Concluído |

## 1. Objetivo

Este documento define como os erros, bugs e riscos técnicos foram identificados, classificados, corrigidos e validados durante o projeto OptiDrive.

## 2. Classificação de Erros

| Severidade | Descrição | Exemplo |
| --- | --- | --- |
| Crítica | Impede login, planeamento ou demonstração | Falha de autenticação/OAuth |
| Alta | Afeta fluxo principal mas tem workaround | Rota sem desenho visual no mapa |
| Média | Afeta cálculo, UI ou dados parciais | Combustível final estimado incorreto |
| Baixa | Problema visual/textual | Tradução incompleta ou texto desalinhado |

## 3. Fluxo de Gestão

| Etapa | Descrição |
| --- | --- |
| Identificação | Erro detetado durante testes, utilização ou revisão |
| Registo | Criação/atualização de issue no Jira |
| Priorização | Classificação por impacto na entrega |
| Correção | Implementação técnica ou ajuste documental |
| Validação | Build, teste manual e revisão do fluxo afetado |
| Fecho | Issue colocada em Done e refletida na documentação |

## 4. Erros Corrigidos na Reta Final

| Área | Problema | Correção |
| --- | --- | --- |
| Autenticação | OAuth/2FA com falhas de redirecionamento e validação | Revisão de fluxos, fallback e mensagens |
| Garagem | Marca/modelo e níveis energéticos inconsistentes | Normalização de dados e ficha dedicada |
| Planeamento | Autonomia de chegada e reforços incompletos | Reserva mínima, consumo por velocidade e paragens |
| Mapa | Excesso/falta de postos relevantes | Filtro por rota, combustível e energia |
| Social | Exposição técnica de emails/fornecedores | Perfis sociais por nome/foto e presença |
| Documentação | Datas e Jira desalinhados | Calendário real 11/05 a 30/06 e bugfix final |

## 5. Evidência Jira

A issue `OP-56 - Corrigir bugs finais e validar entrega em 30/06` representa o fecho de bugfix e validação final antes da entrega.

## 6. Conclusão

A gestão de erros foi integrada no ciclo de entrega, garantindo que os problemas críticos foram corrigidos antes da apresentação e que os riscos residuais ficaram documentados.
