# OptiDrive - Ata Sprint 3

## Cabeçalho da Ata

| Campo | Valor |
| --- | --- |
| Documento | Ata de Reunião |
| Número | ATA-OP-03 |
| Projeto | OptiDrive |
| Sprint | Sprint 3 - Smart Save e Histórico Operacional |
| Data | 08/06/2026 |
| Local | Reunião remota / trabalho local |
| Hora de início | 10:00 |
| Hora de fim | 11:30 |
| Versão | 1.0 |
| Estado | Fechada |

## Participantes

| Papel | Participante | Presença |
| --- | --- | --- |
| Product Owner / Developer | Alexandre Miguel | Presente |
| Stakeholder académico | Docente | A validar em contexto de aula |

## Pendentes da Reunião Anterior

| ID | Assunto | Estado |
| --- | --- | --- |
| P-07 | Garantir que rota aparece visualmente no mapa | Resolvido |
| P-08 | Incluir carregadores EV no Smart Save | Resolvido |
| P-09 | Corrigir pontos intermédios opcionais | Resolvido |
| P-10 | Melhorar filtros por combustível e marca | Resolvido |

## Assuntos Tratados

| ID | Assunto | Discussão |
| --- | --- | --- |
| AT-12 | Smart Save | O sistema deve calcular reforços, autonomia e chegada com reserva, nunca simplesmente 0% |
| AT-13 | Consumo por velocidade | A velocidade média deve influenciar consumo estimado |
| AT-14 | Autonomia EV | Carregadores elétricos devem entrar nas recomendações quando o veículo é elétrico |
| AT-15 | Aplicar rota ao veículo | A rota guardada deve reduzir combustível/carga e gerar histórico |
| AT-16 | Abastecimentos/cargas | O utilizador deve conseguir registar reforços manuais ou enchimento total |
| AT-17 | Postos na rota | Devem aparecer preferencialmente postos próximos da rota para evitar sobrecarga visual |

## Decisões

| ID | Decisão | Impacto |
| --- | --- | --- |
| D-09 | Aplicar viagens ao carro selecionado | Fecha ciclo rota -> garagem -> histórico |
| D-10 | Calcular margem mínima de chegada | Evita planos irrealistas com chegada a 0% |
| D-11 | Considerar velocidade média no consumo | Torna a previsão mais realista |
| D-12 | Integrar reforços a meio da viagem | Ajuda combustão e EV em viagens longas |

## Ações Definidas

| ID | Ação | Responsável | Prazo | Estado |
| --- | --- | --- | --- | --- |
| A-11 | Implementar Smart Save com reserva de chegada | Alexandre Miguel | 08/06/2026 | Concluída |
| A-12 | Associar EVs e carregadores ao planeamento | Alexandre Miguel | 08/06/2026 | Concluída |
| A-13 | Aplicar rota ao veículo e gerar histórico | Alexandre Miguel | 08/06/2026 | Concluída |
| A-14 | Registar abastecimentos/cargas | Alexandre Miguel | 08/06/2026 | Concluída |
| A-15 | Atualizar desenho detalhado Sprint 3 | Alexandre Miguel | 08/06/2026 | Concluída |

## Assuntos a Tratar na Próxima Reunião

| ID | Assunto | Objetivo |
| --- | --- | --- |
| PR-07 | Social | Implementar perfis, contactos, mensagens e viagens colaborativas |
| PR-08 | OAuth | Consolidar Google, Microsoft e fallback Apple |
| PR-09 | Segurança de perfil | Garantir que MFA e providers aparecem de forma clara |

## Data da Próxima Reunião

| Campo | Valor |
| --- | --- |
| Data prevista | 09/06/2026 |
| Foco | Sprint 4 - Social, OAuth e colaboração |

## Resultado da Reunião

A Sprint 3 validou o núcleo inteligente do OptiDrive. O produto passou a estimar autonomia, sugerir reforços, calcular chegada com reserva e refletir viagens no histórico da garagem.
