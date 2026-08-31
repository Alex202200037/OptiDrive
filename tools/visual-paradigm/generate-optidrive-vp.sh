#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VP_APP="${VP_APP:-/Applications/Visual Paradigm.app}"
VP_RES="$VP_APP/Contents/Resources"
VP_APP_RES="$VP_RES/app"
VP_JAVA="$VP_RES/jre.bundle/Contents/Home/bin/java"
PLUGIN_ID="com.optidrive.vpgenerator"
PLUGIN_HOME="$HOME/Library/Application Support/VisualParadigm/plugins/$PLUGIN_ID"
OUT_DIR="$ROOT/docs/reports-oficial/assets/visual-paradigm"
OUT_PROJECT="$OUT_DIR/OptiDrive-Visual-Paradigm.vpp"
TMP_PROJECT="/tmp/OptiDrive-Visual-Paradigm.vpp"
EXPORT_DIR="$OUT_DIR/exported-from-visual-paradigm"
BASE_PROJECT="/tmp/optidrive-vp-base.vpp"

if [[ ! -d "$VP_APP" ]]; then
  echo "Visual Paradigm não encontrado em: $VP_APP" >&2
  echo "Se estiver noutro sítio, executa: VP_APP='/caminho/Visual Paradigm.app' $0" >&2
  exit 1
fi

if [[ ! -x "$VP_JAVA" ]]; then
  echo "Java interno do Visual Paradigm não encontrado: $VP_JAVA" >&2
  exit 1
fi

mkdir -p "$PLUGIN_HOME/classes" "$OUT_DIR" "$EXPORT_DIR"
cp "$ROOT/tools/visual-paradigm/plugin/plugin.xml" "$PLUGIN_HOME/plugin.xml"

CP_JARS="$VP_APP_RES/lib/vpplatform.jar:$VP_APP_RES/lib/lib01.jar:$VP_APP_RES/lib/lib02.jar:$VP_APP_RES/lib/lib03.jar:$VP_APP_RES/lib/lib04.jar:$VP_APP_RES/lib/lib05.jar:$VP_APP_RES/lib/lib06.jar:$VP_APP_RES/lib/lib07.jar:$VP_APP_RES/lib/lib08.jar:$VP_APP_RES/lib/lib09.jar:$VP_APP_RES/lib/lib10.jar:$VP_APP_RES/lib/lib11.jar:$VP_APP_RES/lib/lib12.jar:$VP_APP_RES/lib/lib13.jar:$VP_APP_RES/lib/lib14.jar:$VP_APP_RES/lib/lib15.jar:$VP_APP_RES/lib/lib16.jar:$VP_APP_RES/lib/lib17.jar:$VP_APP_RES/lib/lib18.jar:$VP_APP_RES/lib/lib19.jar:$VP_APP_RES/lib/lib20.jar"

javac --release 11 -cp "$CP_JARS" -d "$PLUGIN_HOME/classes" "$ROOT/tools/visual-paradigm/src/com/optidrive/vpgenerator/OptiDriveVpGenerator.java"

CMD_CP=".:../lib/vpplatform.jar:../lib/jniwrap.jar:../lib/winpack.jar:../ormlib/orm.jar:../ormlib/orm-core.jar:../lib/lib01.jar:../lib/lib02.jar:../lib/lib03.jar:../lib/lib04.jar:../lib/lib05.jar:../lib/lib06.jar:../lib/lib07.jar:../lib/lib08.jar:../lib/lib09.jar:../lib/lib10.jar:../lib/lib11.jar:../lib/lib12.jar:../lib/lib13.jar:../lib/lib14.jar:../lib/lib15.jar:../lib/lib16.jar:../lib/lib17.jar:../lib/lib18.jar:../lib/lib19.jar:../lib/lib20.jar"

cp "$VP_APP_RES/resources/BaggageSchemas.vpp" "$BASE_PROJECT"
rm -f "$TMP_PROJECT" "$TMP_PROJECT.new" "$TMP_PROJECT.vbak"
rm -rf "$EXPORT_DIR"
mkdir -p "$EXPORT_DIR"

cd "$VP_APP_RES/bin"
"$VP_JAVA" -Xms256m -Xmx1024m -Djava.awt.headless=false -cp "$CMD_CP" \
  com.vp.cmd.Plugin \
  -project "$BASE_PROJECT" \
  -upgrade-project \
  -pluginid "$PLUGIN_ID" \
  -pluginargs "$TMP_PROJECT" \
  > "$OUT_DIR/visual-paradigm-generate.log" 2>&1

: > "$OUT_DIR/visual-paradigm-import-bpmn.log"
for BPMN in "$ROOT/tools/visual-paradigm/bpmn-source"/*.bpmn; do
  "$VP_JAVA" -Xms256m -Xmx1024m -Djava.awt.headless=false -cp "$CMD_CP" \
    com.vp.cmd.ImportBPMN \
    -project "$TMP_PROJECT" \
    -upgrade-project \
    -file "$BPMN" \
    >> "$OUT_DIR/visual-paradigm-import-bpmn.log" 2>&1
  echo "---" >> "$OUT_DIR/visual-paradigm-import-bpmn.log"
done

"$VP_JAVA" -Xms256m -Xmx1024m -Djava.awt.headless=false -cp "$CMD_CP" \
  com.vp.cmd.ExportDiagramImage \
  -project "$TMP_PROJECT" \
  -upgrade-project \
  -out "$EXPORT_DIR" \
  -diagram "*" \
  -type png_with_background \
  > "$OUT_DIR/visual-paradigm-export.log" 2>&1

cp "$TMP_PROJECT" "$OUT_PROJECT"
echo "Projeto Visual Paradigm criado em: $OUT_PROJECT"
echo "Imagens exportadas em: $EXPORT_DIR"
