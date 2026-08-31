#!/usr/bin/env python3
from pathlib import Path
from copy import deepcopy
import re
from docx import Document
from docx.shared import Inches, Pt
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.text.paragraph import Paragraph
from docx.table import Table
from docx.oxml.ns import qn
from zipfile import ZipFile, ZIP_DEFLATED
import tempfile

ROOT = Path.cwd()
OUT = ROOT / 'docs' / 'confluence-docx-oficial'
ASSETS = ROOT / 'docs' / 'reports-oficial' / 'assets' / 'diagrams'
TEMPLATE_DIR = Path('/Users/alexandremiguel/Downloads/OneDrive_1_7-8-2026/Templates')
TEMPLATE_AER = TEMPLATE_DIR / 'Template - Analise e Especificação de Requisitos.docx'
TEMPLATE_ATA = TEMPLATE_DIR / 'Template - Ata.docx'
TEMPLATE_HLD = TEMPLATE_DIR / 'Template - Desenho de alto nível.docx'
TEMPLATE_DD = TEMPLATE_DIR / 'Template - Desenho detalhado.docx'
OUT.mkdir(parents=True, exist_ok=True)

AUTHOR = 'Alexandre Miguel'
STUDENT = 'nº202200037 Alexandre Miguel'
PROJECT = 'OptiDrive'
COURSE = 'Engenharia de Software Aplicada'
DATE = '08/07/2026'
TURMA = 'ESA PL1'
GROUP = 'OP'

SPRINTS = [
    {'n':1,'name':'Identidade, Segurança e Garagem','start':'11/05/2026','end':'20/05/2026','mods':'M01, M02','date':'20/05/2026','goal':'Login local, MFA, sessão, garagem base e histórico inicial','bpmn':'bpmn-02-garagem-veiculo.png'},
    {'n':2,'name':'Mapas, Postos e Energia','start':'21/05/2026','end':'30/05/2026','mods':'M03, M04','date':'30/05/2026','goal':'Google Maps, rotas, postos de combustível e carregadores EV','bpmn':'bpmn-01-planeamento-rota.png'},
    {'n':3,'name':'Smart Save e Histórico Operacional','start':'01/06/2026','end':'08/06/2026','mods':'M02, M05','date':'08/06/2026','goal':'Consumo por velocidade, autonomia, reforços, aplicar viagem ao veículo','bpmn':'bpmn-01-planeamento-rota.png'},
    {'n':4,'name':'Social, OAuth e Viagens Colaborativas','start':'09/06/2026','end':'16/06/2026','mods':'M01, M06','date':'16/06/2026','goal':'Perfis sociais, contactos, mensagens, viagens em grupo e autenticação externa','bpmn':'bpmn-03-social-viagem.png'},
    {'n':5,'name':'DevOps, Administração e Entrega Final','start':'17/06/2026','end':'30/06/2026','mods':'M07','date':'30/06/2026','goal':'Docker, Azure, backoffice, i18n, tema, métricas e documentação','bpmn':'deployment.png'},
]
MODULES = [
    ('M01','Identidade e Segurança','Login local, OAuth, sessão, MFA TOTP, códigos de recuperação e perfis de acesso','Must'),
    ('M02','Garagem e Veículos','Gestão de veículos, autonomia, combustível/carga, manutenção e histórico','Must'),
    ('M03','Planeamento Inteligente','Rotas, origem/destino, pontos intermédios opcionais, itinerário e portagens','Must'),
    ('M04','Postos e Energia','Postos de combustível, carregadores EV, filtros por marca, energia e proximidade','Must'),
    ('M05','Smart Save e Autonomia','Consumo por velocidade, custo estimado, reforços e reserva de chegada','Must'),
    ('M06','Social e Viagens Colaborativas','Perfis, contactos, mensagens, grupos, viagens partilhadas e split de custos','Must'),
    ('M07','Administração e Observabilidade','Backoffice, sincronizações, healthcheck, Docker, i18n e tema','Should'),
]
REQS = [
('RF-M01-01','M01','Must','O sistema deverá permitir criar conta local com nome, email e palavra-passe.'),
('RF-M01-02','M01','Must','O sistema deverá autenticar utilizadores locais com hash seguro de palavra-passe.'),
('RF-M01-03','M01','Must','O sistema deverá permitir iniciar sessão e terminar sessão de forma segura.'),
('RF-M01-04','M01','Should','O sistema deverá permitir autenticação externa por Google e Microsoft quando configurada.'),
('RF-M01-05','M01','Should','O sistema deverá permitir ativar MFA TOTP por aplicação Authenticator compatível.'),
('RF-M01-06','M01','Should','O sistema deverá disponibilizar códigos de recuperação para MFA.'),
('RF-M02-01','M02','Must','O sistema deverá permitir criar, editar, consultar e remover veículos da garagem.'),
('RF-M02-02','M02','Must','O sistema deverá associar marca, modelo, ano, combustível, consumo e matrícula a cada veículo.'),
('RF-M02-03','M02','Must','O sistema deverá registar o nível atual de combustível ou carga de cada veículo.'),
('RF-M02-04','M02','Should','O sistema deverá permitir registar abastecimentos ou carregamentos e atualizar o histórico.'),
('RF-M02-05','M02','Should','O sistema deverá refletir viagens aplicadas no nível e histórico do veículo.'),
('RF-M03-01','M03','Must','O sistema deverá permitir planear rota com origem e destino escritos pelo utilizador.'),
('RF-M03-02','M03','Should','O sistema deverá permitir pontos intermédios opcionais, sem os tornar obrigatórios.'),
('RF-M03-03','M03','Must','O sistema deverá apresentar mapa, rota visual, distância, duração e instruções.'),
('RF-M03-04','M03','Should','O sistema deverá recalcular rota quando o utilizador seleciona evitar portagens.'),
('RF-M03-05','M03','Must','O sistema deverá apresentar custo estimado total da viagem.'),
('RF-M04-01','M04','Must','O sistema deverá consultar e apresentar postos de combustível disponíveis.'),
('RF-M04-02','M04','Should','O sistema deverá agregar combustíveis e preços por posto quando existirem dados disponíveis.'),
('RF-M04-03','M04','Must','O sistema deverá consultar e apresentar carregadores elétricos via OpenChargeMap.'),
('RF-M04-04','M04','Should','O sistema deverá filtrar postos por marca e por combustível/energia compatível com o veículo.'),
('RF-M04-05','M04','Should','O sistema deverá privilegiar postos próximos da rota calculada.'),
('RF-M05-01','M05','Must','O sistema deverá estimar consumo com base no veículo, distância e energia utilizada.'),
('RF-M05-02','M05','Should','O sistema deverá ajustar consumo previsto de acordo com velocidade média.'),
('RF-M05-03','M05','Must','O sistema deverá verificar se a autonomia disponível chega ao destino.'),
('RF-M05-04','M05','Must','O sistema deverá sugerir reforços de combustível ou carga quando a autonomia for insuficiente.'),
('RF-M05-05','M05','Should','O sistema deverá garantir margem mínima de chegada, evitando estimativas de chegada a zero.'),
('RF-M06-01','M06','Must','O sistema deverá permitir manter perfil social com nome, fotografia, cidade e preferências.'),
('RF-M06-02','M06','Must','O sistema deverá permitir pesquisar utilizadores e solicitar ligação social.'),
('RF-M06-03','M06','Must','O sistema deverá permitir trocar mensagens entre utilizadores ligados.'),
('RF-M06-04','M06','Should','O sistema deverá permitir criar viagens colaborativas com participantes.'),
('RF-M06-05','M06','Could','O sistema deverá permitir dividir custos estimados entre participantes.'),
('RF-M07-01','M07','Must','O sistema deverá disponibilizar backoffice para estado de integrações e utilizadores.'),
('RF-M07-02','M07','Must','O sistema deverá disponibilizar endpoint de saúde operacional.'),
('RF-M07-03','M07','Should','O sistema deverá disponibilizar modo claro/escuro e português/inglês.'),
('RF-M07-04','M07','Must','O sistema deverá executar em Docker com base de dados persistente.'),
]
UCS = [
('UC-01','Criar conta local','Visitante','RF-M01-01, RF-M01-02'),
('UC-02','Iniciar sessão/MFA','Visitante; Condutor autenticado','RF-M01-03, RF-M01-04, RF-M01-05, RF-M01-06'),
('UC-03','Gerir garagem','Condutor autenticado','RF-M02-01, RF-M02-02, RF-M02-03'),
('UC-04','Registar abastecimento/carga','Condutor autenticado','RF-M02-04'),
('UC-05','Planear rota','Condutor autenticado; Google Maps','RF-M03-01, RF-M03-02, RF-M03-03, RF-M03-04, RF-M03-05'),
('UC-06','Consultar postos/carregadores','Condutor autenticado; OpenChargeMap/Postos','RF-M04-01, RF-M04-02, RF-M04-03, RF-M04-04, RF-M04-05'),
('UC-07','Calcular Smart Save','Condutor autenticado','RF-M05-01, RF-M05-02, RF-M05-03, RF-M05-04, RF-M05-05'),
('UC-08','Aplicar viagem ao veículo','Condutor autenticado','RF-M02-05, RF-M05-01'),
('UC-09','Gerir perfil social','Condutor autenticado','RF-M06-01'),
('UC-10','Solicitar ligação social','Condutor autenticado; Participante social','RF-M06-02'),
('UC-11','Enviar mensagem','Condutor autenticado; Participante social','RF-M06-03'),
('UC-12','Criar viagem colaborativa','Condutor autenticado; Participante social','RF-M06-04, RF-M06-05'),
('UC-13','Consultar backoffice','Administrador','RF-M07-01'),
('UC-14','Consultar healthcheck','Administrador','RF-M07-02'),
('UC-15','Alterar tema/idioma','Condutor autenticado','RF-M07-03'),
]
ACTORS = [
('A01','Visitante','Humano','Pessoa que ainda não iniciou sessão.'),
('A02','Condutor autenticado','Humano','Utilizador principal que gere veículos e rotas.'),
('A03','Participante social','Humano','Utilizador que participa em mensagens ou viagens colaborativas.'),
('A04','Administrador','Humano','Utilizador com acesso ao backoffice e monitorização.'),
('A05','Google Maps','Sistema externo','Serviço de geocoding, mapas e direções.'),
('A06','OpenChargeMap/Postos','Sistema externo','Fontes de carregadores e informação energética.'),
('A07','Fornecedor OAuth','Sistema externo','Google/Microsoft para login externo configurável.'),
]
QUALITY = [
('RQ1','Usabilidade','Capacidade de interação','A interface deverá ser compreensível em desktop e mobile, suportando PT/EN e modo claro/escuro.','Must'),
('RQ2','Segurança','Autenticidade','A autenticação deverá usar hash seguro, cookies HttpOnly, MFA e proteção antiforgery.','Must'),
('RQ3','Eficiência','Tempo de resposta','As páginas principais deverão responder localmente de forma fluida, com APIs externas assíncronas.','Should'),
('RQ4','Manutenibilidade','Modularidade','Controllers, serviços, ViewModels e acesso a dados deverão estar separados.','Should'),
('RQ5','Compatibilidade','Portabilidade','A aplicação deverá executar em macOS via Docker e em Azure App Service Linux.','Must'),
('RQ6','Fiabilidade','Tolerância a falhas','A indisponibilidade de APIs externas deverá ter fallback ou mensagem clara.','Should'),
]
ENV_REQS = [
('RA1','Hardware','MacBook ou computador equivalente com Docker para execução local.'),
('RA2','Software','.NET 8 SDK, Docker Desktop e browser moderno.'),
('RA3','Linguagem','ASP.NET Core MVC, C#, Razor, JavaScript e CSS.'),
('RA4','Browser','Safari, Chrome, Edge ou Firefox atualizados.'),
]

SPRINT_REQS = {
  1: ['RF-M01-01','RF-M01-02','RF-M01-03','RF-M01-05','RF-M01-06','RF-M02-01','RF-M02-02','RF-M02-03'],
  2: ['RF-M03-01','RF-M03-02','RF-M03-03','RF-M03-04','RF-M03-05','RF-M04-01','RF-M04-02','RF-M04-03','RF-M04-04','RF-M04-05'],
  3: ['RF-M02-04','RF-M02-05','RF-M05-01','RF-M05-02','RF-M05-03','RF-M05-04','RF-M05-05'],
  4: ['RF-M01-04','RF-M06-01','RF-M06-02','RF-M06-03','RF-M06-04','RF-M06-05'],
  5: ['RF-M07-01','RF-M07-02','RF-M07-03','RF-M07-04'],
}


def set_run_font(run, size=10, bold=False, font='Times New Roman'):
    run.font.name = font
    run.font.size = Pt(size)
    run.bold = bold
    if run._element.rPr is not None:
        run._element.rPr.rFonts.set(qn('w:ascii'), font)
        run._element.rPr.rFonts.set(qn('w:hAnsi'), font)


def set_para_text(p, text, bold=False):
    p.clear()
    r = p.add_run(text)
    set_run_font(r, 10, bold)


def set_cover_title_cell(cell, text):
    for p in cell.paragraphs:
        p.clear()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p = cell.paragraphs[0]
    r = p.add_run(text)
    set_run_font(r, 25, bold=True, font='Times New Roman')


def replace_cover(doc, title=None):
    filled_student = False
    for p in doc.paragraphs:
        txt = p.text.strip()
        if txt == 'Turma:': set_para_text(p, f'Turma: {TURMA}')
        elif txt == 'Grupo nº': set_para_text(p, f'Grupo nº: {GROUP}')
        elif re.match(r'nº\s*x+', txt, re.I):
            if not filled_student:
                set_para_text(p, STUDENT)
                filled_student = True
            else:
                set_para_text(p, '')
        elif txt == 'Data entrega': set_para_text(p, DATE)
        elif txt == 'SETÚBAL': set_para_text(p, 'SETÚBAL')
    # Replace title in the first single-column title table where possible
    if title and doc.tables:
        for tbl in doc.tables:
            try:
                table_text = '\n'.join(c.text for row in tbl.rows for c in row.cells)
                if len(tbl.rows) >= 2 and len(tbl.columns) >= 1 and 'Engenharia de Software' in table_text:
                    set_cover_title_cell(tbl.cell(1,0), title)
                    break
            except Exception:
                continue


def fill_version_table(doc, rows):
    for t in doc.tables:
        try:
            first_cell_text = t.cell(0,0).text.strip().lower()
        except Exception:
            continue
        if first_cell_text.startswith('vers'):
            ensure_rows(t, len(rows)+1)
            for i,row in enumerate(rows, start=1):
                for j,val in enumerate(row):
                    if j < len(t.columns): set_cell(t.cell(i,j), val)
            # clear extra rows
            for i in range(len(rows)+1, len(t.rows)):
                for c in t.rows[i].cells: set_cell(c, '')
            return


def set_cell(cell, text, bold=False):
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for p in cell.paragraphs:
        p.clear()
    p = cell.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    r = p.add_run(str(text))
    set_run_font(r, 8.5 if len(str(text)) > 80 else 9, bold)


def set_label_body_cell(cell, label, body):
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.TOP
    for p in cell.paragraphs:
        p.clear()
    p = cell.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    r = p.add_run(label)
    set_run_font(r, 10, True, font='Times New Roman')
    p.add_run('\n')
    r2 = p.add_run(body)
    set_run_font(r2, 9.5, False, font='Times New Roman')



def make_table_rows_breakable(table):
    for row in table.rows:
        trPr = row._tr.get_or_add_trPr()
        for tag in [qn('w:trHeight'), qn('w:cantSplit')]:
            for el in list(trPr.findall(tag)):
                trPr.remove(el)

def set_table_font_size(table, size=8):
    for row in table.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                for run in p.runs:
                    run.font.size = Pt(size)

def set_cell_width(cell, width_in):
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    tcW = tcPr.first_child_found_in('w:tcW')
    if tcW is None:
        tcW = OxmlElement('w:tcW')
        tcPr.append(tcW)
    tcW.set(qn('w:w'), str(int(width_in * 1440)))
    tcW.set(qn('w:type'), 'dxa')

def tune_requirements_table(table):
    make_table_rows_breakable(table)
    table.autofit = False
    widths = [0.75, 0.55, 5.0, 0.75]
    for row in table.rows:
        for idx, width in enumerate(widths):
            if idx < len(row.cells):
                set_cell_width(row.cells[idx], width)
    set_table_font_size(table, 7.5)

def ensure_rows(table, n):
    while len(table.rows) < n:
        table.add_row()


def fill_table(table, rows, header=None):
    if header:
        ensure_rows(table, 1)
        for j,val in enumerate(header):
            if j < len(table.columns): set_cell(table.cell(0,j), val, bold=True)
        start=1
    else:
        start=1
    ensure_rows(table, len(rows)+start)
    for i,row in enumerate(rows, start=start):
        for j,val in enumerate(row):
            if j < len(table.columns): set_cell(table.cell(i,j), val)
        for j in range(len(row), len(table.columns)):
            set_cell(table.cell(i,j), '')
    for i in range(len(rows)+start, len(table.rows)):
        for c in table.rows[i].cells: set_cell(c, '')
    table.alignment = WD_TABLE_ALIGNMENT.CENTER


def find_para(doc, startswith):
    s = startswith.lower()
    for p in doc.paragraphs:
        if p.text.strip().lower().startswith(s):
            return p
    return None


def find_para_with_style(doc, text, style_name):
    target = text.strip().lower()
    for p in doc.paragraphs:
        if p.text.strip().lower() == target and p.style and p.style.name == style_name:
            return p
    return None


def insert_paragraph_after(paragraph, text='', style=None, bold=False):
    new_p = OxmlElement('w:p')
    paragraph._p.addnext(new_p)
    p = Paragraph(new_p, paragraph._parent)
    if style:
        try: p.style = style
        except Exception: pass
    if text:
        r=p.add_run(text)
        set_run_font(r, 10, bold)
    return p



def insert_paragraph_after_table(table, text='', style=None, bold=False):
    new_p = OxmlElement('w:p')
    table._tbl.addnext(new_p)
    p = Paragraph(new_p, table._parent)
    if style:
        try: p.style = style
        except Exception: pass
    if text:
        r=p.add_run(text)
        set_run_font(r, 10, bold)
    return p

def insert_table_after(paragraph, rows, cols, data, header=None):
    doc = paragraph.part.document
    table = doc.add_table(rows=rows, cols=cols)
    table.style = 'Table Grid'
    fill_table(table, data, header=header)
    paragraph._p.addnext(table._tbl)
    return table


def insert_image_after(paragraph, image_name, caption, width=6.2):
    path = ASSETS / image_name
    if not path.exists(): return paragraph
    p = insert_paragraph_after(paragraph, '')
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run()
    r.add_picture(str(path), width=Inches(width))
    cap = insert_paragraph_after(p, caption)
    cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    for run in cap.runs:
        run.italic = True
        set_run_font(run, 9)
    return cap


def add_bullets_after(paragraph, items):
    last = paragraph
    for item in reversed(items):
        # insert in reverse to preserve order with addnext
        pass
    for item in items:
        last = insert_paragraph_after(last, item, style='List Bullet')
    return last


def add_paragraphs_after(paragraph, items):
    last=paragraph
    for item in items:
        last=insert_paragraph_after(last,item)
    return last


def set_toc_line(paragraph, text, level=1):
    paragraph.clear()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.LEFT
    paragraph.paragraph_format.left_indent = Inches(0.25 * max(0, level - 1))
    paragraph.paragraph_format.first_line_indent = Inches(0)
    r = paragraph.add_run(text)
    set_run_font(r, 11, False, font='Times New Roman')


def replace_existing_toc_lines(doc, entries):
    toc_paragraphs = [p for p in doc.paragraphs if p.style and p.style.name.lower().startswith('toc')]
    for p, (text, level) in zip(toc_paragraphs, entries):
        set_toc_line(p, text, level)
    for p in toc_paragraphs[len(entries):]:
        set_para_text(p, '')


def patch_docx_xml_strings(path, replacements):
    path = Path(path)
    with tempfile.TemporaryDirectory() as td:
        td = Path(td)
        with ZipFile(path, 'r') as zin:
            zin.extractall(td)
        for xml_path in (td / 'word').rglob('*.xml'):
            xml = xml_path.read_text(encoding='utf-8', errors='ignore')
            original = xml
            for before, after in replacements.items():
                xml = xml.replace(before, after)
            if xml != original:
                xml_path.write_text(xml, encoding='utf-8')
        tmp = path.with_suffix('.tmp.docx')
        with ZipFile(tmp, 'w', ZIP_DEFLATED) as zout:
            for file_path in td.rglob('*'):
                if file_path.is_file():
                    zout.write(file_path, file_path.relative_to(td).as_posix())
        tmp.replace(path)


def delete_empty_placeholder_paragraphs(doc):
    bad_prefixes = [
        '................................................................', 'Devem apresentar', 'Descrição da arquitetura geral', 'Elaboração e identificação',
        'Exemplo da interface', 'Código de cores', '(master pages)', '(utilização de BPD', '(utilizar milestones)',
        '(referente a implementação', 'Explicação da não implementação', 'Quadro com os requisitos',
        '(Validar a navegação', '(qual a estratégia', '(atenção:', '(integração com sistemas externos',
        '(testar os RQ', '(Plenitude', '(performance', '(Coexistência', '(todos as subcaracterísticas',
        '(Tolerância', '(Integridade', '(baseado no jira', '(testar em laptop', '(Fail safe', '(documento independente'
    ]
    for p in doc.paragraphs:
        t=p.text.strip()
        if any(t.startswith(x) for x in bad_prefixes) or t in {'…','GANTT','Colocar todos os requisitos e todos os módulos','Fazer uma global e uma para cada módulo','Colocar códigos de cores consoante a prioridade (MOSCOW)'}:
            set_para_text(p, '')


def build_aer():
    doc=Document(TEMPLATE_AER)
    replace_cover(doc, 'Análise e Especificação de Requisitos')
    fill_version_table(doc, [
        ('1.0','11/05/2026',AUTHOR,'Primeira análise enviada para validação docente'),
        ('2.0','30/06/2026',AUTHOR,'Consolidação funcional do produto'),
        ('3.0','08/07/2026',AUTHOR,'Documento oficial completo sobre o template'),
        ('3.1','08/07/2026',AUTHOR,'Correções de rastreabilidade, módulos, Gantt e use cases'),
    ])
    delete_empty_placeholder_paragraphs(doc)
    # section text
    sec = {
        '1.1 Missão':'O OptiDrive tem como missão apoiar condutores no planeamento de viagens, integrando garagem, autonomia, custos, postos de combustível, carregadores elétricos e colaboração social numa única aplicação web.',
        '1.2 Ponto de situação':'O sistema encontra-se implementado em ASP.NET Core MVC/.NET 8, com SQLite/EF Core, Docker, autenticação local, MFA, rotas, garagem, Smart Save, social, administração e integrações configuráveis.',
        '2.1.1 Origem histórica':'A ideia surge da necessidade de planear deslocações com maior controlo sobre custos, autonomia e disponibilidade de postos ou carregadores.',
        '2.1.2 Qual o problema':'O problema principal é a falta de ligação entre dados reais do veículo, rota, autonomia, custo e rede de abastecimento/carregamento.',
        '2.1.3 Quais as abordagens':'Foram consideradas soluções isoladas de rotas, garagem ou social. A abordagem escolhida foi integrada, porque permite que o planeamento use dados reais do veículo e atualize o histórico após a viagem.',
        '3.1 Sumário':'Foi usada uma abordagem incremental inspirada em Scrum, dividida em cinco sprints, mantendo backlog, prioridades MoSCoW, atas, retrospetivas e validação por incremento.',
        '3.2 Equipa':'O projeto foi desenvolvido individualmente por Alexandre Miguel, acumulando funções de Product Owner académico, Scrum Master, desenvolvimento full-stack, qualidade e documentação.',
        '3.3 Ferramentas':'Foram utilizadas as seguintes ferramentas: ASP.NET Core MVC/.NET 8, Razor, JavaScript, CSS, EF Core, SQLite, Docker, Google Maps APIs, OpenChargeMap, Jira, Confluence e GitHub.',
        '3.4 Controlo':'O código é mantido em Git/GitHub, com solução .NET separada em projeto web e projeto de testes. A documentação é publicada no Confluence e os artefactos são organizados por sprint.',
        '4.2.1 Requisitos':'Todos os requisitos funcionais usam ID único e começam por “O sistema deverá…”, conforme feedback recebido.',
        '4.3 Atores':'Os atores foram designados por papéis próprios do domínio e não por nomes genéricos.',
        '4.4 Use Cases':'Os use cases têm IDs únicos e estão ligados aos requisitos através da matriz de rastreabilidade.',
        '4.6 Matriz':'A matriz geral relaciona requisitos e use cases por ID, com legenda MoSCoW para prioridade.',
    }
    for key,text in sec.items():
        p=find_para(doc,key)
        if p: add_paragraphs_after(p,[text])
    aer_heading_replacements = {
        '4.5 Módulo: ABC (Must Have)': '4.5 Matrizes por módulo e perfil',
        '4.5.1 Matriz Utilizador': '4.5.1 Matriz de acessos por perfil',
        '4.5.2. Use Case Módulo (Nome do modulo)': '4.5.2 Use cases por módulo',
        '4.5.4 Descrição do Use Case Módulo (nome do modulo)': '4.5.3 Descrição geral dos use cases por módulo',
    }
    for p in doc.paragraphs:
        txt = p.text.strip()
        if txt in aer_heading_replacements:
            set_para_text(p, aer_heading_replacements[txt], bold=True)
        elif txt == 'Software de apoio ao desenvolvimento aplicacional':
            set_para_text(p, 'Desenvolvimento aplicacional: .NET 8, ASP.NET Core MVC, Razor, JavaScript, CSS, EF Core e SQLite.')
        elif txt == 'CONFLUENCE/JIRA':
            set_para_text(p, 'Gestão e documentação: Jira para backlog/sprints e Confluence para documentação oficial.')
        elif txt == 'Git (Lab/Hub)':
            set_para_text(p, 'Controlo de versões: Git/GitHub com solução OptiDrive.sln, projeto web e projeto de testes.')
    # Fill template tables
    fill_table(doc.tables[2], [
        ('Requisitos','Análise e Especificação de Requisitos'),('Desenho','Desenho de Alto Nível'),('Implementação incremental','Desenho Detalhado Sprint 1 a 5'),('Gestão','Atas Sprint 1 a 5, Project Manager e métricas'),('Qualidade','Plano de Testes e Gestão de Erros'),('Encerramento','Documento de Encerramento do Projeto')
    ], header=['Fase','Documento'])
    fill_table(doc.tables[3], [
        ('Análise Requisitos','AER inicial','Revisão módulos/UC','Rastreabilidade','Validação','Fecho'),
        ('Desenho','Arquitetura alto nível','BPD rotas','BPD garagem','BPD social','Deployment'),
        ('Desenvolvimento','Login/Garagem','Mapas/Postos','Smart Save','Social/OAuth','Admin/DevOps'),
        ('Testes','Unitários base','Integração APIs','Autonomia/consumo','Social/OAuth','Sistema e usabilidade'),
        ('Deployment','Local','Docker','Configuração APIs','Azure','Entrega final')
    ], header=['','Sprint #1','Sprint #2','Sprint #3','Sprint #4','Sprint #5'])
    fill_table(doc.tables[4], [('Analistas/Programadores', AUTHOR, '', '', '', '')])
    fill_table(doc.tables[5], [('Cliente/Instrutor','Docente da unidade curricular Engenharia de Software Aplicada')])
    fill_table(doc.tables[6], [
        ('T01','Levantamento e correção de requisitos','8','11/05','13/05',AUTHOR,'Must','Concluída'),
        ('T02','Modelação de módulos, atores e use cases','10','14/05','18/05',AUTHOR,'Must','Concluída'),
        ('T03','Implementação de autenticação, MFA e garagem','16','18/05','24/05',AUTHOR,'Must','Concluída'),
        ('T04','Integração de mapas, postos e carregadores','18','21/05','30/05',AUTHOR,'Must','Concluída'),
        ('T05','Smart Save, autonomia e histórico operacional','14','01/06','08/06',AUTHOR,'Must','Concluída'),
        ('T06','Perfis sociais, mensagens e viagens colaborativas','16','09/06','16/06',AUTHOR,'Must','Concluída'),
        ('T07','Docker, Azure, i18n, tema, admin e documentação','22','17/06','30/06',AUTHOR,'Must','Concluída'),
    ], header=['Tarefa','Descrição','Duração (Horas)','Início','Fim','Responsável','Prioridade','Estado'])
    fill_table(doc.tables[7], [(m[0], f'{m[1]}: {m[2]}', m[3]) for m in MODULES], header=['Módulo','Descrição','Prioridade'])
    fill_table(doc.tables[8], [('RF-M01 a RF-M07','Os requisitos funcionais completos são apresentados no Anexo A, agrupados por módulo e com prioridade MoSCoW.','Must/Should/Could','MoSCoW')], header=['ID','Módulo','Descrição','Prioridade'])
    tune_requirements_table(doc.tables[8])
    fill_table(doc.tables[9], [(a[1],a[3],a[2], 'UC relacionados') for a in ACTORS[:4]], header=['Nome','Descrição','Validação do acesso','Caso de utilização'])
    fill_table(doc.tables[10], [(u[0], u[2], u[1], u[3]) for u in UCS], header=['ID','Ator','Caso de Uso','Descrição'])
    fill_table(doc.tables[11], [
        ('Identidade','R','CRUD','R','CRUD'),('Garagem','-','CRUD','R','CRUD'),('Planeamento','-','CRUD','R','R'),('Social','-','CRUD','CRUD','R'),('Administração','-','R','R','CRUD')
    ], header=['','Visitante','Condutor','Participante','Admin'])
    # Traceability as compact rows in table 12
    fill_table(doc.tables[12], [(u[0], u[3], '', '', '', '') for u in UCS[:5]], header=['UC/RF','Requisitos associados','','','',''])
    fill_table(doc.tables[13], QUALITY, header=['Requisitos de Qualidade','Característica','Sub-característica','Descrição','Prioridade'])
    fill_table(doc.tables[14], [('Pior caso:','Indisponibilidade temporária de APIs externas sem perda de dados locais.'),('Planeado:','Fallback com mensagem clara, cache/local data e continuação do fluxo principal.'),('Teste:','Simular ausência de chaves/API e validar resposta do sistema.'),('Unidades:','Tempo de resposta, sucesso/erro controlado e ausência de exceção visível.')], header=['RQ1','Descrição'])
    fill_table(doc.tables[15], ENV_REQS, header=['Requisitos Ambientais','Categoria','Descrição'])
    # Add Gantt and UC diagrams near relevant headings
    p=find_para(doc,'3.5 Estrutura')
    if p: insert_image_after(p, 'gantt-oficial.png', 'Figura 1 - Gantt oficial do projeto.', width=6.0)
    p=find_para(doc,'4.4 Use Cases')
    if p: insert_image_after(p, 'uml-use-cases-geral.png', 'Figura 2 - Diagrama geral de use cases.', width=6.2)
    doc.add_page_break()
    doc.add_heading('Anexo A - Requisitos funcionais por módulo', level=1)
    doc.add_paragraph('Este anexo completa a tabela de requisitos do template, mantendo IDs únicos, prioridade MoSCoW e redação iniciada por “O sistema deverá…”.')
    module_names = {m[0]: m[1] for m in MODULES}
    for mid in [m[0] for m in MODULES]:
        doc.add_heading(f'{mid} - {module_names[mid]}', level=2)
        tbl = doc.add_table(rows=1, cols=4)
        tbl.style = 'Table Grid'
        rows = [(r[0], r[1], r[3], r[2]) for r in REQS if r[1] == mid]
        fill_table(tbl, rows, header=['ID','Módulo','Descrição','Prioridade'])
        tune_requirements_table(tbl)
    out=OUT/'01-OptiDrive-Analise-Especificacao-Requisitos.docx'
    doc.save(out)
    patch_docx_xml_strings(out, {
        '4.5 Módulo: ABC (Must Have)': '4.5 Matrizes por módulo e perfil',
        '4.5.1 Matriz Utilizador': '4.5.1 Matriz de acessos por perfil',
        '4.5.2. Use Case Módulo (Nome do modulo)': '4.5.2 Use cases por módulo',
        '4.5.4 Descrição do Use Case Módulo (nome do modulo)': '4.5.3 Descrição geral dos use cases por módulo',
    })
    return out


def build_hld():
    doc=Document(TEMPLATE_HLD)
    replace_cover(doc, 'Especificação do Sistema – alto nível')
    fill_version_table(doc,[('1','30/06/2026',AUTHOR,'Versão final funcional'),('2','08/07/2026',AUTHOR,'Documento oficial completo sobre template')])
    delete_empty_placeholder_paragraphs(doc)
    sections = {
        'SUMARIO EXECUTIVO':'Este documento descreve o desenho de alto nível do OptiDrive, traduzindo os requisitos em arquitetura, módulos, processos de negócio, interface, persistência, componentes e deployment.',
        'INTRODUÇÃO':'O desenho segue os princípios dos slides de desenho: separação de responsabilidades, arquitetura modular, representação de dados, componentes, interfaces e implantação.',
        'ARQUITETURA GERAL':'A arquitetura está organizada em apresentação MVC, serviços de domínio, acesso a dados e integrações externas. A UI Razor/JavaScript comunica com controllers; os controllers delegam regras para serviços; os serviços persistem via EF Core e comunicam com APIs externas quando configuradas.',
        'ARQUITETURA LÓGICA':'A arquitetura lógica separa módulos de identidade, garagem, planeamento, energia, Smart Save, social e administração, reduzindo acoplamento entre interface, regras de negócio e persistência.',
        'DIAGRAMA DE CLASSES':'As classes de desenho representam utilizadores, veículos, rotas, atividades, mensagens, viagens colaborativas, postos e estado de sincronização.',
        'PROCESSOS DE NEGÓCIO':'Foram identificados três processos de negócio principais to-be: planeamento de rota/Smart Save, gestão de garagem/histórico e viagem colaborativa social.',
        'PROCESOS DE NEGÓCIO':'Foram identificados três processos de negócio principais to-be: planeamento de rota/Smart Save, gestão de garagem/histórico e viagem colaborativa social.',
        'INTERFACE COM O UTILIZADOR':'A interface organiza a experiência em Perfil, Garagem, Planeamento, Social e Administração, suportando o fio condutor escolher veículo → planear → validar autonomia → aplicar rota → colaborar.',
        'PERSISTÊNCIA':'A persistência é suportada por EF Core e SQLite. As entidades principais são UserAccount, VehicleProfile, PlannedRoute, FuelStation, VehicleActivity, SocialProfile, DirectMessage, CollaborativeTrip e ApiSyncStatus.',
        'ARQUITETURA FÍSICA':'O sistema pode ser executado localmente, em Docker ou em Azure App Service. A configuração sensível é feita por variáveis de ambiente.',
        'NORMAS DE CODIFICAÇÃO':'São aplicadas normas de separação MVC, serviços para regras de domínio, ViewModels para ecrã, variáveis de ambiente para configuração e mecanismos de segurança como antiforgery, cookies HttpOnly e MFA.'
    }
    for key,text in sections.items():
        p=find_para(doc,key)
        if p: add_paragraphs_after(p,[text])
    for p in doc.paragraphs:
        txt = p.text.strip()
        if txt == 'PROCESOS DE NEGÓCIO':
            set_para_text(p, 'PROCESSOS DE NEGÓCIO', bold=True)
        elif txt.startswith('Protótipo (caso exista)') or txt.startswith('Interfaces (Mockup)'):
            set_para_text(p, 'Interfaces e mockups implementados', bold=True)
    for key,img,cap in [
        ('ARQUITETURA GERAL','arquitetura-geral.png','Figura 1 - Arquitetura geral ASP.NET Core MVC.'),
        ('ARQUITETURA LÓGICA','uml-pacotes-logicos.png','Figura 2 - Diagrama de pacotes lógicos.'),
        ('DIAGRAMA DE CLASSES','classes-dominio.png','Figura 3 - Diagrama de classes de desenho.'),
        ('Diagramas dos processos','bpmn-01-planeamento-rota.png','Figura 4 - BPD de planeamento de rota e Smart Save.'),
    ]:
        p=find_para(doc,key)
        if p: insert_image_after(p,img,cap,width=6.2)
    p=find_para(doc,'Diagramas dos processos')
    if p:
        last=insert_image_after(p,'bpmn-02-garagem-veiculo.png','Figura 5 - BPD de gestão de garagem e histórico.',width=6.2)
        insert_image_after(last,'bpmn-03-social-viagem.png','Figura 6 - BPD de viagem colaborativa social.',width=6.2)
    p=find_para(doc,'Diagrama de Componentes')
    if p: insert_image_after(p,'componentes.png','Figura 7 - Diagrama de componentes.',width=6.2)
    p=find_para(doc,'Diagrama de Instalação')
    if p: insert_image_after(p,'deployment.png','Figura 8 - Diagrama de instalação/deployment.',width=6.2)
    # Process and access tables
    p=find_para(doc,'Identificação dos processos')
    if p:
        insert_table_after(p,1,4,[('P01','Planeamento de rota e Smart Save','Processo interno com serviços externos','Calcular rota, custos, autonomia e reforços'),('P02','Gestão de garagem e histórico','Processo interno','Manter dados operacionais do veículo'),('P03','Viagem colaborativa social','Colaboração','Permitir interação entre condutores e participantes')], header=['ID','Processo','Tipo','Objetivo'])
    p=find_para(doc,'Matriz de acessos')
    if p:
        insert_table_after(p,1,5,[('Login/Registo','Sim','Sim','Sim','Sim'),('Perfil','Não','Sim','Sim','Sim'),('Garagem','Não','Sim','Parcial','Sim'),('Planeamento','Não','Sim','Parcial','Sim'),('Social','Não','Sim','Sim','Sim'),('Administração','Não','Não','Não','Sim')], header=['Área','Visitante','Condutor','Participante','Admin'])
    replace_existing_toc_lines(doc, [
        ('1    SUMÁRIO EXECUTIVO ........................................................................ 4', 1),
        ('2    INTRODUÇÃO ..................................................................................... 4', 1),
        ('3    DESENHO DE ALTO NÍVEL ............................................................... 4', 1),
        ('3.1    ARQUITETURA GERAL ................................................................. 4', 2),
        ('3.2    ARQUITETURA LÓGICA ............................................................... 5', 2),
        ('3.3    DIAGRAMA DE CLASSES DE DESENHO .................................... 5', 2),
        ('3.4    PROCESSOS DE NEGÓCIO ........................................................... 6', 2),
        ('3.4.1    Identificação dos processos ......................................................... 6', 3),
        ('3.4.2    Diagramas dos processos de negócio .......................................... 6', 3),
        ('3.5    INTERFACE COM O UTILIZADOR ............................................... 8', 2),
        ('3.5.1    Introdução ...................................................................................... 8', 3),
        ('3.5.2    Interfaces e mockups implementados .......................................... 8', 3),
        ('3.5.3    Normas .......................................................................................... 8', 3),
        ('3.5.4    Diagrama Geral de Navegação ..................................................... 8', 3),
        ('3.5.5    Matriz de acessos ......................................................................... 8', 3),
        ('3.6    PERSISTÊNCIA ............................................................................... 9', 2),
        ('3.6.1    Introdução ...................................................................................... 9', 3),
        ('3.6.2    Modelo Relacional ........................................................................ 9', 3),
        ('3.7    ARQUITETURA FÍSICA ................................................................. 9', 2),
        ('3.7.1    Introdução ...................................................................................... 9', 3),
        ('3.7.2    Diagrama de Componentes ........................................................... 9', 3),
        ('3.7.3    Diagrama de Instalação ............................................................... 10', 3),
        ('3.8    NORMAS DE CODIFICAÇÃO DA APLICAÇÃO .......................... 11', 2),
    ])
    out=OUT/'02-OptiDrive-Desenho-Alto-Nivel.docx'
    doc.save(out); return out


def build_dd(s):
    doc=Document(TEMPLATE_DD)
    replace_cover(doc, f'Desenho detalhado – Sprint nº{s["n"]} - {s["name"]}')
    fill_version_table(doc,[('1',s['date'],AUTHOR,'Documento da sprint'),('2','08/07/2026',AUTHOR,'Documento oficial completo sobre template')])
    delete_empty_placeholder_paragraphs(doc)
    # replace sprint generic heading
    for p in doc.paragraphs:
        if p.text.strip().startswith('Modulo X1'):
            set_para_text(p, f'Módulo(s) {s["mods"]} / Sprint {s["n"]} - {s["name"]}', bold=True)
        if 'Desenho detalhado – Sprint nºXX' in p.text:
            set_para_text(p, f'Desenho detalhado – Sprint nº{s["n"]} - {s["name"]}', bold=True)
        txt = p.text.strip()
        replacements = {
            'Diagrama de Classes de desenho detalhado do Modulo X1': f'Diagrama de classes de desenho detalhado da Sprint {s["n"]}',
            'Diagramas de Processos de negócio referentes ao Modulo X1': f'Diagramas de processos de negócio da Sprint {s["n"]}',
            'Diagramas de Estados referentes ao Modulo X1 (se fizer sentido)': f'Análise de estados relevantes da Sprint {s["n"]}',
            'Interface com o utilizador referente ao Modulo X1': f'Interface com o utilizador da Sprint {s["n"]}',
            'Protótipo (caso exista) ou mock-up em alternativa (detalhado)': 'Ecrãs e fluxos de utilização implementados',
            '(explicar estratégia)': 'A ajuda ao utilizador está distribuída pelas próprias páginas, com validação de campos e mensagens de erro contextuais.',
            '(VSDocMan)': 'A documentação técnica é mantida no Confluence e complementada por README, scripts Docker e configuração por variáveis de ambiente.',
            'Diretamente na plataforma e sensível ao contexto': 'O manual de utilização é orientado ao percurso real do utilizador na aplicação.'
        }
        if txt in replacements:
            set_para_text(p, replacements[txt], bold=p.style.name.startswith('Heading'))
    section_texts={
        'SUMARIO EXECUTIVO': f'A Sprint {s["n"]} teve como objetivo {s["goal"]}. Este documento detalha requisitos implementados, classes, processos de negócio, interface, testes, manual de utilização e manual técnico.',
        'INTRODUÇÃO': f'O incremento da Sprint {s["n"]} foi planeado a partir do backlog Jira e validado por execução local/Docker, seguindo os módulos {s["mods"]}.',
        'Introdução': 'O desenho detalhado descreve como os requisitos da sprint foram implementados e validados na aplicação ASP.NET Core MVC.',
        'Requisitos funcionais': 'Os requisitos abaixo foram considerados no incremento e encontram-se implementados ou operacionalmente suportados na aplicação.',
        'Diagrama de Classes': 'O diagrama representa as entidades persistidas e serviços relevantes para o incremento.',
        'Diagramas de Processos': 'O processo de negócio é representado em BPD/BPMN, usando lanes e fluxos compatíveis com a notação trabalhada na unidade curricular.',
        'Diagramas de Estados': 'Não foi necessário diagrama de estados autónomo para esta sprint; os estados relevantes são persistidos no histórico do veículo, rota, autenticação ou viagem colaborativa.',
        'Interface com o utilizador': 'A interface foi validada diretamente nas views Razor correspondentes, mantendo navegação clara entre as áreas do sistema.',
        'Testes': 'Os testes combinaram validação unitária, integração com serviços, execução manual orientada a cenário e aceitação funcional.',
        'Testes de Automação': 'Os testes automatizados podem ser executados por dotnet test e incidem sobretudo sobre serviços de domínio e estado aplicacional.',
        'Testes de Integração': 'A estratégia adotada foi incremental, validando integrações com base de dados, APIs externas configuráveis e fluxos MVC.',
        'Testes de Sistema': 'Os testes de sistema validam adequação funcional, compatibilidade, segurança, eficiência e capacidade de interação.',
        'Testes de aceitação': 'A aceitação baseia-se na demonstração do fluxo funcional previsto para a sprint e na rastreabilidade entre requisito, implementação e evidência.',
        'Manual de utilização': 'O utilizador inicia sessão, acede ao módulo correspondente, preenche os dados obrigatórios, valida o resultado e guarda/aplica as alterações quando necessário.',
        'Manual técnico': 'Executar dotnet build, dotnet test e docker compose. A configuração externa é feita por variáveis de ambiente e ficheiro .env local.'
    }
    for key,text in section_texts.items():
        p=find_para(doc,key)
        if p: add_paragraphs_after(p,[text])
    h2_intro = find_para_with_style(doc, 'Introdução', 'Heading 2')
    if h2_intro:
        add_paragraphs_after(h2_intro, [
            f'A Sprint {s["n"]} foi detalhada a partir dos requisitos selecionados no backlog, das decisões de desenho tomadas no desenho de alto nível e das evidências implementadas no código ASP.NET Core MVC.'
        ])
    reqs=[r for r in REQS if r[0] in SPRINT_REQS[s['n']]]
    p=find_para(doc,'Requisitos funcionais')
    if p:
        tbl_req = insert_table_after(p,1,5,[(r[0],r[1],r[2],'Implementado',r[3]) for r in reqs], header=['ID','Módulo','Prioridade','Estado','Requisito'])
        make_table_rows_breakable(tbl_req)
        set_table_font_size(tbl_req, 7.5)
    p=find_para(doc,'Diagrama de Classes')
    if p: insert_image_after(p,'classes-dominio.png','Figura 1 - Diagrama de classes de desenho detalhado.',width=6.2)
    p=find_para(doc,'Diagramas de Processos')
    if p: insert_image_after(p,s['bpmn'],f'Figura 2 - BPD da Sprint {s["n"]}.',width=6.2)
    p=find_para(doc,'Diagrama de Navegação')
    if p:
        insert_table_after(p,1,3,[('Login','Perfil','Autenticação válida'),('Perfil','Garagem','Utilizador autenticado'),('Garagem','Planeamento','Veículo selecionado'),('Planeamento','Garagem','Rota aplicada'),('Perfil','Social','Perfil social ativo')], header=['Origem','Destino','Condição'])
    replace_existing_toc_lines(doc, [
        ('1    SUMÁRIO EXECUTIVO ........................................................................ 4', 1),
        ('2    INTRODUÇÃO ..................................................................................... 4', 1),
        ('3    DESENHO DETALHADO .................................................................... 5', 1),
        ('3.1    Introdução ...................................................................................... 5', 2),
        (f'3.2    Módulo(s) {s["mods"]} / Sprint {s["n"]} - {s["name"]} ............. 5', 2),
        ('3.2.1    Requisitos funcionais implementados ......................................... 5', 3),
        (f'3.2.2    Diagrama de classes da Sprint {s["n"]} .................................... 6', 3),
        (f'3.2.3    Processos de negócio da Sprint {s["n"]} .................................. 7', 3),
        (f'3.2.4    Análise de estados relevantes da Sprint {s["n"]} ..................... 7', 3),
        (f'3.2.5    Interface com o utilizador da Sprint {s["n"]} .......................... 7', 3),
        ('3.3    Testes ............................................................................................ 8', 2),
        ('3.3.1    Testes Unitários .......................................................................... 8', 3),
        ('3.3.2    Testes de Automação .................................................................. 8', 3),
        ('3.3.3    Testes de Integração ................................................................... 8', 3),
        ('3.3.4    Testes de Sistema ....................................................................... 8', 3),
        ('3.3.5    Testes de aceitação .................................................................... 9', 3),
        ('3.4    Manual de utilização ..................................................................... 9', 2),
        ('3.5    Manual técnico .............................................................................. 9', 2),
    ])
    out=OUT/f'{2+s["n"]:02d}-OptiDrive-Desenho-Detalhado-Sprint-{s["n"]}.docx'
    doc.save(out); return out


def build_ata(s):
    doc=Document(TEMPLATE_ATA)
    # Header table
    ht=doc.tables[0]
    set_cell(ht.cell(0,2),f'Doc. Nº. ATA-OP-0{s["n"]}')
    set_cell(ht.cell(1,2),'Página(s): 1/')
    set_cell(ht.cell(2,2),'Versão: 1.0')
    t=doc.tables[1]
    set_cell(t.cell(0,1),PROJECT)
    set_cell(t.cell(1,1),s['date'])
    set_cell(t.cell(2,1),'Reunião remota / trabalho local')
    set_cell(t.cell(3,1),'10:00 - 11:30')
    set_cell(t.cell(4,1),'Product Owner / Scrum Master / Desenvolvimento')
    set_cell(t.cell(4,3),AUTHOR)
    set_cell(t.cell(5,1),'Stakeholder académico')
    set_cell(t.cell(5,3),'Docente da unidade curricular')
    pending = 'Sem pendências da reunião anterior.' if s['n']==1 else 'Validação dos entregáveis e correções identificadas na sprint anterior.'
    treated = f'Planeamento e execução da Sprint {s["n"]}: {s["name"]}. Foram revistos requisitos, implementação, testes, evidências Jira/Confluence e riscos.'
    nexts = 'Preparar sprint seguinte, atualizar documentação e continuar validação do produto.' if s['n']<5 else 'Fechar documentação final, validar entrega, preparar defesa e publicação final.'
    nextdate = SPRINTS[s['n']]['date'] if s['n']<5 else 'N.A.'
    set_label_body_cell(t.cell(6,0), 'ASSUNTOS PENDENTES DA REUNIÃO ANTERIOR:', pending)
    set_label_body_cell(t.cell(7,0), 'ASSUNTOS TRATADOS NA REUNIÃO:', treated)
    set_label_body_cell(t.cell(8,0), 'ASSUNTOS A TRATAR PARA A PRÓXIMA REUNIÃO:', nexts)
    set_label_body_cell(t.cell(9,0), 'DATA DA PRÓXIMA REUNIÃO:', nextdate)
    out=OUT/f'{7+s["n"]:02d}-OptiDrive-Ata-Sprint-{s["n"]}.docx'
    doc.save(out); return out

if __name__ == '__main__':
    outputs=[]
    outputs.append(build_aer())
    outputs.append(build_hld())
    for s in SPRINTS: outputs.append(build_dd(s))
    for s in SPRINTS: outputs.append(build_ata(s))
    print('\n'.join(str(p) for p in outputs))
