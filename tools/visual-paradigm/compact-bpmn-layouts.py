#!/usr/bin/env python3
"""Apply compact, presentation-friendly DI layouts to the OptiDrive BPMN sources."""

from pathlib import Path
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parent / "bpmn-source"
NS = {
    "bpmndi": "http://www.omg.org/spec/BPMN/20100524/DI",
    "dc": "http://www.omg.org/spec/DD/20100524/DC",
    "di": "http://www.omg.org/spec/DD/20100524/DI",
}

for prefix, uri in {
    "bpmn": "http://www.omg.org/spec/BPMN/20100524/MODEL",
    "bpmndi": NS["bpmndi"],
    "dc": NS["dc"],
    "di": NS["di"],
}.items():
    ET.register_namespace(prefix, uri)


LAYOUTS = {
    "bpmn-01-planeamento-rota.bpmn": {
        "shapes": {
            "route_start": (60, 197, 56, 56),
            "route_input": (150, 170, 250, 110),
            "route_validate": (465, 170, 250, 110),
            "route_maps": (780, 170, 225, 110),
            "route_stations": (1070, 170, 275, 110),
            "route_smart": (1410, 170, 260, 110),
            "route_autonomy": (1735, 184, 82, 82),
            "route_present": (1880, 170, 260, 110),
            "route_end": (2200, 197, 56, 56),
            "route_refuel": (1510, 420, 300, 110),
            "route_recalc": (1870, 420, 270, 110),
        },
        "edges": {
            "route_f1": [(116, 225), (150, 225)],
            "route_f2": [(400, 225), (465, 225)],
            "route_f3": [(715, 225), (780, 225)],
            "route_f4": [(1005, 225), (1070, 225)],
            "route_f5": [(1345, 225), (1410, 225)],
            "route_f6": [(1670, 225), (1735, 225)],
            "route_f7": [(1817, 225), (1880, 225)],
            "route_f8": [(2140, 225), (2200, 225)],
            "route_f9": [(1776, 266), (1776, 355), (1660, 355), (1660, 420)],
            "route_f10": [(1810, 475), (1870, 475)],
            "route_f11": [(2005, 420), (2005, 280)],
        },
    },
    "bpmn-02-garagem-veiculo.bpmn": {
        "shapes": {
            "garage_start": (60, 197, 56, 56),
            "garage_select": (150, 170, 280, 110),
            "garage_validate": (500, 170, 240, 110),
            "garage_gateway": (810, 184, 82, 82),
            "garage_save": (960, 170, 240, 110),
            "garage_history": (1270, 170, 260, 110),
            "garage_end": (1600, 197, 56, 56),
            "garage_errors": (570, 420, 250, 110),
        },
        "edges": {
            "garage_f1": [(116, 225), (150, 225)],
            "garage_f2": [(430, 225), (500, 225)],
            "garage_f3": [(740, 225), (810, 225)],
            "garage_f4": [(892, 225), (960, 225)],
            "garage_f5": [(1200, 225), (1270, 225)],
            "garage_f6": [(1530, 225), (1600, 225)],
            "garage_f7": [(851, 266), (851, 360), (695, 360), (695, 420)],
            "garage_f8": [(570, 475), (290, 475), (290, 280)],
        },
    },
    "bpmn-03-social-viagem.bpmn": {
        "shapes": {
            "social_start": (60, 197, 56, 56),
            "social_search": (150, 170, 270, 110),
            "social_notify": (490, 170, 230, 110),
            "social_gateway": (790, 184, 82, 82),
            "social_group": (940, 170, 235, 110),
            "social_chat": (1245, 170, 250, 110),
            "social_follow": (1565, 170, 255, 110),
            "social_end": (1890, 197, 56, 56),
            "social_individual": (940, 420, 250, 110),
        },
        "edges": {
            "social_f1": [(116, 225), (150, 225)],
            "social_f2": [(420, 225), (490, 225)],
            "social_f3": [(720, 225), (790, 225)],
            "social_f4": [(872, 225), (940, 225)],
            "social_f5": [(1175, 225), (1245, 225)],
            "social_f6": [(1495, 225), (1565, 225)],
            "social_f7": [(1820, 225), (1890, 225)],
            "social_f8": [(831, 266), (831, 355), (1065, 355), (1065, 420)],
            "social_f9": [(1190, 475), (1918, 475), (1918, 253)],
        },
    },
    "bpmn-04-devops-entrega.bpmn": {
        "shapes": {
            "devops_start": (60, 197, 56, 56),
            "devops_prepare": (150, 170, 280, 110),
            "devops_build": (500, 170, 270, 110),
            "devops_ok": (840, 184, 82, 82),
            "devops_deploy": (990, 170, 300, 110),
            "devops_health": (1360, 170, 280, 110),
            "devops_package": (1710, 170, 260, 110),
            "devops_end": (2040, 197, 56, 56),
            "devops_fix": (500, 420, 270, 110),
        },
        "edges": {
            "devops_f1": [(116, 225), (150, 225)],
            "devops_f2": [(430, 225), (500, 225)],
            "devops_f3": [(770, 225), (840, 225)],
            "devops_f4": [(922, 225), (990, 225)],
            "devops_f5": [(1290, 225), (1360, 225)],
            "devops_f6": [(1640, 225), (1710, 225)],
            "devops_f7": [(1970, 225), (2040, 225)],
            "devops_f8": [(881, 266), (881, 355), (635, 355), (635, 420)],
            "devops_f9": [(635, 420), (635, 280)],
        },
    },
}


def update(path: Path, config: dict) -> None:
    tree = ET.parse(path)
    root = tree.getroot()
    # Visual Paradigm's BPMN importer expects the conventional xsi declaration,
    # even when no xsi-prefixed attribute is used in the file.
    root.set("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")

    for shape in root.findall(".//bpmndi:BPMNShape", NS):
        element_id = shape.get("bpmnElement")
        if element_id not in config["shapes"]:
            continue
        x, y, width, height = config["shapes"][element_id]
        bounds = shape.find("dc:Bounds", NS)
        bounds.set("x", str(x))
        bounds.set("y", str(y))
        bounds.set("width", str(width))
        bounds.set("height", str(height))

    for edge in root.findall(".//bpmndi:BPMNEdge", NS):
        flow_id = edge.get("bpmnElement")
        if flow_id not in config["edges"]:
            continue
        for waypoint in list(edge.findall("di:waypoint", NS)):
            edge.remove(waypoint)
        for x, y in config["edges"][flow_id]:
            ET.SubElement(edge, f"{{{NS['di']}}}waypoint", {"x": str(float(x)), "y": str(float(y))})

    ET.indent(tree, space="  ")
    tree.write(path, encoding="UTF-8", xml_declaration=True)


def main() -> None:
    for filename, config in LAYOUTS.items():
        update(ROOT / filename, config)
        print(f"Updated {filename}")


if __name__ == "__main__":
    main()
