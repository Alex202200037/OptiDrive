package com.optidrive.vpgenerator;

import com.vp.plugin.ApplicationManager;
import com.vp.plugin.ProjectManager;
import com.vp.plugin.VPPlugin;
import com.vp.plugin.VPPluginCommandLineSupport;
import com.vp.plugin.VPPluginInfo;
import com.vp.plugin.DiagramManager;
import com.vp.plugin.diagram.IDiagramElement;
import com.vp.plugin.diagram.IDiagramUIModel;
import com.vp.plugin.diagram.IDiagramTypeConstants;
import com.vp.plugin.diagram.IConnectorUIModel;
import com.vp.plugin.model.IActor;
import com.vp.plugin.model.IAttribute;
import com.vp.plugin.model.IClass;
import com.vp.plugin.model.IModelElement;
import com.vp.plugin.model.IRelationship;
import com.vp.plugin.model.IUseCase;
import com.vp.plugin.model.factory.IModelElementFactory;
import java.awt.Color;
import java.io.File;
import java.util.LinkedHashMap;
import java.util.Map;

public class OptiDriveVpGenerator implements VPPlugin, VPPluginCommandLineSupport {
    private static final Color INK = new Color(25, 43, 50);
    private static final Color NAVY = new Color(24, 66, 101);
    private static final Color TEAL = new Color(12, 105, 91);
    private static final Color PALE_BLUE = new Color(226, 239, 247);
    private static final Color PALE_GREEN = new Color(224, 242, 233);
    private static final Color PALE_GOLD = new Color(250, 239, 207);
    private static final Color PALE_LILAC = new Color(239, 231, 247);
    private static final Color PALE_GREY = new Color(241, 244, 245);
    private static final Color WHITE = Color.WHITE;

    private DiagramManager dm;
    private IModelElementFactory f;

    public void loaded(VPPluginInfo info) {
        System.out.println("OptiDriveVpGenerator loaded");
    }

    public void unloaded() { }

    public void invoke(String[] args) {
        try {
            String out = args.length > 0
                ? args[0]
                : "docs/reports-oficial/assets/visual-paradigm/OptiDrive-Visual-Paradigm.vpp";
            ApplicationManager app = ApplicationManager.instance();
            ProjectManager pm = app.getProjectManager();
            dm = app.getDiagramManager();
            f = IModelElementFactory.instance();
            pm.getProject().setName("OptiDrive - Diagramas Visual Paradigm");

            makeGantt();
            makeBurndown();
            makeVelocity();
            makeUseCases();
            makePackages();
            makeClassDiagram();
            makeArchitecture();
            makeComponents();
            makeDeployment();

            pm.saveProjectAs(new File(out));
            System.out.println("SAVED " + out + " diagrams=" + pm.getProject().toDiagramArray().length);
        } catch (Throwable t) {
            t.printStackTrace();
        }
    }

    private IDiagramUIModel diagram(String type, String name, int width, int height) {
        IDiagramUIModel d = dm.createDiagram(type);
        d.setName(name);
        d.setBounds(0, 0, width, height);
        d.setDiagramBackground(WHITE);
        d.setGridVisible(false);
        d.setAlignToGrid(true);
        d.setGridWidth(10);
        d.setGridHeight(10);
        d.setConnectorStyle(IConnectorUIModel.CS_ROUND_RECTILINEAR);
        d.setConnectorLineJumps(IConnectorUIModel.CLJ_GAP);
        d.setConnectorLineJumpsSize(8);
        d.setConnectorLabelOrientation(IConnectorUIModel.CLO_HORIZONTAL_ONLY);
        d.setPaintConnectorThroughLabel(IDiagramUIModel.PAINT_CONNECTOR_THROUGH_LABEL_NO);
        d.setShowConnectorName(IDiagramUIModel.SHOW_CONNECTOR_NAME_YES);
        return d;
    }

    private IModelElement model(String type, String name) {
        IModelElement m = f.create(type);
        m.setName(name);
        return m;
    }

    private IDiagramElement elem(IDiagramUIModel d, IModelElement m, int x, int y, int w, int h) {
        IDiagramElement e = dm.createDiagramElement(d, m);
        e.setBounds(x, y, w, h);
        e.setBackground(WHITE);
        e.setForeground(TEAL);
        e.getElementFont().setValues("Arial", false, false, 15, INK);
        e.setModelElementNameAlignment(IDiagramElement.MODEL_ELEMENT_NAME_ALIGNMENT_ALIGN_MIDDLE);
        e.setRequestResetCaption(true);
        e.setRequestResetCaptionSize(true);
        e.setRequestResetCaptionFitWidth(true);
        return e;
    }

    private IDiagramElement box(IDiagramUIModel d, String name, int x, int y, int w, int h, Color bg) {
        IDiagramElement e = elem(d, model(IModelElementFactory.MODEL_TYPE_FLOWCHART_PROCESS, name), x, y, w, h);
        e.setBackground(bg);
        e.setForeground(TEAL);
        return e;
    }

    private IDiagramElement blankBox(IDiagramUIModel d, int x, int y, int w, int h, Color bg) {
        IDiagramElement e = box(d, "layout-cell", x, y, w, h, bg);
        e.getCaptionUIModel().setVisible(false);
        return e;
    }

    private IDiagramElement title(IDiagramUIModel d, String name, int width) {
        IDiagramElement e = elem(d, model(IModelElementFactory.MODEL_TYPE_FLOWCHART_PROCESS, name), 70, 45, width - 140, 70);
        e.setBackground(NAVY);
        e.setForeground(NAVY);
        e.getElementFont().setValues("Arial", true, false, 22, WHITE);
        return e;
    }

    private IDiagramElement dot(IDiagramUIModel d, String name, int x, int y, Color bg) {
        IDiagramElement e = elem(d, model(IModelElementFactory.MODEL_TYPE_FLOWCHART_ON_PAGE_REFERENCE, name), x, y, 54, 54);
        e.setBackground(bg);
        e.setForeground(bg.darker());
        e.getElementFont().setValues("Arial", true, false, 12, WHITE);
        return e;
    }

    private IConnectorUIModel connect(
        IDiagramUIModel d,
        IDiagramElement from,
        IDiagramElement to,
        IRelationship rel,
        String name
    ) {
        if (name != null) {
            rel.setName(name);
        }
        rel.setFrom(from.getModelElement());
        rel.setTo(to.getModelElement());
        IConnectorUIModel c = (IConnectorUIModel) dm.createConnector(d, rel, from, to, new java.awt.Point[0]);
        c.setConnectorStyle(IConnectorUIModel.CS_ROUND_RECTILINEAR);
        c.setConnectorLineJumps(IConnectorUIModel.CLJ_GAP);
        c.setConnectorLabelOrientation(IConnectorUIModel.CLO_HORIZONTAL_ONLY);
        c.setForeground(new Color(76, 104, 111));
        c.getElementFont().setValues("Arial", false, false, 12, INK);
        c.setShowConnectorName(IDiagramUIModel.SHOW_CONNECTOR_NAME_YES);
        c.setPaintThroughLabel(IDiagramUIModel.PAINT_CONNECTOR_THROUGH_LABEL_NO);
        return c;
    }

    private void assoc(IDiagramUIModel d, IDiagramElement from, IDiagramElement to) {
        connect(d, from, to, f.createAssociation(), null);
    }

    private void dep(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, String name) {
        connect(d, from, to, f.createDependency(), name);
    }

    private void include(IDiagramUIModel d, IDiagramElement from, IDiagramElement to) {
        connect(d, from, to, f.createInclude(), "«include»");
    }

    private void flow(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, String name) {
        connect(
            d,
            from,
            to,
            (IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),
            name
        );
    }

    private void makeBurndown() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "burndown-geral", 1540, 850);
        title(d, "Burndown geral - esforço restante por sprint", 1540);

        box(d, "Esforço restante (horas)", 80, 150, 220, 55, PALE_BLUE);
        int[] values = {52, 39, 26, 13, 0};
        String[] sprints = {"Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5"};
        int plotLeft = 300;
        int plotTop = 205;
        int plotHeight = 430;
        int plotBottom = plotTop + plotHeight;
        int step = 230;

        box(d, "52", 205, plotTop - 20, 65, 40, WHITE);
        box(d, "39", 205, plotTop + 88, 65, 40, WHITE);
        box(d, "26", 205, plotTop + 196, 65, 40, WHITE);
        box(d, "13", 205, plotTop + 304, 65, 40, WHITE);
        box(d, "0", 215, plotBottom - 20, 55, 40, WHITE);
        blankBox(d, plotLeft - 10, plotTop, 8, plotHeight + 8, INK);
        blankBox(d, plotLeft - 10, plotBottom, 1125, 8, INK);

        IDiagramElement previous = null;
        for (int i = 0; i < values.length; i++) {
            int x = plotLeft + 80 + i * step;
            int y = plotBottom - (int) Math.round(values[i] * plotHeight / 52.0) - 27;
            IDiagramElement current = dot(d, String.valueOf(values[i]), x, y, TEAL);
            if (previous != null) {
                flow(d, previous, current, null);
            }
            previous = current;
            box(d, sprints[i], x - 40, plotBottom + 35, 135, 45, WHITE);
        }

        box(d, "Leitura: o trabalho remanescente diminui de forma contínua até zero.", 300, 735, 880, 55, PALE_GREEN);
    }

    private void makeVelocity() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "velocity-geral", 1540, 850);
        title(d, "Velocity geral - pontos concluídos por sprint", 1540);
        box(d, "Pontos concluídos", 80, 150, 210, 55, PALE_BLUE);

        int[] values = {18, 22, 24, 21, 20};
        String[] sprints = {"Sprint 1", "Sprint 2", "Sprint 3", "Sprint 4", "Sprint 5"};
        int plotLeft = 290;
        int plotTop = 205;
        int plotBottom = 650;
        int step = 225;
        blankBox(d, plotLeft - 10, plotTop, 8, plotBottom - plotTop + 8, INK);
        blankBox(d, plotLeft - 10, plotBottom, 1110, 8, INK);

        for (int i = 0; i < values.length; i++) {
            int height = (int) Math.round(values[i] * 15.5);
            int x = plotLeft + 80 + i * step;
            IDiagramElement bar = box(d, values[i] + " pts", x, plotBottom - height, 105, height, i % 2 == 0 ? PALE_BLUE : PALE_GREEN);
            bar.getElementFont().setValues("Arial", true, false, 14, INK);
            box(d, sprints[i], x - 15, plotBottom + 35, 135, 45, WHITE);
        }
        box(d, "Média: 21 pontos/sprint", 300, 735, 330, 55, PALE_GOLD);
        box(d, "Leitura: capacidade estável, sem oscilações abruptas.", 670, 735, 600, 55, PALE_GREEN);
    }

    private void makeGantt() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "gantt-oficial", 1700, 940);
        title(d, "Plano temporal do projeto OptiDrive", 1700);

        String[] weeks = {"11/05", "18/05", "25/05", "01/06", "08/06", "15/06", "22/06", "29/06", "06/07", "13/07"};
        int timelineX = 455;
        int weekWidth = 112;
        box(d, "Atividade", 70, 145, 350, 55, PALE_BLUE);
        for (int i = 0; i < weeks.length; i++) {
            box(d, weeks[i], timelineX + i * weekWidth, 145, 92, 55, PALE_BLUE);
        }

        String[] labels = {
            "Sprint 1 - base, autenticação e garagem",
            "Sprint 2 - mapas, postos e energia",
            "Sprint 3 - Smart Save e autonomia",
            "Sprint 4 - social e viagens em grupo",
            "Sprint 5 - administração, DevOps e entrega",
            "Revisão documental, testes e diagramas"
        };
        int[][] spans = {{0, 2}, {1, 3}, {3, 2}, {4, 2}, {5, 3}, {7, 3}};
        Color[] colors = {PALE_BLUE, PALE_GREEN, PALE_GOLD, PALE_LILAC, PALE_BLUE, PALE_GREEN};
        for (int row = 0; row < labels.length; row++) {
            int y = 235 + row * 95;
            box(d, labels[row], 70, y, 350, 58, PALE_GREY);
            for (int i = 0; i < weeks.length; i++) {
                blankBox(d, timelineX + i * weekWidth, y, 92, 58, WHITE);
            }
            int start = spans[row][0];
            int length = spans[row][1];
            IDiagramElement bar = blankBox(d, timelineX + start * weekWidth, y + 8, length * weekWidth - 20, 42, colors[row]);
            bar.setForeground(colors[row].darker());
        }
        box(d, "Marco: entrega final validada", 1300, 825, 330, 60, PALE_GOLD);
    }

    private void makeUseCases() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_USE_CASE_DIAGRAM, "uml-use-cases-geral", 1780, 1120);
        Map<String, IDiagramElement> actors = new LinkedHashMap<>();
        String[][] actorData = {
            {"Visitante", "35", "120"}, {"Condutor autenticado", "35", "440"},
            {"Participante social", "35", "820"}, {"Administrador", "1515", "120"},
            {"Google Maps", "1515", "440"}, {"OpenChargeMap / Postos", "1515", "720"}
        };
        for (String[] data : actorData) {
            IActor actorModel = f.createActor();
            actorModel.setName(data[0]);
            IDiagramElement actor = elem(d, actorModel, Integer.parseInt(data[1]), Integer.parseInt(data[2]), 210, 150);
            actor.setModelElementNameAlignment(IDiagramElement.MODEL_ELEMENT_NAME_ALIGNMENT_ALIGN_BOTTOM_MIDDLE);
            actor.getElementFont().setValues("Arial", true, false, 15, INK);
            actors.put(data[0], actor);
        }

        Map<String, IDiagramElement> uses = new LinkedHashMap<>();
        String[][] useData = {
            {"UC01", "Criar conta", "320", "100"}, {"UC02", "Iniciar sessão / MFA", "320", "245"},
            {"UC11", "Gerir backoffice", "1170", "100"}, {"UC12", "Consultar readiness e métricas", "1170", "245"},
            {"UC03", "Gerir garagem", "320", "430"}, {"UC07", "Aplicar viagem ao veículo", "320", "600"},
            {"UC04", "Planear rota", "700", "430"}, {"UC06", "Calcular Smart Save", "700", "600"},
            {"UC05", "Consultar postos e energia", "1080", "430"}, {"UC08", "Gerir perfil social", "320", "825"},
            {"UC09", "Trocar mensagens e contactos", "700", "825"}, {"UC10", "Gerir viagem colaborativa", "1080", "825"}
        };
        for (String[] data : useData) {
            IUseCase useModel = f.createUseCase();
            useModel.setName(data[0] + " " + data[1]);
            IDiagramElement use = elem(d, useModel, Integer.parseInt(data[2]), Integer.parseInt(data[3]), 265, 105);
            use.setBackground(PALE_BLUE);
            use.setForeground(TEAL);
            use.getElementFont().setValues("Arial", false, false, 15, INK);
            uses.put(data[0], use);
        }

        assoc(d, actors.get("Visitante"), uses.get("UC01"));
        assoc(d, actors.get("Visitante"), uses.get("UC02"));
        assoc(d, actors.get("Condutor autenticado"), uses.get("UC03"));
        assoc(d, actors.get("Condutor autenticado"), uses.get("UC07"));
        assoc(d, actors.get("Participante social"), uses.get("UC08"));
        assoc(d, actors.get("Participante social"), uses.get("UC09"));
        assoc(d, actors.get("Participante social"), uses.get("UC10"));
        assoc(d, actors.get("Administrador"), uses.get("UC11"));
        assoc(d, actors.get("Administrador"), uses.get("UC12"));
        assoc(d, actors.get("Google Maps"), uses.get("UC04"));
        assoc(d, actors.get("Google Maps"), uses.get("UC05"));
        assoc(d, actors.get("OpenChargeMap / Postos"), uses.get("UC05"));
        include(d, uses.get("UC04"), uses.get("UC05"));
        include(d, uses.get("UC04"), uses.get("UC06"));
        include(d, uses.get("UC10"), uses.get("UC09"));
    }

    private IClass cls(String name, String... attrs) {
        IClass c = f.createClass();
        c.setName(name);
        for (String s : attrs) {
            IAttribute a = c.createAttribute();
            a.setName(s);
        }
        return c;
    }

    private IDiagramElement classBox(IDiagramUIModel d, IClass c, int x, int y, int w, int h) {
        IDiagramElement e = elem(d, c, x, y, w, h);
        e.setBackground(PALE_BLUE);
        e.setForeground(TEAL);
        e.getElementFont().setValues("Arial", false, false, 14, INK);
        return e;
    }

    private void classAssoc(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, String name) {
        connect(d, from, to, f.createAssociation(), name);
    }

    private void makeClassDiagram() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_CLASS_DIAGRAM, "classes-dominio", 1680, 1050);
        IDiagramElement profile = classBox(d, cls("SocialProfile", "DisplayName", "Bio", "City", "PhotoUrl"), 110, 90, 270, 180);
        IDiagramElement user = classBox(d, cls("UserAccount", "Id", "Name", "Email", "Role", "MfaEnabled"), 620, 70, 290, 205);
        IDiagramElement route = classBox(d, cls("PlannedRoute", "Origin", "Destination", "DistanceKm", "EstimatedCost", "Stops"), 110, 390, 290, 210);
        IDiagramElement vehicle = classBox(d, cls("VehicleProfile", "Brand", "Model", "FuelKind", "Level", "Consumption"), 620, 390, 290, 210);
        IDiagramElement activity = classBox(d, cls("VehicleActivity", "Type", "Amount", "Cost", "LevelAfter", "CreatedAt"), 1120, 390, 290, 210);
        IDiagramElement station = classBox(d, cls("FuelStation", "Brand", "FuelKind", "Price", "Latitude", "Longitude"), 110, 740, 290, 210);
        IDiagramElement trip = classBox(d, cls("CollaborativeTrip", "Members", "Status", "SplitAmount", "StartedAt"), 620, 740, 300, 205);
        IDiagramElement message = classBox(d, cls("DirectMessage", "Thread", "Body", "CreatedAt", "RelatedTrip"), 1120, 740, 290, 195);

        classAssoc(d, user, profile, "1 : 1");
        classAssoc(d, user, vehicle, "1 : N");
        classAssoc(d, user, route, "planeia");
        classAssoc(d, vehicle, activity, "1 : N");
        classAssoc(d, route, station, "sugere");
        classAssoc(d, route, trip, "origina");
        classAssoc(d, trip, message, "1 : N");
    }

    private void makePackages() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_PACKAGE_DIAGRAM, "uml-pacotes-logicos", 1660, 830);
        String[][] packageData = {
            {"M01 Autenticação e Perfil", "100", "120"}, {"M02 Garagem", "500", "120"},
            {"M03 Planeamento", "900", "120"}, {"M04 Postos e Energia", "1300", "120"},
            {"M06 Social", "300", "510"}, {"M05 Smart Save", "700", "510"},
            {"M07 Administração e DevOps", "1100", "510"}
        };
        Map<String, IDiagramElement> packages = new LinkedHashMap<>();
        for (String[] data : packageData) {
            IDiagramElement e = elem(d, model(IModelElementFactory.MODEL_TYPE_PACKAGE, data[0]), Integer.parseInt(data[1]), Integer.parseInt(data[2]), 270, 135);
            e.setBackground(PALE_BLUE);
            e.setForeground(TEAL);
            e.getElementFont().setValues("Arial", true, false, 15, INK);
            packages.put(data[0], e);
        }
        dep(d, packages.get("M01 Autenticação e Perfil"), packages.get("M02 Garagem"), "utilizador");
        dep(d, packages.get("M02 Garagem"), packages.get("M03 Planeamento"), "veículo");
        dep(d, packages.get("M03 Planeamento"), packages.get("M04 Postos e Energia"), "consulta");
        dep(d, packages.get("M03 Planeamento"), packages.get("M05 Smart Save"), "cálculo");
        dep(d, packages.get("M06 Social"), packages.get("M03 Planeamento"), "viagem");
        dep(d, packages.get("M07 Administração e DevOps"), packages.get("M01 Autenticação e Perfil"), "operação");
    }

    private void makeArchitecture() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "arquitetura-geral", 1700, 940);
        title(d, "Arquitetura geral OptiDrive - visão por camadas", 1700);

        IDiagramElement client = box(d, "Cliente Web\nDesktop e mobile", 80, 285, 250, 130, PALE_GREEN);
        IDiagramElement presentation = box(d, "Apresentação\nASP.NET Core MVC\nRazor Views + Controllers", 420, 255, 310, 190, PALE_BLUE);
        IDiagramElement application = box(d, "Aplicação e domínio\nGaragem · Rotas · Smart Save\nSocial · Administração", 820, 255, 350, 190, PALE_GREEN);
        IDiagramElement persistence = box(d, "Persistência\nEntity Framework Core\nSQLite / Azure Storage", 1280, 255, 300, 190, PALE_GOLD);
        IDiagramElement identity = box(d, "Identidade externa\nGoogle e Microsoft OAuth", 360, 635, 320, 125, PALE_LILAC);
        IDiagramElement maps = box(d, "Google Maps Platform\nGeocoding, Directions e mapa", 780, 635, 350, 125, PALE_LILAC);
        IDiagramElement stations = box(d, "OpenChargeMap / fontes de postos\nCarregadores, combustíveis e preços", 1230, 635, 360, 125, PALE_LILAC);

        flow(d, client, presentation, "HTTPS");
        flow(d, presentation, application, "pedidos e DTOs");
        flow(d, application, persistence, "repositórios");
        flow(d, presentation, identity, "autenticação");
        flow(d, application, maps, "rotas");
        flow(d, application, stations, "postos e energia");
    }

    private IDiagramElement component(IDiagramUIModel d, String name, int x, int y, int w, int h, Color bg) {
        IDiagramElement e = elem(d, model(IModelElementFactory.MODEL_TYPE_COMPONENT, name), x, y, w, h);
        e.setBackground(bg);
        e.setForeground(TEAL);
        e.getElementFont().setValues("Arial", false, false, 14, INK);
        return e;
    }

    private void makeComponents() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_COMPONENT_DIAGRAM, "componentes", 1840, 1040);
        IDiagramElement ui = component(d, "OptiDrive.Web\nControllers e Razor Views", 640, 70, 460, 130, PALE_BLUE);
        IDiagramElement state = component(d, "AppStateService\nSessão, tema e idioma", 80, 350, 290, 125, PALE_GREEN);
        IDiagramElement garage = component(d, "GarageService\nVeículos e histórico", 410, 350, 290, 125, PALE_GREEN);
        IDiagramElement route = component(d, "RoutePlanningService\nRotas e autonomia", 740, 350, 300, 125, PALE_GREEN);
        IDiagramElement social = component(d, "SocialService\nContactos, chat e viagens", 1080, 350, 300, 125, PALE_GREEN);
        IDiagramElement admin = component(d, "AdminHealthService\nReadiness e métricas", 1420, 350, 300, 125, PALE_GREEN);
        IDiagramElement db = component(d, "OptiDriveDbContext\nEF Core + SQLite", 430, 725, 390, 130, PALE_GOLD);
        IDiagramElement external = component(d, "ExternalApiClients\nGoogle Maps, OpenChargeMap e OAuth", 1030, 725, 430, 130, PALE_LILAC);

        dep(d, ui, state, "perfil e sessão");
        dep(d, ui, garage, "garagem");
        dep(d, ui, route, "planeamento");
        dep(d, ui, social, "social");
        dep(d, ui, admin, "backoffice");
        dep(d, garage, db, "persistência");
        dep(d, social, db, "mensagens");
        dep(d, admin, db, "auditoria");
        dep(d, route, external, "geocoding e directions");
        dep(d, route, db, "rotas guardadas");
        dep(d, state, external, "OAuth");
    }

    private IDiagramElement deploymentElement(IDiagramUIModel d, String type, String name, int x, int y, int w, int h, Color bg) {
        IDiagramElement e = elem(d, model(type, name), x, y, w, h);
        e.setBackground(bg);
        e.setForeground(TEAL);
        e.getElementFont().setValues("Arial", false, false, 15, INK);
        return e;
    }

    private void makeDeployment() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_DEPLOYMENT_DIAGRAM, "deployment", 1740, 900);
        IDiagramElement browser = deploymentElement(d, IModelElementFactory.MODEL_TYPE_NODE, "Cliente\nBrowser desktop / mobile", 70, 300, 260, 140, PALE_GREEN);
        IDiagramElement github = deploymentElement(d, IModelElementFactory.MODEL_TYPE_NODE, "GitHub\nRepositório e Actions", 410, 80, 280, 140, PALE_LILAC);
        IDiagramElement azure = deploymentElement(d, IModelElementFactory.MODEL_TYPE_NODE, "Azure App Service\nLinux · .NET 8", 780, 280, 320, 180, PALE_BLUE);
        IDiagramElement artifact = deploymentElement(d, IModelElementFactory.MODEL_TYPE_ARTIFACT, "OptiDrive.Web\nArtefacto publicado", 820, 310, 240, 100, WHITE);
        IDiagramElement db = deploymentElement(d, IModelElementFactory.MODEL_TYPE_NODE, "Persistência\nSQLite / armazenamento", 1190, 130, 300, 150, PALE_GOLD);
        IDiagramElement external = deploymentElement(d, IModelElementFactory.MODEL_TYPE_NODE, "Serviços externos\nGoogle Maps · OCM · OAuth", 1190, 540, 320, 155, PALE_LILAC);

        dep(d, browser, azure, "HTTPS");
        dep(d, github, azure, "CI/CD");
        dep(d, azure, db, "leitura e escrita");
        dep(d, azure, external, "HTTPS / JSON");
        dep(d, azure, artifact, "executa");
    }

}
