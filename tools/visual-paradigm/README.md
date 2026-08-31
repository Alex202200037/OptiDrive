# Gerador Visual Paradigm do OptiDrive

Esta pasta permite recriar os diagramas do OptiDrive diretamente com código usando a Open API e os comandos oficiais do Visual Paradigm.

## Forma Recomendada

Executar a partir da raiz do repositório:

```bash
./tools/visual-paradigm/generate-optidrive-vp.sh
```

O script cria/atualiza:

- `docs/reports-oficial/assets/visual-paradigm/OptiDrive-Visual-Paradigm.vpp`
- `docs/reports-oficial/assets/visual-paradigm/exported-from-visual-paradigm/`
- `docs/reports-oficial/assets/visual-paradigm/visual-paradigm-generate.log`
- `docs/reports-oficial/assets/visual-paradigm/visual-paradigm-import-bpmn.log`
- `docs/reports-oficial/assets/visual-paradigm/visual-paradigm-export.log`

## Se o Visual Paradigm estiver noutro caminho

```bash
VP_APP="/caminho/Visual Paradigm.app" ./tools/visual-paradigm/generate-optidrive-vp.sh
```

## O que é criado por código

- Gantt oficial
- Burndown geral
- Velocity geral
- Use case diagram geral
- Class diagram de domínio
- Package diagram lógico
- Component diagram de arquitetura
- Component diagram de serviços
- Deployment diagram

## O que é importado como BPMN

Os processos BPMN estão em:

- `bpmn-source/bpmn-01-planeamento-rota.bpmn`
- `bpmn-source/bpmn-02-garagem-veiculo.bpmn`
- `bpmn-source/bpmn-03-social-viagem.bpmn`

O script importa estes ficheiros usando o comando oficial `ImportBPMN` do Visual Paradigm.

## Notas

- A versão de avaliação pode exportar imagens com watermark.
- O ficheiro `.vpp` é a prova principal de edição/geração em Visual Paradigm.
- Para mostrar ao professor, abrir `OptiDrive-Visual-Paradigm.vpp` no Visual Paradigm.
