#!/usr/bin/env python3
"""Generate clean, presentation-ready OptiDrive diagrams from original project data.

These images are independent redraws. They do not remove or conceal third-party
watermarks and must not be represented as licensed Visual Paradigm exports.
"""

from __future__ import annotations

from dataclasses import dataclass
from math import atan2, cos, pi, sin
from pathlib import Path
from typing import Iterable, Sequence

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "docs/reports-oficial/assets/diagrams"
OUT.mkdir(parents=True, exist_ok=True)

INK = "#192B32"
MUTED = "#5F7075"
NAVY = "#184265"
TEAL = "#0C695B"
BLUE = "#4A90B8"
GREEN = "#6CAB8B"
GOLD = "#E0B14B"
LILAC = "#B19ACC"
PALE_BLUE = "#E2EFF7"
PALE_GREEN = "#E0F2E9"
PALE_GOLD = "#FAEFCF"
PALE_LILAC = "#EFE7F7"
PALE_GREY = "#F1F4F5"
GRID = "#DCE4E6"
WHITE = "#FFFFFF"
RED = "#BF5B5B"


FONT_REGULAR_CANDIDATES = [
    "/System/Library/Fonts/Supplemental/Arial.ttf",
    "/System/Library/Fonts/Supplemental/Helvetica.ttf",
    "/Library/Fonts/Arial.ttf",
]
FONT_BOLD_CANDIDATES = [
    "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
    "/System/Library/Fonts/Supplemental/Helvetica Bold.ttf",
    "/Library/Fonts/Arial Bold.ttf",
]


def _first_font(candidates: Sequence[str]) -> str:
    for candidate in candidates:
        if Path(candidate).exists():
            return candidate
    raise FileNotFoundError("No suitable system font was found")


FONT_REGULAR = _first_font(FONT_REGULAR_CANDIDATES)
FONT_BOLD = _first_font(FONT_BOLD_CANDIDATES)


@dataclass
class Box:
    x: int
    y: int
    w: int
    h: int

    @property
    def left(self) -> int:
        return self.x

    @property
    def right(self) -> int:
        return self.x + self.w

    @property
    def top(self) -> int:
        return self.y

    @property
    def bottom(self) -> int:
        return self.y + self.h

    @property
    def cx(self) -> int:
        return self.x + self.w // 2

    @property
    def cy(self) -> int:
        return self.y + self.h // 2


class Canvas:
    def __init__(self, width: int, height: int, scale: int = 2):
        self.width = width
        self.height = height
        self.scale = scale
        self.image = Image.new("RGB", (width * scale, height * scale), WHITE)
        self.draw = ImageDraw.Draw(self.image)
        self._fonts: dict[tuple[int, bool], ImageFont.FreeTypeFont] = {}

    def s(self, value: float) -> int:
        return round(value * self.scale)

    def font(self, size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
        key = (size, bold)
        if key not in self._fonts:
            self._fonts[key] = ImageFont.truetype(FONT_BOLD if bold else FONT_REGULAR, self.s(size))
        return self._fonts[key]

    def rect(self, box: Box, fill: str = WHITE, outline: str = INK, width: int = 2) -> None:
        self.draw.rectangle(
            (self.s(box.left), self.s(box.top), self.s(box.right), self.s(box.bottom)),
            fill=fill,
            outline=outline,
            width=self.s(width),
        )

    def rounded(self, box: Box, radius: int = 18, fill: str = WHITE, outline: str = TEAL, width: int = 2) -> None:
        self.draw.rounded_rectangle(
            (self.s(box.left), self.s(box.top), self.s(box.right), self.s(box.bottom)),
            radius=self.s(radius),
            fill=fill,
            outline=outline,
            width=self.s(width),
        )

    def line(self, points: Sequence[tuple[float, float]], fill: str = INK, width: int = 2, dash: bool = False) -> None:
        pts = [(self.s(x), self.s(y)) for x, y in points]
        if not dash:
            self.draw.line(pts, fill=fill, width=self.s(width), joint="curve")
            return
        for a, b in zip(points, points[1:]):
            x1, y1 = a
            x2, y2 = b
            length = max(abs(x2 - x1), abs(y2 - y1))
            if length == 0:
                continue
            segments = max(1, int(length // 12))
            for i in range(segments):
                if i % 2:
                    continue
                t1 = i / segments
                t2 = min(1, (i + 1) / segments)
                self.draw.line(
                    (self.s(x1 + (x2 - x1) * t1), self.s(y1 + (y2 - y1) * t1),
                     self.s(x1 + (x2 - x1) * t2), self.s(y1 + (y2 - y1) * t2)),
                    fill=fill,
                    width=self.s(width),
                )

    def arrow(
        self,
        points: Sequence[tuple[float, float]],
        fill: str = MUTED,
        width: int = 2,
        label: str | None = None,
        label_at: tuple[int, int] | None = None,
        dashed: bool = False,
        head: bool = True,
    ) -> None:
        self.line(points, fill=fill, width=width, dash=dashed)
        if head:
            (x1, y1), (x2, y2) = points[-2], points[-1]
            angle = atan2(y2 - y1, x2 - x1)
            length = 12
            spread = pi / 7
            p1 = (x2 - length * cos(angle - spread), y2 - length * sin(angle - spread))
            p2 = (x2 - length * cos(angle + spread), y2 - length * sin(angle + spread))
            self.draw.polygon(
                [(self.s(x2), self.s(y2)), (self.s(*p1) if False else self.s(p1[0]), self.s(p1[1])),
                 (self.s(p2[0]), self.s(p2[1]))],
                fill=fill,
            )
        if label:
            if label_at is None:
                x1, y1 = points[len(points) // 2 - 1]
                x2, y2 = points[len(points) // 2]
                label_at = (round((x1 + x2) / 2), round((y1 + y2) / 2 - 18))
            self.label(label, label_at[0], label_at[1], 13, fill=INK, background=WHITE)

    def label(
        self,
        text: str,
        x: int,
        y: int,
        size: int = 15,
        bold: bool = False,
        fill: str = INK,
        anchor: str = "mm",
        background: str | None = None,
        padding: int = 4,
    ) -> None:
        font = self.font(size, bold)
        bbox = self.draw.textbbox((self.s(x), self.s(y)), text, font=font, anchor=anchor)
        if background:
            self.draw.rounded_rectangle(
                (bbox[0] - self.s(padding), bbox[1] - self.s(padding), bbox[2] + self.s(padding), bbox[3] + self.s(padding)),
                radius=self.s(4),
                fill=background,
            )
        self.draw.text((self.s(x), self.s(y)), text, font=font, fill=fill, anchor=anchor)

    def wrap(self, text: str, max_width: int, size: int, bold: bool = False) -> list[str]:
        font = self.font(size, bold)
        lines: list[str] = []
        for paragraph in text.split("\n"):
            words = paragraph.split()
            if not words:
                lines.append("")
                continue
            current = words[0]
            for word in words[1:]:
                candidate = current + " " + word
                width = self.draw.textlength(candidate, font=font) / self.scale
                if width <= max_width:
                    current = candidate
                else:
                    lines.append(current)
                    current = word
            lines.append(current)
        return lines

    def text_box(
        self,
        box: Box,
        text: str,
        size: int = 15,
        bold: bool = False,
        fill: str = INK,
        padding: int = 14,
        align: str = "center",
    ) -> None:
        lines = self.wrap(text, box.w - padding * 2, size, bold)
        line_height = size * 1.25
        total = len(lines) * line_height
        y = box.cy - total / 2 + line_height / 2
        for line in lines:
            x = box.cx if align == "center" else box.left + padding
            anchor = "mm" if align == "center" else "lm"
            self.label(line, round(x), round(y), size, bold, fill, anchor)
            y += line_height

    def card(self, box: Box, text: str, fill: str = PALE_BLUE, outline: str = TEAL, size: int = 15, bold: bool = False) -> None:
        self.rounded(box, 16, fill, outline, 2)
        self.text_box(box, text, size, bold)

    def title(self, text: str, subtitle: str | None = None) -> None:
        self.rounded(Box(45, 35, self.width - 90, 76), 18, NAVY, NAVY, 1)
        self.label(text, 78, 72, 24, True, WHITE, "lm")
        if subtitle:
            self.label(subtitle, self.width - 78, 72, 13, False, "#DDEAF1", "rm")

    def save(self, filename: str) -> Path:
        target = OUT / filename
        downsampled = self.image.resize((self.width, self.height), Image.Resampling.LANCZOS)
        downsampled.save(target, optimize=True)
        return target


def architecture() -> None:
    c = Canvas(1700, 920)
    c.title("Arquitetura geral OptiDrive", "Visão lógica por camadas e integrações")
    boxes = {
        "client": Box(70, 285, 230, 135),
        "web": Box(390, 255, 320, 195),
        "app": Box(800, 255, 350, 195),
        "data": Box(1260, 255, 340, 195),
        "auth": Box(360, 630, 350, 135),
        "maps": Box(780, 630, 370, 135),
        "stations": Box(1220, 630, 380, 135),
    }
    c.card(boxes["client"], "Cliente Web\nDesktop e mobile", PALE_GREEN, GREEN, 17, True)
    c.card(boxes["web"], "Apresentação\nASP.NET Core MVC\nRazor Views + Controllers", PALE_BLUE, BLUE, 17, True)
    c.card(boxes["app"], "Aplicação e domínio\nGaragem · Rotas · Smart Save\nSocial · Administração", PALE_GREEN, GREEN, 17, True)
    c.card(boxes["data"], "Persistência\nEntity Framework Core\nSQLite / Azure Storage", PALE_GOLD, GOLD, 17, True)
    c.card(boxes["auth"], "Identidade externa\nGoogle e Microsoft OAuth", PALE_LILAC, LILAC, 16, True)
    c.card(boxes["maps"], "Google Maps Platform\nGeocoding, Directions e mapa", PALE_LILAC, LILAC, 16, True)
    c.card(boxes["stations"], "OpenChargeMap / fontes de postos\nCarregadores, combustíveis e preços", PALE_LILAC, LILAC, 16, True)
    c.arrow([(boxes["client"].right, boxes["client"].cy), (boxes["web"].left, boxes["web"].cy)], label="HTTPS")
    c.arrow([(boxes["web"].right, boxes["web"].cy), (boxes["app"].left, boxes["app"].cy)], label="pedidos / DTOs")
    c.arrow([(boxes["app"].right, boxes["app"].cy), (boxes["data"].left, boxes["data"].cy)], label="repositórios")
    c.arrow([(boxes["web"].cx, boxes["web"].bottom), (boxes["web"].cx, boxes["auth"].top)], label="OAuth", label_at=(boxes["web"].cx + 40, 540))
    c.arrow([(boxes["app"].cx, boxes["app"].bottom), (boxes["app"].cx, boxes["maps"].top)], label="rotas", label_at=(boxes["app"].cx + 35, 540))
    c.arrow([(boxes["app"].right - 30, boxes["app"].bottom), (boxes["app"].right - 30, 545), (boxes["stations"].cx, 545), (boxes["stations"].cx, boxes["stations"].top)], label="postos / energia", label_at=(1340, 525))
    c.card(Box(390, 815, 1010, 55), "Separação de responsabilidades: apresentação → aplicação/domínio → persistência; integrações externas isoladas por clientes próprios.", PALE_GREY, GRID, 14)
    c.save("arquitetura-geral.png")


def burndown() -> None:
    c = Canvas(1600, 900)
    c.title("Burndown global documentado", "Âmbito consolidado dos oito incrementos")
    left, top, right, bottom = 180, 180, 1390, 690
    max_value = 180
    for value in [0, 40, 80, 120, 160, 180]:
        y = bottom - (value / max_value) * (bottom - top)
        c.line([(left, y), (right, y)], GRID, 1)
        c.label(str(value), left - 30, round(y), 14, False, MUTED, "rm")
    c.line([(left, top), (left, bottom), (right, bottom)], INK, 2)
    values = [177, 159, 137, 113, 92, 72, 44, 16, 0]
    labels = ["Início", "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8"]
    points = []
    for i, value in enumerate(values):
        x = left + 30 + i * 145
        y = bottom - (value / max_value) * (bottom - top)
        points.append((x, y))
        c.label(labels[i], x, bottom + 42, 15, False, INK)
    c.line(points, TEAL, 5)
    for (x, y), value in zip(points, values):
        c.draw.ellipse((c.s(x - 10), c.s(y - 10), c.s(x + 10), c.s(y + 10)), fill=TEAL, outline=WHITE, width=c.s(3))
        c.label(str(value), round(x), round(y - 28), 14, True, TEAL)
    c.label("Story points restantes", 55, 435, 15, True, INK, "lm")
    c.card(Box(220, 755, 1160, 78), "Visão consolidada do âmbito entregue. Os Burndowns automáticos de cada sprint permanecem no Jira; o Sprint 8 inclui eventos de remoção e reinserção sem alteração líquida do âmbito.", PALE_GREEN, GREEN, 14)
    c.save("burndown-geral.png")


def velocity() -> None:
    c = Canvas(1600, 900)
    c.title("Velocity documentada", "Pontos concluídos por sprint")
    left, top, right, bottom = 170, 180, 1390, 690
    for value in [0, 10, 20, 30, 40]:
        y = bottom - value / 40 * (bottom - top)
        c.line([(left, y), (right, y)], GRID, 1)
        c.label(str(value), left - 25, round(y), 14, False, MUTED, "rm")
    c.line([(left, top), (left, bottom), (right, bottom)], INK, 2)
    values = [18, 22, 24, 21, 20, 28, 28, 36]
    for i, value in enumerate(values):
        x = 205 + i * 150
        height = value / 40 * (bottom - top)
        box = Box(x, round(bottom - height), 92, round(height))
        fill, stroke = (PALE_BLUE, BLUE) if i < 5 else (PALE_GREEN, GREEN)
        c.rounded(box, 10, fill, stroke, 2)
        c.label(f"{value} pts", box.cx, box.top - 24, 14, True, INK)
        c.label(f"S{i + 1}", box.cx, bottom + 42, 15, False, INK)
    c.card(Box(190, 755, 350, 70), "S1-S5: estimativas documentadas", PALE_BLUE, BLUE, 14, True)
    c.card(Box(575, 755, 350, 70), "S6-S8: resultados Jira", PALE_GREEN, GREEN, 14, True)
    c.card(Box(960, 755, 430, 70), "Média consolidada: 22,1 pts/sprint", PALE_GOLD, GOLD, 14, True)
    c.save("velocity-geral.png")


def gantt() -> None:
    c = Canvas(1800, 1160)
    c.title("Plano temporal consolidado do projeto OptiDrive", "11/05/2026 — 08/09/2026")
    labels = [
        "Sprint 1 — base, autenticação e garagem",
        "Sprint 2 — mapas, postos e energia",
        "Sprint 3 — Smart Save e autonomia",
        "Sprint 4 — social e viagens em grupo",
        "Sprint 5 — administração, DevOps e entrega",
        "Consolidação documental e técnica",
        "Sprint 6 — melhorias de código e UI",
        "Sprint 7 — polimento e estabilidade",
        "Sprint 8 — auditoria final e aceitação",
    ]
    periods = ["11-20/05", "21-31/05", "01-08/06", "09-16/06", "17-30/06", "Jul-Ago", "29-31/08", "03-08/09"]
    spans = [(0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 2), (6, 1), (6, 1), (7, 1)]
    colors = [
        (PALE_BLUE, BLUE), (PALE_GREEN, GREEN), (PALE_GOLD, GOLD),
        (PALE_LILAC, LILAC), (PALE_BLUE, BLUE), (PALE_GREY, MUTED),
        (PALE_GREEN, GREEN), (PALE_LILAC, LILAC), (PALE_GOLD, GOLD),
    ]
    label_x, label_w = 55, 480
    timeline_x, col_w = 555, 145
    c.rect(Box(label_x, 145, label_w, 58), PALE_GREY, GRID, 1)
    c.label("Atividade", label_x + 18, 174, 15, True, INK, "lm")
    for i, period in enumerate(periods):
        c.rect(Box(timeline_x + i * col_w, 145, col_w, 58), PALE_BLUE, GRID, 1)
        c.label(period, timeline_x + i * col_w + col_w // 2, 174, 13, True, INK)
    for row, label in enumerate(labels):
        y = 220 + row * 92
        c.rect(Box(label_x, y, label_w, 65), PALE_GREY, GRID, 1)
        c.label(label, label_x + 18, y + 33, 14, False, INK, "lm")
        for i in range(len(periods)):
            c.rect(Box(timeline_x + i * col_w, y, col_w, 65), WHITE, GRID, 1)
        start, length = spans[row]
        fill, stroke = colors[row]
        c.rounded(Box(timeline_x + start * col_w + 8, y + 11, length * col_w - 16, 43), 10, fill, stroke, 2)
    c.card(Box(1120, 1065, 560, 55), "Marco final — entrega validada em 08/09/2026", PALE_GOLD, GOLD, 15, True)
    c.save("gantt-oficial.png")


def actor(c: Canvas, x: int, y: int, name: str) -> Box:
    c.draw.ellipse((c.s(x - 15), c.s(y), c.s(x + 15), c.s(y + 30)), outline=TEAL, width=c.s(3))
    c.line([(x, y + 30), (x, y + 82)], TEAL, 3)
    c.line([(x - 32, y + 48), (x + 32, y + 48)], TEAL, 3)
    c.line([(x, y + 82), (x - 28, y + 118)], TEAL, 3)
    c.line([(x, y + 82), (x + 28, y + 118)], TEAL, 3)
    c.label(name, x, y + 145, 14, True, INK)
    return Box(x - 40, y, 80, 120)


def use_case(c: Canvas, box: Box, text: str) -> None:
    c.draw.ellipse((c.s(box.left), c.s(box.top), c.s(box.right), c.s(box.bottom)), fill=PALE_BLUE, outline=TEAL, width=c.s(2))
    c.text_box(box, text, 14, False, INK, 20)


def use_cases() -> None:
    c = Canvas(1780, 1120)
    c.title("Diagrama geral de casos de utilização", "Sistema OptiDrive")
    boundary = Box(280, 125, 1220, 900)
    c.rounded(boundary, 14, "#FBFDFD", MUTED, 2)
    c.label("OptiDrive", boundary.left + 20, boundary.top + 20, 16, True, TEAL, "lm", WHITE)

    actors = {
        "visitor": actor(c, 105, 165, "Visitante"),
        "driver": actor(c, 105, 455, "Condutor autenticado"),
        "social": actor(c, 105, 820, "Participante social"),
        "admin": actor(c, 1665, 165, "Administrador"),
        "maps": actor(c, 1665, 455, "Google Maps"),
        "ocm": actor(c, 1665, 735, "OpenChargeMap / Postos"),
    }
    uses = {
        "uc01": Box(365, 165, 270, 95), "uc02": Box(365, 300, 270, 95),
        "uc11": Box(1110, 165, 270, 95), "uc12": Box(1110, 300, 270, 95),
        "uc03": Box(365, 455, 270, 95), "uc07": Box(365, 590, 270, 95),
        "uc04": Box(745, 455, 270, 95), "uc06": Box(745, 590, 270, 95),
        "uc05": Box(1110, 455, 270, 95),
        "uc08": Box(365, 790, 270, 95), "uc09": Box(745, 790, 270, 95),
        "uc10": Box(1110, 790, 270, 95),
    }
    labels = {
        "uc01": "UC01 Criar conta", "uc02": "UC02 Iniciar sessão / MFA",
        "uc11": "UC11 Gerir backoffice", "uc12": "UC12 Consultar readiness e métricas",
        "uc03": "UC03 Gerir garagem", "uc07": "UC07 Aplicar viagem ao veículo",
        "uc04": "UC04 Planear rota", "uc06": "UC06 Calcular Smart Save",
        "uc05": "UC05 Consultar postos e energia", "uc08": "UC08 Gerir perfil social",
        "uc09": "UC09 Trocar mensagens e contactos", "uc10": "UC10 Gerir viagem colaborativa",
    }
    for key, box in uses.items():
        use_case(c, box, labels[key])
    for points in [
        [(145, 205), (uses["uc01"].left, uses["uc01"].cy)],
        [(145, 225), (250, 225), (250, uses["uc02"].cy), (uses["uc02"].left, uses["uc02"].cy)],
        [(145, 500), (uses["uc03"].left, uses["uc03"].cy)],
        [(145, 520), (245, 520), (245, uses["uc07"].cy), (uses["uc07"].left, uses["uc07"].cy)],
        [(145, 860), (uses["uc08"].left, uses["uc08"].cy)],
        [(145, 880), (250, 880), (250, 920), (690, 920), (690, uses["uc09"].cy), (uses["uc09"].left, uses["uc09"].cy)],
        [(1630, 205), (uses["uc11"].right, uses["uc11"].cy)],
        [(1630, 225), (1535, 225), (1535, uses["uc12"].cy), (uses["uc12"].right, uses["uc12"].cy)],
        [(1630, 500), (uses["uc05"].right, uses["uc05"].cy)],
        [(1630, 520), (1460, 520), (1460, 410), (uses["uc04"].cx, 410), (uses["uc04"].cx, uses["uc04"].top)],
        [(1630, 790), (1460, 790), (1460, uses["uc05"].cy), (uses["uc05"].right, uses["uc05"].cy)],
        [(145, 900), (270, 900), (270, 975), (uses["uc10"].cx, 975), (uses["uc10"].cx, uses["uc10"].bottom)],
    ]:
        c.arrow(points, MUTED, 2, head=False)
    c.arrow([(uses["uc04"].right, uses["uc04"].cy), (uses["uc05"].left, uses["uc05"].cy)], TEAL, 2, "«include»", dashed=True)
    c.arrow([(uses["uc04"].cx, uses["uc04"].bottom), (uses["uc06"].cx, uses["uc06"].top)], TEAL, 2, "«include»", label_at=(uses["uc04"].cx + 55, 570), dashed=True)
    c.arrow([(uses["uc10"].left, uses["uc10"].cy), (uses["uc09"].right, uses["uc09"].cy)], TEAL, 2, "«include»", dashed=True)
    c.save("uml-use-cases-geral.png")


def package_box(c: Canvas, box: Box, text: str, fill: str) -> None:
    tab = Box(box.x, box.y, round(box.w * 0.45), 34)
    c.rounded(tab, 8, fill, TEAL, 2)
    c.rect(Box(box.x, box.y + 28, box.w, box.h - 28), fill, TEAL, 2)
    c.text_box(Box(box.x, box.y + 28, box.w, box.h - 28), text, 15, True)


def packages() -> None:
    c = Canvas(1660, 820)
    c.title("Pacotes lógicos do OptiDrive", "Dependências entre módulos")
    boxes = {
        "m1": Box(70, 170, 280, 135), "m2": Box(440, 170, 250, 135),
        "m3": Box(790, 170, 270, 135), "m4": Box(1160, 170, 300, 135),
        "m6": Box(250, 540, 270, 135), "m5": Box(690, 540, 270, 135),
        "m7": Box(1130, 540, 330, 135),
    }
    labels = {
        "m1": "M01 Autenticação e Perfil", "m2": "M02 Garagem", "m3": "M03 Planeamento",
        "m4": "M04 Postos e Energia", "m5": "M05 Smart Save", "m6": "M06 Social",
        "m7": "M07 Administração e DevOps",
    }
    for i, (key, box) in enumerate(boxes.items()):
        package_box(c, box, labels[key], PALE_BLUE if i < 4 else PALE_GREEN)
    c.arrow([(boxes["m1"].right, boxes["m1"].cy), (boxes["m2"].left, boxes["m2"].cy)], label="utilizador", dashed=True)
    c.arrow([(boxes["m2"].right, boxes["m2"].cy), (boxes["m3"].left, boxes["m3"].cy)], label="veículo", dashed=True)
    c.arrow([(boxes["m3"].right, boxes["m3"].cy), (boxes["m4"].left, boxes["m4"].cy)], label="consulta", dashed=True)
    c.arrow([(boxes["m3"].cx, boxes["m3"].bottom), (boxes["m3"].cx, boxes["m5"].top)], label="cálculo", label_at=(boxes["m3"].cx + 45, 430), dashed=True)
    c.arrow([(boxes["m6"].cx, boxes["m6"].top), (boxes["m6"].cx, 420), (boxes["m3"].left + 45, 420), (boxes["m3"].left + 45, boxes["m3"].bottom)], label="viagem", label_at=(610, 400), dashed=True)
    c.arrow([(boxes["m7"].left, boxes["m7"].cy), (1080, boxes["m7"].cy), (1080, 720), (110, 720), (110, boxes["m1"].bottom)], label="operação", label_at=(600, 700), dashed=True)
    c.save("uml-pacotes-logicos.png")


def class_box(c: Canvas, box: Box, name: str, attrs: Iterable[str]) -> None:
    c.rounded(box, 10, WHITE, TEAL, 2)
    header_h = 42
    c.rounded(Box(box.x, box.y, box.w, header_h), 10, TEAL, TEAL, 1)
    c.rect(Box(box.x, box.y + 28, box.w, 14), TEAL, TEAL, 1)
    c.label(name, box.cx, box.y + 22, 16, True, WHITE)
    y = box.y + 62
    for attr in attrs:
        c.label("+ " + attr, box.x + 16, y, 13, False, INK, "lm")
        y += 24


def classes() -> None:
    c = Canvas(1650, 1070)
    c.title("Modelo de classes do domínio", "Entidades principais e associações")
    boxes = {
        "profile": Box(70, 150, 280, 190), "user": Box(610, 130, 300, 220),
        "route": Box(70, 430, 300, 230), "vehicle": Box(610, 430, 300, 230),
        "activity": Box(1140, 430, 300, 230), "station": Box(70, 770, 300, 220),
        "trip": Box(610, 770, 310, 215), "message": Box(1140, 770, 300, 205),
    }
    data = {
        "profile": ("SocialProfile", ["DisplayName", "Bio", "City", "PhotoUrl"]),
        "user": ("UserAccount", ["Id", "Name", "Email", "Role", "MfaEnabled"]),
        "route": ("PlannedRoute", ["Origin", "Destination", "DistanceKm", "EstimatedCost", "Stops"]),
        "vehicle": ("VehicleProfile", ["Brand", "Model", "FuelKind", "Level", "Consumption"]),
        "activity": ("VehicleActivity", ["Type", "Amount", "Cost", "LevelAfter", "CreatedAt"]),
        "station": ("FuelStation", ["Brand", "FuelKind", "Price", "Latitude", "Longitude"]),
        "trip": ("CollaborativeTrip", ["Members", "Status", "SplitAmount", "StartedAt"]),
        "message": ("DirectMessage", ["Thread", "Body", "CreatedAt", "RelatedTrip"]),
    }
    for key, box in boxes.items():
        class_box(c, box, data[key][0], data[key][1])
    relations = [
        ([(boxes["profile"].right, boxes["profile"].cy), (boxes["user"].left, boxes["user"].cy)], "1 : 1"),
        ([(boxes["user"].cx, boxes["user"].bottom), (boxes["vehicle"].cx, boxes["vehicle"].top)], "1 : N"),
        ([(boxes["user"].left, boxes["user"].bottom - 35), (480, boxes["user"].bottom - 35), (480, boxes["route"].top + 35), (boxes["route"].right, boxes["route"].top + 35)], "planeia"),
        ([(boxes["vehicle"].right, boxes["vehicle"].cy), (boxes["activity"].left, boxes["activity"].cy)], "1 : N"),
        ([(boxes["route"].cx, boxes["route"].bottom), (boxes["station"].cx, boxes["station"].top)], "sugere"),
        ([(boxes["route"].right, boxes["route"].bottom - 35), (480, boxes["route"].bottom - 35), (480, boxes["trip"].top + 35), (boxes["trip"].left, boxes["trip"].top + 35)], "origina"),
        ([(boxes["trip"].right, boxes["trip"].cy), (boxes["message"].left, boxes["message"].cy)], "1 : N"),
    ]
    for points, label in relations:
        c.arrow(points, MUTED, 2, label, head=False)
    c.save("classes-dominio.png")


def component_box(c: Canvas, box: Box, text: str, fill: str) -> None:
    c.rounded(box, 12, fill, TEAL, 2)
    notch_x = box.right - 28
    c.rect(Box(notch_x, box.y + 20, 18, 10), WHITE, TEAL, 2)
    c.rect(Box(notch_x, box.y + 38, 18, 10), WHITE, TEAL, 2)
    c.text_box(Box(box.x, box.y, box.w - 28, box.h), text, 14, True)


def components() -> None:
    c = Canvas(1840, 1040)
    c.title("Diagrama de componentes", "Responsabilidades e dependências técnicas")
    ui = Box(650, 120, 500, 130)
    services = [
        Box(60, 390, 300, 135), Box(410, 390, 300, 135), Box(760, 390, 310, 135),
        Box(1120, 390, 310, 135), Box(1480, 390, 300, 135),
    ]
    db = Box(400, 790, 420, 140)
    external = Box(1030, 790, 470, 140)
    component_box(c, ui, "OptiDrive.Web\nControllers e Razor Views", PALE_BLUE)
    labels = [
        "AppStateService\nSessão, tema e idioma", "GarageService\nVeículos e histórico",
        "RoutePlanningService\nRotas e autonomia", "SocialService\nContactos, chat e viagens",
        "AdminHealthService\nReadiness e métricas",
    ]
    for box, label in zip(services, labels):
        component_box(c, box, label, PALE_GREEN)
    component_box(c, db, "OptiDriveDbContext\nEntity Framework Core + SQLite", PALE_GOLD)
    component_box(c, external, "ExternalApiClients\nGoogle Maps · OpenChargeMap · OAuth", PALE_LILAC)
    hub_y = 315
    c.line([(ui.cx, ui.bottom), (ui.cx, hub_y)], MUTED, 2)
    c.line([(services[0].cx, hub_y), (services[-1].cx, hub_y)], MUTED, 2)
    for box in services:
        c.arrow([(box.cx, hub_y), (box.cx, box.top)], MUTED, 2)
    c.arrow([(services[1].cx, services[1].bottom), (services[1].cx, 680), (db.cx, 680), (db.cx, db.top)], label="persistência")
    c.arrow([(services[3].cx, services[3].bottom), (services[3].cx, 710), (db.right - 60, 710), (db.right - 60, db.top)], label="mensagens")
    c.arrow([(services[2].cx, services[2].bottom), (services[2].cx, 650), (external.left + 100, 650), (external.left + 100, external.top)], label="APIs externas")
    c.arrow([(services[0].cx, services[0].bottom), (services[0].cx, 620), (external.cx, 620), (external.cx, external.top)], label="OAuth", label_at=(920, 600))
    c.save("componentes.png")


def node_box(c: Canvas, box: Box, text: str, fill: str, artifact: bool = False) -> None:
    if artifact:
        c.rect(box, fill, TEAL, 2)
        fold = 22
        c.line([(box.right - fold, box.top), (box.right - fold, box.top + fold), (box.right, box.top + fold)], TEAL, 2)
    else:
        c.rounded(box, 8, fill, TEAL, 2)
        c.line([(box.x + 12, box.y + 12), (box.x + 24, box.y), (box.right + 12, box.y), (box.right, box.y + 12)], TEAL, 2)
        c.line([(box.right, box.y + 12), (box.right + 12, box.y + 12), (box.right + 12, box.bottom - 12), (box.right, box.bottom)], TEAL, 2)
    c.text_box(box, text, 15, True)


def deployment() -> None:
    c = Canvas(1740, 900)
    c.title("Diagrama de deployment", "Ambientes, artefactos e serviços externos")
    browser = Box(60, 330, 270, 150)
    github = Box(400, 130, 290, 155)
    azure = Box(790, 300, 350, 210)
    artifact = Box(850, 355, 230, 100)
    db = Box(1250, 155, 320, 160)
    external = Box(1250, 575, 350, 170)
    node_box(c, browser, "Cliente\nBrowser desktop / mobile", PALE_GREEN)
    node_box(c, github, "GitHub\nRepositório e Actions", PALE_LILAC)
    node_box(c, azure, "Azure App Service\nLinux · .NET 8", PALE_BLUE)
    node_box(c, artifact, "OptiDrive.Web\nArtefacto publicado", WHITE, True)
    node_box(c, db, "Persistência\nSQLite / armazenamento", PALE_GOLD)
    node_box(c, external, "Serviços externos\nGoogle Maps · OCM · OAuth", PALE_LILAC)
    c.arrow([(browser.right, browser.cy), (azure.left, azure.cy)], label="HTTPS")
    c.arrow([(github.right, github.cy), (760, github.cy), (760, azure.top + 30), (azure.left, azure.top + 30)], label="CI/CD")
    c.arrow([(azure.right, azure.top + 45), (1200, azure.top + 45), (1200, db.cy), (db.left, db.cy)], label="leitura / escrita")
    c.arrow([(azure.right, azure.bottom - 45), (1200, azure.bottom - 45), (1200, external.cy), (external.left, external.cy)], label="HTTPS / JSON")
    c.card(Box(520, 790, 700, 55), "O mesmo artefacto é validado em build/test e publicado no Azure App Service.", PALE_GREY, GRID, 14)
    c.save("deployment.png")


def event(c: Canvas, x: int, y: int, label: str, end: bool = False) -> Box:
    r = 28
    c.draw.ellipse((c.s(x - r), c.s(y - r), c.s(x + r), c.s(y + r)), fill=WHITE, outline=TEAL if not end else RED, width=c.s(4 if end else 3))
    if end:
        c.draw.ellipse((c.s(x - r + 7), c.s(y - r + 7), c.s(x + r - 7), c.s(y + r - 7)), outline=RED, width=c.s(2))
    c.label(label, x, y + 52, 13, True, INK)
    return Box(x - r, y - r, r * 2, r * 2)


def task(c: Canvas, box: Box, text: str, kind: str = "task") -> None:
    fill, stroke = (PALE_GREEN, GREEN) if kind == "user" else ((PALE_LILAC, LILAC) if kind == "service" else (PALE_BLUE, BLUE))
    c.rounded(box, 14, fill, stroke, 2)
    icon = "U" if kind == "user" else ("S" if kind == "service" else "")
    if icon:
        c.draw.ellipse((c.s(box.x + 12), c.s(box.y + 12), c.s(box.x + 36), c.s(box.y + 36)), fill=stroke)
        c.label(icon, box.x + 24, box.y + 24, 11, True, WHITE)
    c.text_box(Box(box.x + 10, box.y + 4, box.w - 20, box.h - 8), text, 14, True)


def gateway(c: Canvas, x: int, y: int, text: str) -> Box:
    r = 38
    points = [(x, y - r), (x + r, y), (x, y + r), (x - r, y)]
    c.draw.polygon([(c.s(px), c.s(py)) for px, py in points], fill=PALE_GOLD, outline=GOLD)
    c.line([(x - 13, y - 13), (x + 13, y + 13)], GOLD, 3)
    c.line([(x + 13, y - 13), (x - 13, y + 13)], GOLD, 3)
    c.label(text, x, y + 60, 13, True, INK)
    return Box(x - r, y - r, r * 2, r * 2)


def bpmn_base(width: int, title: str) -> Canvas:
    c = Canvas(width, 760)
    c.title(title, "BPMN 2.0 — fluxo principal e exceção")
    c.rounded(Box(45, 145, width - 90, 535), 10, "#FCFEFE", MUTED, 2)
    c.rect(Box(45, 145, width - 90, 55), PALE_GREY, MUTED, 1)
    c.label("Processo OptiDrive", 75, 173, 15, True, TEAL, "lm")
    c.card(Box(70, 700, 610, 38), "Legenda: verde = utilizador · lilás = serviço · azul = tarefa interna", WHITE, GRID, 12)
    return c


def bpmn_route() -> None:
    c = bpmn_base(2300, "BPMN — Planeamento de rota")
    y = 320
    start = event(c, 135, y, "Necessidade de viagem")
    boxes = [
        Box(220, 265, 250, 110), Box(525, 265, 245, 110), Box(825, 265, 220, 110),
        Box(1100, 265, 260, 110), Box(1415, 265, 245, 110),
    ]
    task(c, boxes[0], "Introduzir origem, destino e veículo", "user")
    task(c, boxes[1], "Validar dados do veículo e nível atual")
    task(c, boxes[2], "Geocoding e Directions", "service")
    task(c, boxes[3], "Consultar postos e carregadores", "service")
    task(c, boxes[4], "Calcular custo, autonomia e Smart Save")
    gate = gateway(c, 1745, y, "Autonomia suficiente?")
    present = Box(1840, 265, 270, 110)
    task(c, present, "Apresentar itinerário, reforços e gasto total")
    end = event(c, 2200, y, "Rota pronta", True)
    chain = [start] + boxes + [gate, present, end]
    for a, b in zip(chain, chain[1:]):
        c.arrow([(a.right, a.cy), (b.left, b.cy)])
    refuel = Box(1460, 505, 300, 105)
    recalc = Box(1820, 505, 290, 105)
    task(c, refuel, "Sugerir paragem de combustível ou carregamento")
    task(c, recalc, "Recalcular autonomia prevista à chegada")
    c.arrow([(gate.cx, gate.bottom), (gate.cx, 455), (refuel.cx, 455), (refuel.cx, refuel.top)], label="não", label_at=(gate.cx + 25, 430))
    c.arrow([(refuel.right, refuel.cy), (recalc.left, recalc.cy)])
    c.arrow([(recalc.cx, recalc.top), (recalc.cx, present.bottom)])
    c.label("sim", gate.right + 25, gate.cy - 18, 13, True, TEAL)
    c.save("bpmn-01-planeamento-rota.png")


def bpmn_garage() -> None:
    c = bpmn_base(1850, "BPMN — Registo de veículo na garagem")
    y = 320
    start = event(c, 135, y, "Veículo a registar")
    select = Box(225, 265, 280, 110)
    validate = Box(565, 265, 240, 110)
    gate = gateway(c, 890, y, "Dados válidos?")
    save = Box(990, 265, 240, 110)
    history = Box(1290, 265, 270, 110)
    end = event(c, 1700, y, "Garagem atualizada", True)
    task(c, select, "Selecionar marca, modelo e combustível", "user")
    task(c, validate, "Validar dados obrigatórios")
    task(c, save, "Guardar perfil do veículo")
    task(c, history, "Registar nível inicial e histórico")
    chain = [start, select, validate, gate, save, history, end]
    for a, b in zip(chain, chain[1:]):
        c.arrow([(a.right, a.cy), (b.left, b.cy)])
    errors = Box(565, 505, 260, 105)
    task(c, errors, "Mostrar erros ao utilizador")
    c.arrow([(gate.cx, gate.bottom), (gate.cx, 455), (errors.cx, 455), (errors.cx, errors.top)], label="não", label_at=(gate.cx + 25, 430))
    c.arrow([(errors.left, errors.cy), (select.cx, errors.cy), (select.cx, select.bottom)], label="corrigir", label_at=(390, 530))
    c.label("sim", gate.right + 24, gate.cy - 18, 13, True, TEAL)
    c.save("bpmn-02-garagem-veiculo.png")


def bpmn_social() -> None:
    c = bpmn_base(2050, "BPMN — Viagem colaborativa")
    y = 320
    start = event(c, 135, y, "Condutor cria viagem")
    search = Box(225, 265, 260, 110)
    notify = Box(545, 265, 230, 110)
    gate = gateway(c, 860, y, "Participantes aceitam?")
    group = Box(960, 265, 235, 110)
    chat = Box(1255, 265, 250, 110)
    follow = Box(1565, 265, 260, 110)
    end = event(c, 1940, y, "Viagem concluída", True)
    task(c, search, "Pesquisar pessoas e enviar convite", "user")
    task(c, notify, "Notificar participantes")
    task(c, group, "Criar viagem em grupo")
    task(c, chat, "Ativar chat e partilha de custos")
    task(c, follow, "Acompanhar viagem colaborativa")
    chain = [start, search, notify, gate, group, chat, follow, end]
    for a, b in zip(chain, chain[1:]):
        c.arrow([(a.right, a.cy), (b.left, b.cy)])
    individual = Box(960, 505, 255, 105)
    task(c, individual, "Manter viagem individual")
    c.arrow([(gate.cx, gate.bottom), (gate.cx, 455), (individual.cx, 455), (individual.cx, individual.top)], label="não", label_at=(gate.cx + 25, 430))
    c.arrow([(individual.right, individual.cy), (end.cx, individual.cy), (end.cx, end.bottom)])
    c.label("sim", gate.right + 24, gate.cy - 18, 13, True, TEAL)
    c.save("bpmn-03-social-viagem.png")


def bpmn_devops() -> None:
    c = bpmn_base(2150, "BPMN — Entrega e operação DevOps")
    y = 320
    start = event(c, 135, y, "Necessidade de publicar")
    prepare = Box(225, 265, 280, 110)
    build = Box(565, 265, 270, 110)
    gate = gateway(c, 920, y, "Build e testes OK?")
    deploy = Box(1020, 265, 300, 110)
    health = Box(1380, 265, 280, 110)
    package = Box(1720, 265, 260, 110)
    end = event(c, 2070, y, "Entrega validada", True)
    task(c, prepare, "Preparar configuração local e Azure", "user")
    task(c, build, "Executar build, testes e validação", "service")
    task(c, deploy, "Publicar aplicação e atualizar documentação", "service")
    task(c, health, "Verificar healthcheck, admin e métricas")
    task(c, package, "Gerar pacote final de entrega")
    chain = [start, prepare, build, gate, deploy, health, package, end]
    for a, b in zip(chain, chain[1:]):
        c.arrow([(a.right, a.cy), (b.left, b.cy)])
    fix = Box(565, 505, 270, 105)
    task(c, fix, "Corrigir defeitos e repetir validação")
    c.arrow([(gate.cx, gate.bottom), (gate.cx, 455), (fix.cx, 455), (fix.cx, fix.top)], label="não", label_at=(gate.cx + 25, 430))
    c.arrow([(fix.cx, fix.top), (fix.cx, build.bottom)])
    c.label("sim", gate.right + 24, gate.cy - 18, 13, True, TEAL)
    c.save("bpmn-04-devops-entrega.png")


def main() -> None:
    generators = [
        architecture, burndown, velocity, gantt, use_cases, packages, classes,
        components, deployment, bpmn_route, bpmn_garage, bpmn_social, bpmn_devops,
    ]
    for generator in generators:
        generator()
        print(f"Generated {generator.__name__}")


if __name__ == "__main__":
    main()
