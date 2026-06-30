# OptiDrive - Ata Sprint 2

## Cabeçalho da Ata

| Campo | Valor |
| --- | --- |
| Documento | Ata de Reunião |
| Número | ATA-OP-02 |
| Projeto | OptiDrive |
| Sprint | Sprint 2 - Mapas, Postos e Rotas |
| Data | 30/05/2026 |
| Local | Reunião remota / trabalho local |
| Hora de início | 10:00 |
| Hora de fim | 11:15 |
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
| P-04 | Melhorar ficha de veículo e dados técnicos | Resolvido |
| P-05 | Pesquisar APIs de postos e carregadores | Resolvido |
| P-06 | Confirmar integração de mapas | Resolvido |

## Assuntos Tratados

| ID | Assunto | Discussão |
| --- | --- | --- |
| AT-06 | Google Maps | Foi decidido usar Google Maps para mapa, geocoding e direções quando a API estiver configurada |
| AT-07 | Postos de combustível | A aplicação deve apresentar preços, combustível e marca, evitando sobrecarregar o mapa |
| AT-08 | Carregadores elétricos | Foi integrada OpenChargeMap para EVs |
| AT-09 | Pontos intermédios | Ficou decidido que pontos intermédios devem ser opcionais |
| AT-10 | Evitar portagens | A opção deve alterar efetivamente o pedido de rota |
| AT-11 | Postos junto à rota | O mapa deve mostrar prioritariamente postos próximos da rota selecionada |

## Decisões

| ID | Decisão | Impacto |
| --- | --- | --- |
| D-05 | Filtrar postos por proximidade à rota | Melhora performance e legibilidade do mapa |
| D-06 | Filtrar postos por combustível do veículo | Torna o planeamento coerente com a garagem |
| D-07 | Adicionar filtro por marca | Permite pesquisar Galp, Repsol, Shell, BP e outras |
| D-08 | Manter fallback visual se Google Maps falhar | Evita bloqueio total da experiência em ambiente local |

## Ações Definidas

| ID | Ação | Responsável | Prazo | Estado |
| --- | --- | --- | --- | --- |
| A-06 | Integrar Google Directions e Geocoding | Alexandre Miguel | 30/05/2026 | Concluída |
| A-07 | Integrar postos de combustível | Alexandre Miguel | 30/05/2026 | Concluída |
| A-08 | Integrar OpenChargeMap | Alexandre Miguel | 30/05/2026 | Concluída |
| A-09 | Implementar filtros por combustível e marca | Alexandre Miguel | 30/05/2026 | Concluída |
| A-10 | Atualizar desenho detalhado Sprint 2 | Alexandre Miguel | 30/05/2026 | Concluída |

## Assuntos a Tratar na Próxima Reunião

| ID | Assunto | Objetivo |
| --- | --- | --- |
| PR-04 | Smart Save | Validar consumo, autonomia, reserva de chegada e reforços |
| PR-05 | Aplicação da rota ao veículo | Garantir atualização de combustível/carga e histórico |
| PR-06 | Postos de reforço | Integrar abastecimentos/carregamentos quando autonomia é insuficiente |

## Data da Próxima Reunião

| Campo | Valor |
| --- | --- |
| Data prevista | 01/06/2026 |
| Foco | Sprint 3 - Smart Save, autonomia e histórico operacional |

## Resultado da Reunião

A Sprint 2 ficou validada como a sprint que transforma a garagem numa ferramenta de planeamento real. O produto passou a calcular rotas, mostrar mapa, postos e carregadores, e preparar o terreno para Smart Save.
