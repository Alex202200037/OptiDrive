# Evidência de Diagramas em Visual Paradigm

Este diretório contém a evidência técnica de que os diagramas finais do OptiDrive foram gerados/preparados em Visual Paradigm Enterprise 18.1, em modo de avaliação.

## Projeto Visual Paradigm

- `OptiDrive-Visual-Paradigm.vpp`: projeto Visual Paradigm com os diagramas do OptiDrive.

## Diagramas incluídos no projeto

- `gantt-oficial`
- `burndown-geral`
- `velocity-geral`
- `uml-use-cases-geral`
- `classes-dominio`
- `uml-pacotes-logicos`
- `arquitetura-geral`
- `componentes`
- `deployment`
- `bpmn-01-planeamento-rota`
- `bpmn-02-garagem-veiculo`
- `bpmn-03-social-viagem`

## Evidência de execução

- `visual-paradigm-generate.log`: criação do projeto `.vpp` via plugin do Visual Paradigm.
- `visual-paradigm-import-bpmn.log`: importação dos processos BPMN pelo comando oficial `ImportBPMN`.
- `visual-paradigm-export.log`: exportação de imagens pelo comando oficial `ExportDiagramImage`.
- `exported-from-visual-paradigm/`: imagens exportadas diretamente pelo Visual Paradigm.
- `screenshots/`: capturas de ecrã da aplicação Visual Paradigm aberta no macOS.

## Nota operacional

A versão em avaliação do Visual Paradigm pode acrescentar watermark nas imagens exportadas diretamente. Para validação académica, a evidência principal é o ficheiro `.vpp` e os logs oficiais de importação/exportação.

- `bpmn-04-devops-entrega.png`: BPMN da Sprint 5, referente a DevOps, validação e entrega final.
