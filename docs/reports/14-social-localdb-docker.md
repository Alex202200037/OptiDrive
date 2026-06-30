# OptiDrive - Modulo Social, LocalDB e Docker

Estado do documento: evidencia tecnica da iteracao de reforco social e persistencia.

## 1. Objetivo

Esta iteracao teve como objetivo reforcar a componente social do OptiDrive, introduzir autenticação federada configurável com Google, Microsoft e Apple OAuth, substituir o estado apenas em memoria por persistencia local em SQLite e permitir execucao da aplicacao em Docker no MacBook.

## 2. Funcionalidades sociais implementadas

| Area | Funcionalidade | Estado |
| --- | --- | --- |
| Perfil social | bio, cidade, estilo de conducao, marcas preferidas e estado de partilha | Implementado |
| Contactos | adicionar circulo de confianca | Implementado |
| Mensagens | enviar e consultar mensagens diretas | Implementado |
| Grupos | criar grupos com membros e split de custos | Implementado |
| Viagens colaborativas | criar viagem conjunta associada a rota/veiculo | Implementado |
| Arranque conjunto | iniciar viagem em grupo e registar estado `Active` | Implementado |
| Partilha de veiculo | partilhar veiculo com permissao de leitura/edicao | Implementado |

## 3. Autenticacao Google

Foi adicionada a base tecnica para Google OAuth atraves de `Microsoft.AspNetCore.Authentication.Google`.

O login Google fica ativo quando forem configuradas as seguintes chaves:

```json
"Authentication": {
  "Google": {
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

Sem essas credenciais, o botao aparece desativado e a app continua funcional com login local.

## 4. Persistencia LocalDB

Como SQL Server LocalDB nao e nativo em MacBook, foi adotada a alternativa mais adequada ao ambiente: `SQLite` com `EF Core`.

Localizacao local:

```text
OptiDrive.Web/App_Data/optidrive.db
```

Entidades persistidas:

- utilizadores e perfis sociais
- garagem e atividades dos veiculos
- postos e carregadores
- rotas e paragens Smart Save
- contactos, partilhas, mensagens e viagens colaborativas
- reportes de preco
- estados de sincronizacao das APIs

## 5. Docker

Foram criados:

- `Dockerfile`
- `docker-compose.yml`
- `OptiDrive.Web/appsettings.Docker.json`
- `.dockerignore`

Comando:

```bash
docker compose up -d
```

URL:

```text
http://127.0.0.1:5080
```

A base de dados Docker fica em:

```text
./data/optidrive.db
```

## 6. Evidencia de validacao

Validações executadas:

- `dotnet build OptiDrive.Web/OptiDrive.Web.csproj`
- `dotnet test OptiDrive.sln`
- `docker compose config`
- `docker compose build`
- `docker compose up -d`
- `GET http://127.0.0.1:5080/Home/Login` com resposta `200`
- smoke test visual da pagina `/Social`

Resultado da suite automatizada:

- `6` testes executados
- `6` testes com sucesso
- `0` falhas

Screenshot:

```text
docs/screenshots/social-module.png
```

## 7. Jira

Issues criadas/atualizadas para esta entrega:

| Issue | Descricao | Estado |
| --- | --- | --- |
| OP-32 | Implementar modulo social avancado | Done |
| OP-33 | Preparar login com Google OAuth | In Review |
| OP-34 | Implementar LocalDB SQLite com EF Core | Done |
| OP-35 | Preparar Docker para MacBook | Done |
| OP-36 | Validar social, LocalDB e Docker | Done |

## 8. Limitacoes assumidas

- Login Google, Microsoft e Apple requerem credenciais OAuth reais para validação externa completa.
- A componente social nao usa WebSockets; mensagens e estados sao persistidos, mas nao em tempo real.
- As portagens continuam estimadas, pois nao existe API oficial de portagens integrada.

## 9. Conclusao

Esta iteracao reforca significativamente a parte social, que e uma area valorizada na cadeira, e remove uma das principais limitacoes tecnicas anteriores: a falta de persistencia. O OptiDrive passa a ser apresentavel como aplicacao `.NET 8` persistente, socialmente colaborativa e executavel em Docker no MacBook.
