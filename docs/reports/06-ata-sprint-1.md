# OptiDrive - Ata Sprint 1

## Cabeçalho da Ata

| Campo | Valor |
| --- | --- |
| Documento | Ata de Reunião |
| Número | ATA-OP-01 |
| Projeto | OptiDrive |
| Sprint | Sprint 1 - Identidade e Garagem |
| Data | 20/05/2026 |
| Local | Reunião remota / trabalho local |
| Hora de início | 10:00 |
| Hora de fim | 11:00 |
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
| P-01 | Confirmar stack tecnológica da aplicação | Resolvido |
| P-02 | Definir módulos nucleares do produto | Resolvido |
| P-03 | Iniciar backlog Jira com épicos principais | Resolvido |

## Assuntos Tratados

| ID | Assunto | Discussão |
| --- | --- | --- |
| AT-01 | Estrutura ASP.NET Core MVC | Validou-se a escolha por MVC/Razor por estar alinhada com a cadeira e permitir produto web completo |
| AT-02 | Identidade local | Ficou decidido implementar login local com password hashing e sessão |
| AT-03 | MFA Authenticator | Foi definido que o sistema deve suportar TOTP para reforçar realismo de produto |
| AT-04 | Garagem inicial | Confirmou-se que o veículo deve ter dados técnicos e nível atual desde a primeira sprint |
| AT-05 | Documentação | Ficou definido que cada sprint terá ata e desenho detalhado correspondente |

## Decisões

| ID | Decisão | Impacto |
| --- | --- | --- |
| D-01 | Usar ASP.NET Core MVC / .NET 8 | Mantém stack académica clara e robusta |
| D-02 | Usar SQLite como LocalDB | Permite persistência simples em MacBook e Docker |
| D-03 | Implementar Authenticator cedo | Aumenta maturidade de segurança do produto |
| D-04 | Separar garagem por ficha de veículo | Evita UI confusa e facilita histórico por carro |

## Ações Definidas

| ID | Ação | Responsável | Prazo | Estado |
| --- | --- | --- | --- | --- |
| A-01 | Criar solução MVC e navegação base | Alexandre Miguel | 20/05/2026 | Concluída |
| A-02 | Implementar registo e login local | Alexandre Miguel | 20/05/2026 | Concluída |
| A-03 | Implementar MFA TOTP e recovery codes | Alexandre Miguel | 20/05/2026 | Concluída |
| A-04 | Criar primeira versão da garagem | Alexandre Miguel | 20/05/2026 | Concluída |
| A-05 | Atualizar documentação da Sprint 1 | Alexandre Miguel | 20/05/2026 | Concluída |

## Assuntos a Tratar na Próxima Reunião

| ID | Assunto | Objetivo |
| --- | --- | --- |
| PR-01 | Integração Google Maps | Validar cálculo de rota, geocoding e fallback |
| PR-02 | Postos e carregadores | Identificar fontes de dados e filtros por energia |
| PR-03 | Planeamento de viagem | Garantir que origem, destino e pontos intermédios são usáveis |

## Data da Próxima Reunião

| Campo | Valor |
| --- | --- |
| Data prevista | 21/05/2026 |
| Foco | Sprint 2 - Mapas, postos e rotas |

## Resultado da Reunião

A Sprint 1 ficou validada como base funcional do produto. A aplicação passou a ter estrutura web, identidade, segurança e garagem inicial, permitindo avançar para mapas, postos e planeamento real na Sprint 2.
