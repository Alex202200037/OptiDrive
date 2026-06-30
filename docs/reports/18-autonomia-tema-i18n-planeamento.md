# OptiDrive - Hardening de Autonomia, Consumo por Velocidade e UI Bilingue

Estado do documento: relatorio complementar de fecho para a apresentacao final, associado ao hardening do planeamento inteligente e experiencia de utilizador.

## 1. Objetivo

Este relatorio documenta a revisao final feita ao modulo de planeamento para corrigir a estimativa de chegada, acrescentar consumo dependente da velocidade media e melhorar a experiencia visual com tema claro/escuro e idioma portugues/ingles.

O foco foi eliminar um comportamento incorreto em que uma rota podia apresentar chegada estimada a 0% mesmo quando o Smart Save ja tinha planeado reforcos de combustivel ou carga.

## 2. Alteracoes funcionais

| Area | Alteracao | Resultado |
| --- | --- | --- |
| Autonomia | Criada margem minima de chegada por tipo de veiculo | EV chega com alvo minimo de 12%; combustao com alvo minimo de 8% |
| Reforcos | Recalculo do reforco sugerido quando a chegada fica abaixo da margem | As paragens passam a considerar nao so terminar a viagem, mas terminar com reserva |
| Consumo | Consumo ajustado pela velocidade media introduzida no formulario | Velocidades elevadas aumentam consumo e reduzem autonomia efetiva |
| EV | Paragens eletricas integradas no Smart Save | O planeamento EV passa a considerar carregadores para autonomia e reforco |
| UI | Tema claro/escuro persistente | Preferencia guardada no browser via localStorage |
| Idioma | Alternador PT/EN no layout | Textos principais da interface ficam disponiveis em portugues e ingles |

## 3. Modelo de consumo por velocidade

A estimativa base deixou de usar apenas o consumo medio fixo do veiculo. O sistema calcula agora um consumo ajustado, com fator de velocidade:

| Tipo de veiculo | Regra aplicada |
| --- | --- |
| Eletrico | Penaliza velocidades altas, especialmente acima dos 100 km/h, por maior resistencia aerodinamica |
| Combustao | Penaliza cidade lenta e autoestrada rapida; mantem melhor eficiencia perto de velocidade de cruzeiro |

Campos adicionados ao modelo de rota:

| Campo | Utilizacao |
| --- | --- |
| `AverageSpeedKmh` | Velocidade media usada no calculo |
| `AdjustedConsumptionPer100` | Consumo efetivo usado na rota |
| `ConsumptionSpeedFactor` | Fator aplicado face ao consumo medio do veiculo |
| `MinimumArrivalLevelPercent` | Reserva minima alvo a chegada |

## 4. Evidencia de validacao

Foi criada uma rota de validacao com veiculo eletrico entre Setubal e Faro, via Grandola, a 130 km/h.

Resultado observado:

| Indicador | Valor |
| --- | --- |
| Chegada sem paragens | 0% |
| Chegada com Smart Save | 45% |
| Margem minima alvo | 12% |
| Consumo ajustado | 20.86 kWh/100 km |
| Velocidade media | 130 km/h |
| Paragens planeadas | 1 |
| Postos devolvidos no mapa | 220 proximos da rota |

Isto apresenta que o bug original foi corrigido: mesmo quando a rota direta ficaria a 0%, o plano com reforco deixa uma margem realista.

## 5. Validacao tecnica executada

```bash
dotnet build OptiDrive.Web/OptiDrive.Web.csproj
node --check OptiDrive.Web/wwwroot/js/site.js
node --check OptiDrive.Web/wwwroot/js/planning-map.js
dotnet test OptiDrive.sln
docker compose build
docker compose up -d
curl http://127.0.0.1:5080/health
```

Resultado:

| Validacao | Estado |
| --- | --- |
| Build ASP.NET Core | Passou |
| Sintaxe JavaScript | Passou |
| Testes automatizados | 9/9 passaram |
| Docker MacBook | Container reconstruido e iniciado |
| Healthcheck | Healthy |

## 6. Impacto na apresentacao

Durante a apresentacao, o fluxo recomendado passa a ser:

1. Abrir Planeamento.
2. Selecionar o veiculo eletrico.
3. Planear Setubal para Faro com velocidade media elevada.
4. Mostrar que a rota direta ficaria sem autonomia.
5. Mostrar que o Smart Save planeia reforco e chegada com margem.
6. Alternar tema claro/escuro.
7. Alternar PT/EN para evidenciar internacionalizacao.

## 7. Conclusao

O planeamento ficou mais credivel para apresentacao e mais proximo de um produto real. A estimativa ja considera velocidade, reserva minima e reforcos coerentes, enquanto a interface passa a suportar preferencia visual e bilingue.
