# Diagramas do OptiDrive

Este diretório contém as fontes editáveis dos 13 diagramas únicos usados na documentação oficial. As versões PNG e SVG estão em `../mermaid-rendered/`.

## Critério de equivalência

As novas versões preservam os elementos e o significado dos diagramas anteriores: atores, módulos, classes, componentes, decisões, relações, fluxos, métricas e períodos. As alterações são exclusivamente de apresentação e clareza:

- organização por camadas ou módulos;
- rótulos completos e consistentes;
- ligações com direção e resultado explícitos;
- redução de cruzamentos e elementos sem destino;
- contraste, espaçamento e tipografia uniformes;
- remoção de marcas de avaliação das imagens destinadas aos relatórios.

## Ficheiros

| Fonte Mermaid | Diagrama |
|---|---|
| `arquitetura-geral.mmd` | Arquitetura geral por camadas |
| `uml-pacotes-logicos.mmd` | Pacotes lógicos e dependências |
| `classes-dominio.mmd` | Classes principais do domínio |
| `componentes.mmd` | Componentes da aplicação |
| `deployment.mmd` | Implantação em Azure e integrações |
| `uml-use-cases-geral.mmd` | Casos de utilização por módulo |
| `bpmn-01-planeamento-rota.mmd` | Planeamento de rota e Smart Save |
| `bpmn-02-garagem-veiculo.mmd` | Gestão de veículos e histórico |
| `bpmn-03-social-viagem.mmd` | Viagem colaborativa e mensagens |
| `bpmn-04-devops-entrega.mmd` | Pipeline de integração e entrega |
| `gantt-oficial.mmd` | Planeamento temporal do projeto |
| `burndown-geral.mmd` | Evolução do esforço restante |
| `velocity-geral.mmd` | Pontos concluídos por sprint |

## Editar em mermaid.ai

1. Abrir o editor do Mermaid Chart.
2. Criar um diagrama novo e escolher a opção de código Mermaid.
3. Abrir o ficheiro `.mmd` pretendido e colar todo o conteúdo no editor.
4. Confirmar que nenhum ator, passo, relação ou valor foi removido.
5. Exportar em SVG para arquivo e em PNG para o Confluence.

O ficheiro `mermaid-config.json` contém a paleta e a tipografia comuns usadas na renderização local.
