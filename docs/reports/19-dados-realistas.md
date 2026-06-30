# OptiDrive - Dataset Realista para Apresentacao

Estado do documento: relatorio complementar de apresentacao, criado para suportar a apresentacao do produto com dados de utilizacao realistas.

## 1. Objetivo

Foi enriquecida a base de dados local do OptiDrive com utilizadores, veiculos, consumos, historico, conversas e viagens colaborativas para que a apresentacao nao pareca uma aplicacao vazia.

O objetivo e permitir mostrar o produto como se ja estivesse a ser usado por varias pessoas.

## 2. Utilizadores criados/atualizados

| Utilizador | Email | Perfil | Estado |
| --- | --- | --- | --- |
| Alexandre Miguel | alexandre@optidrive.pt | Condutor principal | Online |
| Marta Costa | marta.costa@optidrive.pt | Perfil hibrido/EV | Online |
| Joao Pereira | joao.pereira@optidrive.pt | Diesel autoestrada | Online |
| Ines Martins | ines.martins@optidrive.pt | EV e carregamentos | Online |
| Rui Almeida | rui.almeida@optidrive.pt | GPL low-cost | Online recente |
| Sofia Rocha | sofia.rocha@optidrive.pt | Viagens longas | Online |
| Alexandre Admin | admin@optidrive.pt | Administracao | Backoffice |

## 3. Veiculos realistas

| Utilizador | Veiculo | Tipo | Consumo realista |
| --- | --- | --- | --- |
| Alexandre Miguel | Peugeot 308 SW 1.5 BlueHDi | Diesel | 5.2 L/100 km |
| Alexandre Miguel | Tesla Model 3 RWD | Eletrico | 15.8 kWh/100 km |
| Marta Costa | Toyota Yaris Hybrid 1.5 | Gasolina 95 | 4.3 L/100 km |
| Marta Costa | Renault Zoe R135 ZE50 | Eletrico | 16.5 kWh/100 km |
| Joao Pereira | Volkswagen Golf 2.0 TDI | Diesel | 5.0 L/100 km |
| Ines Martins | Hyundai Kauai Electric 64 kWh | Eletrico | 15.2 kWh/100 km |
| Rui Almeida | Dacia Sandero Stepway Bi-Fuel | GPL | 7.4 L/100 km |
| Sofia Rocha | BMW 320d Touring | Diesel | 5.4 L/100 km |

## 4. Dados sociais adicionados

| Area | Dados inseridos |
| --- | --- |
| Contactos | 7 ligacoes de confianca entre utilizadores |
| Partilhas | 3 veiculos partilhados entre contactos |
| Conversas | 11 mensagens com contexto de rotas, autonomia, EV, GPL e split |
| Viagens colaborativas | 4 viagens sociais, incluindo uma ativa |
| Historico de garagem | 15 atividades entre abastecimentos, cargas, viagens e updates |
| Relatorios de preco | 4 reports de precos de postos |

## 5. Evidencia da base de dados

Apos reiniciar Docker, o healthcheck devolveu:

```json
{
  "status": "Healthy",
  "counts": {
    "users": 7,
    "vehicles": 8,
    "routes": 2
  }
}
```

Validacao direta na SQLite local:

| Tabela | Registos |
| --- | --- |
| Users | 7 |
| SocialProfiles | 7 |
| Vehicles | 8 |
| Contacts | 7 |
| SharedVehicles | 3 |
| DirectMessages | 11 |
| CollaborativeTrips | 4 |
| VehicleActivities | 15 |
| PriceReports | 4 |

## 6. Fluxo recomendado para apresentacao

1. Entrar com `alexandre@optidrive.pt` / `opti2026`.
2. Abrir Social.
3. Mostrar 5 contactos, 4 viagens sociais e membros online.
4. Mostrar a viagem ativa `Caravana Setubal -> Faro`.
5. Mostrar mensagens entre Marta, Joao, Ines, Rui, Sofia e Alexandre Miguel.
6. Abrir Garagem e mostrar os veiculos do Alexandre Miguel com historico.
7. Se necessario, iniciar sessao com outros utilizadores para mostrar garagens diferentes.

## 7. Conclusao

O OptiDrive passa a ter uma base local rica para apresentacao: varios perfis, carros reais, consumos plausiveis, atividade social e historico de utilizacao. Isto melhora a percecao de produto acabado e permite apresentar melhor o modulo social, a garagem e o planeamento.
