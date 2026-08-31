#!/usr/bin/env python3
import html as html_lib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path
from urllib.parse import unquote

from lxml import html, etree

sys.path.insert(0, str(Path(__file__).resolve().parent))
from confluence_publish import ConfluenceClient

ROOT = Path.cwd()
DOCX_DIR = ROOT / 'docs' / 'confluence-docx-oficial'
SOFFICE = Path('/Users/alexandremiguel/.cache/codex-runtimes/codex-primary-runtime/dependencies/bin/soffice')

ITEMS = [
    ('OptiDrive - Analise e Especificacao de Requisitos - Oficial', '01-OptiDrive-Analise-Especificacao-Requisitos.docx'),
    ('OptiDrive - Desenho de Alto Nivel - Oficial', '02-OptiDrive-Desenho-Alto-Nivel.docx'),
    ('OptiDrive - Desenho Detalhado Sprint 1 - Oficial', '03-OptiDrive-Desenho-Detalhado-Sprint-1.docx'),
    ('OptiDrive - Desenho Detalhado Sprint 2 - Oficial', '04-OptiDrive-Desenho-Detalhado-Sprint-2.docx'),
    ('OptiDrive - Desenho Detalhado Sprint 3 - Oficial', '05-OptiDrive-Desenho-Detalhado-Sprint-3.docx'),
    ('OptiDrive - Desenho Detalhado Sprint 4 - Oficial', '06-OptiDrive-Desenho-Detalhado-Sprint-4.docx'),
    ('OptiDrive - Desenho Detalhado Sprint 5 - Oficial', '07-OptiDrive-Desenho-Detalhado-Sprint-5.docx'),
    ('OptiDrive - Ata Sprint 1 - Oficial', '08-OptiDrive-Ata-Sprint-1.docx'),
    ('OptiDrive - Ata Sprint 2 - Oficial', '09-OptiDrive-Ata-Sprint-2.docx'),
    ('OptiDrive - Ata Sprint 3 - Oficial', '10-OptiDrive-Ata-Sprint-3.docx'),
    ('OptiDrive - Ata Sprint 4 - Oficial', '11-OptiDrive-Ata-Sprint-4.docx'),
    ('OptiDrive - Ata Sprint 5 - Oficial', '12-OptiDrive-Ata-Sprint-5.docx'),
]

ALLOWED_ATTRS = {
    'table': {'width', 'cellpadding', 'cellspacing', 'style'},
    'td': {'width', 'height', 'style', 'colspan', 'rowspan'},
    'th': {'width', 'height', 'style', 'colspan', 'rowspan'},
    'tr': {'style'},
    'p': {'style'},
    'h1': {'style'}, 'h2': {'style'}, 'h3': {'style'}, 'h4': {'style'}, 'h5': {'style'}, 'h6': {'style'},
    'span': {'style'}, 'div': {'style'},
    'img': {'src', 'width', 'height', 'style', 'alt'},
    'a': {'href'},
}
DROP_TAGS = {'meta', 'style', 'script', 'title', 'link', 'head', 'col', 'colgroup'}
INLINE_TAGS = {'font'}


def merge_css(style, additions):
    parsed = {}
    for part in (style or '').split(';'):
        if ':' not in part:
            continue
        key, value = part.split(':', 1)
        key = key.strip().lower()
        value = value.strip()
        if key and value:
            parsed[key] = value
    parsed.update(additions)
    return '; '.join(f'{k}: {v}' for k, v in parsed.items())


def run_lo_to_html(docx_path: Path, out_dir: Path) -> Path:
    profile = out_dir / '.lo_profile'
    profile.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env['HOME'] = str(profile)
    env['TMPDIR'] = '/private/tmp'
    result = subprocess.run([
        str(SOFFICE), '--headless', f'-env:UserInstallation=file://{profile}',
        '--convert-to', 'html', '--outdir', str(out_dir), str(docx_path)
    ], env=env, text=True, capture_output=True, check=False)
    if result.returncode != 0:
        raise RuntimeError(result.stdout + result.stderr)
    htmls = list(out_dir.glob('*.html'))
    if not htmls:
        raise RuntimeError(f'No HTML generated for {docx_path}')
    return htmls[0]


def unwrap(el):
    parent = el.getparent()
    if parent is None:
        return
    idx = parent.index(el)
    if el.text:
        if idx == 0:
            parent.text = (parent.text or '') + el.text
        else:
            prev = parent[idx-1]
            prev.tail = (prev.tail or '') + el.text
    for child in list(el):
        el.remove(child)
        parent.insert(idx, child)
        idx += 1
    if el.tail:
        if idx == 0:
            parent.text = (parent.text or '') + el.tail
        else:
            prev = parent[idx-1]
            prev.tail = (prev.tail or '') + el.tail
    parent.remove(el)


def sanitize_tree(root):
    # remove comments
    for c in root.xpath('//comment()'):
        c.getparent().remove(c)
    for el in list(root.iter()):
        if not isinstance(el.tag, str):
            continue
        tag = el.tag.lower()
        el.tag = tag
        if tag in DROP_TAGS:
            parent = el.getparent()
            if parent is not None:
                parent.remove(el)
            continue
        if tag in INLINE_TAGS:
            el.tag = 'span'
            tag = 'span'
        # normalize Word/LibreOffice tags
        if tag == 'b':
            el.tag = 'strong'; tag = 'strong'
        if tag == 'i':
            el.tag = 'em'; tag = 'em'
        if tag in {'h1', 'h2', 'h3', 'h4', 'h5', 'h6'}:
            in_table = any((parent.tag if isinstance(parent.tag, str) else '').lower() in {'td', 'th'} for parent in el.iterancestors())
            if in_table:
                el.tag = 'p'
                tag = 'p'
                strong = etree.Element('strong')
                strong.text = el.text
                el.text = None
                for child in list(el):
                    el.remove(child)
                    strong.append(child)
                el.append(strong)
        allowed = ALLOWED_ATTRS.get(tag, set())
        for attr in list(el.attrib):
            if attr not in allowed:
                del el.attrib[attr]
        # remove noisy style declarations that tend to fail/uglify Confluence
        if 'style' in el.attrib:
            style = el.attrib['style']
            # keep useful text/table styling, drop page-break and direction bits
            style = re.sub(r'(^|;)\s*(direction|orphans|widows|page-break-[^:]+|so-language|width|height)\s*:[^;]+;?', ';', style, flags=re.I)
            style = re.sub(r'\s+', ' ', style).strip(' ;')
            if style:
                el.attrib['style'] = style
            else:
                del el.attrib['style']
        if tag == 'table':
            el.attrib.pop('width', None)
            el.attrib['style'] = merge_css(el.attrib.get('style'), {
                'width': '100%',
                'table-layout': 'fixed',
                'border-collapse': 'collapse',
                'margin': '14px 0 18px 0',
            })
        elif tag in {'td', 'th'}:
            el.attrib.pop('width', None)
            el.attrib.pop('height', None)
            el.attrib['style'] = merge_css(el.attrib.get('style'), {
                'padding': '6px 8px',
                'vertical-align': 'top',
                'border': '1px solid #c8d3df',
                'overflow-wrap': 'anywhere',
                'word-break': 'normal',
            })
        elif tag == 'p':
            el.attrib['style'] = merge_css(el.attrib.get('style'), {
                'margin': '0 0 10px 0',
                'line-height': '1.35',
            })
    for p in list(root.xpath('.//td//p | .//th//p')):
        has_image = bool(p.xpath('.//ac:image', namespaces={'ac': 'http://www.atlassian.com/schema/confluence/4/ac/'}))
        text = ''.join(p.itertext()).strip()
        non_break_children = [c for c in p if isinstance(c.tag, str) and c.tag.lower() not in {'br'}]
        if not text and not has_image and not non_break_children:
            parent = p.getparent()
            if parent is not None and len(parent) > 1:
                parent.remove(p)
    return root


def convert_html_to_storage(html_path: Path, page_id: str, client: ConfluenceClient):
    raw = html_path.read_text(encoding='utf-8', errors='ignore')
    doc = html.document_fromstring(raw)
    body = doc.find('body')
    if body is None:
        body = doc
    sanitize_tree(body)
    attachments = []
    for img in body.xpath('.//img'):
        src = unquote(img.attrib.get('src', '')).strip()
        if not src:
            continue
        src_path = (html_path.parent / src).resolve()
        if src_path.exists():
            attachments.append(src_path)
    for path in attachments:
        client.upload_attachment(page_id, path)
    fragments = []
    for child in body:
        if isinstance(child.tag, str) and child.tag in {'meta', 'style', 'script', 'title', 'link'}:
            continue
        xml = etree.tostring(child, encoding='unicode', method='xml')
        fragments.append(xml)
    storage = '\n'.join(fragments)

    def repl_img(match):
        attrs = match.group(0)
        src_m = re.search(r'src="([^"]+)"', attrs)
        if not src_m:
            return ''
        filename = Path(unquote(src_m.group(1))).name
        width_m = re.search(r'width="([0-9]+)"', attrs)
        width = width_m.group(1) if width_m else '760'
        try:
            width_int = max(120, min(760, int(width)))
        except ValueError:
            width_int = 760
        return f'<ac:image ac:align="center" ac:layout="center" ac:width="{width_int}"><ri:attachment ri:filename="{html_lib.escape(filename)}" /></ac:image>'

    storage = re.sub(r'<img\b[^>]*/>', repl_img, storage)
    storage = re.sub(r'<br></br>', '<br />', storage)
    storage = re.sub(r'<hr></hr>', '<hr />', storage)
    # Confluence storage dislikes empty strong/em spans sometimes; leave ordinary content only.
    return storage, [p.name for p in attachments]


def main():
    base = os.environ['CONFLUENCE_BASE_URL']
    email = os.environ['CONFLUENCE_EMAIL']
    token = os.environ['CONFLUENCE_API_TOKEN']
    space = os.environ['CONFLUENCE_SPACE_KEY']
    client = ConfluenceClient(base, email, token)
    results = []
    work_root = Path('/tmp/optidrive_confluence_native_import_html')
    if work_root.exists():
        shutil.rmtree(work_root)
    work_root.mkdir(parents=True)
    for title, docx_name in ITEMS:
        page = client.find_page(space, title)
        if not page:
            results.append({'title': title, 'error': 'page not found'})
            continue
        docx_path = DOCX_DIR / docx_name
        out_dir = work_root / docx_path.stem
        out_dir.mkdir(parents=True, exist_ok=True)
        html_path = run_lo_to_html(docx_path, out_dir)
        storage, image_names = convert_html_to_storage(html_path, page['id'], client)
        # Safety: add no wrapper; this is intended to behave like imported Word content.
        fresh = client.find_page(space, title)
        parent_id = fresh.get('ancestors', [{}])[-1].get('id') if fresh.get('ancestors') else None
        updated = client.update_page(fresh['id'], title, storage, fresh['version']['number'], parent_id=parent_id)
        results.append({
            'title': title,
            'id': fresh['id'],
            'docx': docx_name,
            'html': str(html_path),
            'images': len(image_names),
            'storage_chars': len(storage),
            'webui': updated.get('_links', {}).get('webui'),
        })
    print(json.dumps({'updated': len(results), 'results': results}, ensure_ascii=False, indent=2))

if __name__ == '__main__':
    main()
