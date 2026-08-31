package com.optidrive.vpgenerator;

import com.vp.plugin.*;
import com.vp.plugin.diagram.*;
import com.vp.plugin.model.*;
import com.vp.plugin.model.factory.IModelElementFactory;
import java.awt.Color;
import java.awt.Point;
import java.io.File;
import java.util.*;

public class OptiDriveVpGenerator implements VPPlugin, VPPluginCommandLineSupport {
    private DiagramManager dm;
    private IModelElementFactory f;

    public void loaded(VPPluginInfo info) { System.out.println("OptiDriveVpGenerator loaded"); }
    public void unloaded() { }

    public void invoke(String[] args) {
        try {
            String out = args.length > 0 ? args[0] : "docs/reports-oficial/assets/visual-paradigm/OptiDrive-Visual-Paradigm.vpp";
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
        } catch (Throwable t) { t.printStackTrace(); }
    }

    private IDiagramUIModel diagram(String type, String name) {
        IDiagramUIModel d = dm.createDiagram(type);
        d.setName(name);
        d.setBounds(0, 0, 1700, 1000);
        return d;
    }
    private IDiagramElement elem(IDiagramUIModel d, IModelElement m, int x, int y, int w, int h) {
        IDiagramElement e = dm.createDiagramElement(d, m);
        e.setBounds(x, y, w, h);
        e.setForeground(new Color(6, 88, 78));
        e.setRequestResetCaption(true);
        e.setRequestResetCaptionSize(true);
        e.setRequestResetCaptionFitWidth(true);
        return e;
    }
    private IModelElement model(String type, String name) {
        IModelElement m = f.create(type);
        m.setName(name);
        return m;
    }
    private void connect(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, IRelationship rel, String name) {
        if (name != null) rel.setName(name);
        rel.setFrom(from.getModelElement());
        rel.setTo(to.getModelElement());
        IConnectorUIModel c = (IConnectorUIModel) dm.createConnector(d, rel, from, to, new Point[0]);
        c.setConnectorStyle(IConnectorUIModel.CS_RECTI_LINEAR);
        c.setForeground(new Color(92, 129, 122));
    }
    private void assoc(IDiagramUIModel d, IDiagramElement from, IDiagramElement to) { connect(d, from, to, f.createAssociation(), null); }
    private void dep(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, String name) { connect(d, from, to, f.createDependency(), name); }
    private void include(IDiagramUIModel d, IDiagramElement from, IDiagramElement to) { connect(d, from, to, f.createInclude(), null); }

    private IDiagramElement box(IDiagramUIModel d, String name, int x, int y, int w, int h) {
        return elem(d, model(IModelElementFactory.MODEL_TYPE_FLOWCHART_PROCESS, name), x, y, w, h);
    }
    private IDiagramElement boxBg(IDiagramUIModel d, String name, int x, int y, int w, int h, Color bg) {
        IDiagramElement e = box(d, name, x, y, w, h);
        e.setRequestResetCaption(true);
        e.setRequestResetCaptionSize(true);
        e.setRequestResetCaptionFitWidth(true);
        return e;
    }
    private void makeBurndown() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "burndown-geral");
        box(d, "Burndown geral - OptiDrive", 80, 60, 420, 70);
        String[] labels = {"S1\n52h", "S2\n39h", "S3\n26h", "S4\n13h", "S5\n0h"};
        int[] heights = {260, 205, 150, 95, 40};
        ArrayList<IDiagramElement> bars = new ArrayList<>();
        for (int i=0;i<labels.length;i++) {
            bars.add(box(d, labels[i], 180+i*230, 420-heights[i], 120, heights[i]));
        }
        for (int i=0;i<bars.size()-1;i++) connect(d, bars.get(i), bars.get(i+1), (IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE), "restante");
        box(d, "Eixo: sprints | Métrica: esforço restante", 170, 500, 760, 60);
    }
    private void makeVelocity() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "velocity-geral");
        box(d, "Velocity chart geral - OptiDrive", 80, 60, 430, 70);
        String[] labels = {"S1\n18 pts", "S2\n22 pts", "S3\n24 pts", "S4\n21 pts", "S5\n20 pts"};
        int[] heights = {180, 220, 240, 210, 200};
        ArrayList<IDiagramElement> bars = new ArrayList<>();
        for (int i=0;i<labels.length;i++) {
            bars.add(box(d, labels[i], 180+i*230, 430-heights[i], 120, heights[i]));
        }
        box(d, "Eixo: sprints | Métrica: pontos concluídos", 170, 500, 780, 60);
    }

    private void makeGantt() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "gantt-oficial");
        Color header = new Color(210, 232, 245);
        Color sprint = new Color(116, 190, 220);
        Color review = new Color(155, 214, 178);
        Color milestone = new Color(244, 196, 102);
        boxBg(d, "Mapa de Gantt - OptiDrive (11/05/2026 a 14/07/2026)", 90, 60, 1370, 70, header);
        String[] weeks = {"11/05", "18/05", "25/05", "01/06", "08/06", "15/06", "22/06", "29/06", "06/07", "13/07"};
        for (int i=0;i<weeks.length;i++) boxBg(d, weeks[i], 320+i*115, 155, 90, 45, header);
        String[][] rows = {
            {"Sprint 1 - Base, login e garagem", "330", "230", "240", "11/05-20/05"},
            {"Sprint 2 - Mapas, postos e energia", "560", "300", "240", "21/05-30/05"},
            {"Sprint 3 - Smart Save e autonomia", "800", "370", "240", "01/06-08/06"},
            {"Sprint 4 - Social e viagens em grupo", "1035", "440", "240", "09/06-16/06"},
            {"Sprint 5 - DevOps, admin e entrega", "1255", "510", "260", "17/06-30/06"},
            {"Revisão documental e diagramas VP", "920", "610", "560", "01/07-14/07"}
        };
        for (String[] r : rows) {
            int x=Integer.parseInt(r[1]), y=Integer.parseInt(r[2]), w=Integer.parseInt(r[3]);
            boxBg(d, r[0], 90, y, 285, 55, header);
            boxBg(d, r[4], x, y, w, 55, r[0].startsWith("Revisão") ? review : sprint);
        }
        boxBg(d, "Entregáveis finais\nConfluence + Jira + Git + Azure", 1335, 700, 310, 70, milestone);
    }

    private void makeUseCases() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_USE_CASE_DIAGRAM, "uml-use-cases-geral");
        Map<String,IDiagramElement> a = new LinkedHashMap<>();
        String[][] actors = {
            {"Visitante","70","110"},
            {"Condutor autenticado","70","390"},
            {"Participante social","70","690"},
            {"Administrador","1390","110"},
            {"Google Maps","1390","365"},
            {"OpenChargeMap/Postos","1390","635"}
        };
        for (String[] x: actors) {
            IActor m=f.createActor();
            m.setName(x[0]);
            IDiagramElement actor = elem(d,m,Integer.parseInt(x[1]),Integer.parseInt(x[2]),210,150);
            actor.setModelElementNameAlignment(IDiagramElement.MODEL_ELEMENT_NAME_ALIGNMENT_ALIGN_BOTTOM_MIDDLE);
            actor.setRequestResetCaption(true);
            actor.setRequestResetCaptionSize(true);
            actor.setRequestResetCaptionFitWidth(true);
            a.put(x[0], actor);
        }
        Map<String,IDiagramElement> u = new LinkedHashMap<>();
        String[][] use = {
            {"UC01 Criar conta","360","95"},
            {"UC02 Iniciar sessão/MFA","650","95"},
            {"UC11 Backoffice/readiness","1040","95"},
            {"UC12 Healthcheck/sync","1040","205"},
            {"UC03 Gerir garagem","360","350"},
            {"UC04 Planear rota","660","330"},
            {"UC05 Consultar postos","1010","330"},
            {"UC06 Calcular Smart Save","660","485"},
            {"UC07 Aplicar viagem ao veículo","360","515"},
            {"UC08 Gerir perfil social","360","705"},
            {"UC09 Mensagens/contactos","660","705"},
            {"UC10 Viagem colaborativa","980","705"}
        };
        for (String[] x: use) { IUseCase m=f.createUseCase(); m.setName(x[0]); u.put(x[0].substring(0,4), elem(d,m,Integer.parseInt(x[1]),Integer.parseInt(x[2]),230,90)); }
        assoc(d,a.get("Visitante"),u.get("UC01")); assoc(d,a.get("Visitante"),u.get("UC02"));
        for(String id:new String[]{"UC03","UC04","UC07"}) assoc(d,a.get("Condutor autenticado"),u.get(id));
        for(String id:new String[]{"UC08","UC09","UC10"}) assoc(d,a.get("Participante social"),u.get(id));
        for(String id:new String[]{"UC11","UC12"}) assoc(d,a.get("Administrador"),u.get(id));
        for(String id:new String[]{"UC04","UC05"}) assoc(d,a.get("Google Maps"),u.get(id));
        for(String id:new String[]{"UC05","UC06"}) assoc(d,a.get("OpenChargeMap/Postos"),u.get(id));
        include(d,u.get("UC04"),u.get("UC05"));
        include(d,u.get("UC04"),u.get("UC06"));
        include(d,u.get("UC10"),u.get("UC09"));
    }

    private IClass cls(String name, String... attrs) {
        IClass c = f.createClass(); c.setName(name);
        for (String s: attrs) { IAttribute a=c.createAttribute(); a.setName(s); }
        return c;
    }
    private void classAssoc(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, String name) { connect(d, from, to, f.createAssociation(), name); }
    private void makeClassDiagram() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_CLASS_DIAGRAM, "classes-dominio");
        IDiagramElement user=elem(d,cls("UserAccount","Id","Name","Email","Role","MfaEnabled"),80,90,240,170);
        IDiagramElement profile=elem(d,cls("SocialProfile","DisplayName","Bio","City","PhotoUrl"),420,90,240,170);
        IDiagramElement vehicle=elem(d,cls("VehicleProfile","Brand","Model","FuelKind","Level","Consumption"),80,360,260,180);
        IDiagramElement activity=elem(d,cls("VehicleActivity","Type","Amount","Cost","LevelAfter","CreatedAt"),420,360,260,180);
        IDiagramElement route=elem(d,cls("PlannedRoute","Origin","Destination","DistanceKm","EstimatedCost","Stops"),760,230,270,190);
        IDiagramElement station=elem(d,cls("FuelStation","Brand","FuelKind","Price","Latitude","Longitude"),1120,230,260,180);
        IDiagramElement msg=elem(d,cls("DirectMessage","Thread","Body","CreatedAt","RelatedTrip"),760,590,250,170);
        IDiagramElement trip=elem(d,cls("CollaborativeTrip","Members","Status","SplitAmount","StartedAt"),1120,590,280,170);
        classAssoc(d,user,profile,"1:1"); classAssoc(d,user,vehicle,"1:N"); classAssoc(d,vehicle,activity,"1:N"); classAssoc(d,user,route,"planeia"); classAssoc(d,route,station,"sugere"); classAssoc(d,route,trip,"opcional"); classAssoc(d,msg,trip,"contexto");
    }

    private void makePackages() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_PACKAGE_DIAGRAM, "uml-pacotes-logicos");
        String[][] pkgs={{"M01 Autenticação e Perfil","100","120"},{"M02 Garagem","420","120"},{"M03 Planeamento","740","120"},{"M04 Postos/Energia","1060","120"},{"M05 Smart Save","740","390"},{"M06 Social","420","390"},{"M07 Administração/DevOps","1060","390"}};
        Map<String,IDiagramElement> m=new HashMap<>();
        for(String[] p:pkgs) m.put(p[0], elem(d,model(IModelElementFactory.MODEL_TYPE_PACKAGE,p[0]),Integer.parseInt(p[1]),Integer.parseInt(p[2]),250,120));
        dep(d,m.get("M01 Autenticação e Perfil"),m.get("M02 Garagem"),"utilizador"); dep(d,m.get("M02 Garagem"),m.get("M03 Planeamento"),"veículo"); dep(d,m.get("M03 Planeamento"),m.get("M04 Postos/Energia"),"consulta"); dep(d,m.get("M03 Planeamento"),m.get("M05 Smart Save"),"cálculo"); dep(d,m.get("M06 Social"),m.get("M03 Planeamento"),"viagem grupo"); dep(d,m.get("M07 Administração/DevOps"),m.get("M01 Autenticação e Perfil"),"operação");
    }

    private void makeArchitecture() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_FLOWCHART, "arquitetura-geral");
        Color title = new Color(210, 232, 245);
        Color client = new Color(215, 241, 232);
        Color app = new Color(170, 218, 236);
        Color domain = new Color(184, 224, 196);
        Color infra = new Color(243, 226, 170);
        Color ext = new Color(231, 217, 246);
        boxBg(d, "Arquitetura geral OptiDrive - visão por camadas", 110, 60, 1320, 70, title);
        IDiagramElement user = boxBg(d, "Utilizador\nDesktop / Mobile", 90, 245, 210, 110, client);
        IDiagramElement mvc = boxBg(d, "Camada de apresentação\nASP.NET Core MVC\nRazor Views + Controllers", 390, 225, 300, 150, app);
        IDiagramElement services = boxBg(d, "Camada aplicacional\nServiços de garagem, rotas, social, admin", 790, 225, 340, 150, domain);
        IDiagramElement db = boxBg(d, "Persistência\nEF Core + SQLite/LocalDB\nHistórico, veículos, perfis", 1230, 225, 320, 150, infra);
        IDiagramElement auth = boxBg(d, "OAuth externo\nGoogle + Microsoft", 390, 510, 300, 110, ext);
        IDiagramElement maps = boxBg(d, "Google Maps APIs\nGeocoding + Directions + mapa", 790, 510, 340, 110, ext);
        IDiagramElement charge = boxBg(d, "OpenChargeMap / Postos\nCarregadores, combustíveis e preços", 1230, 510, 320, 110, ext);
        connect(d,user,mvc,(IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),"HTTPS");
        connect(d,mvc,services,(IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),"DTOs / comandos");
        connect(d,services,db,(IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),"repositórios");
        connect(d,mvc,auth,(IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),"login externo");
        connect(d,services,maps,(IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),"rotas");
        connect(d,services,charge,(IRelationship) f.create(IModelElementFactory.MODEL_TYPE_FLOWCHART_FLOWLINE),"postos/energia");
    }

    private void makeComponents() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_COMPONENT_DIAGRAM, "componentes");
        IDiagramElement ui=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"OptiDrive.Web - Controllers e Razor Views"),80,255,330,120);
        IDiagramElement state=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"AppStateService - sessão, tema e idioma"),520,105,330,110);
        IDiagramElement garage=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"GarageService - perfis, níveis e histórico"),520,290,340,120);
        IDiagramElement route=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"RoutePlanningService - rotas e autonomia"),950,105,350,120);
        IDiagramElement stations=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"FuelStationService - postos, marcas e preços"),950,310,360,120);
        IDiagramElement social=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"SocialService - contactos, chat e viagens"),520,520,360,120);
        IDiagramElement admin=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"AdminHealthService - readiness e métricas"),950,540,360,110);
        IDiagramElement db=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"OptiDriveDbContext - EF Core + SQLite"),1400,265,340,120);
        IDiagramElement external=elem(d,model(IModelElementFactory.MODEL_TYPE_COMPONENT,"ExternalApiClients - Google Maps, OCM e OAuth"),1400,520,380,120);
        dep(d,ui,state,"perfil/sessão");
        dep(d,ui,garage,"garagem");
        dep(d,ui,route,"planeamento");
        dep(d,ui,social,"social");
        dep(d,ui,admin,"backoffice");
        dep(d,garage,db,"persistência");
        dep(d,social,db,"mensagens");
        dep(d,admin,db,"auditoria");
        dep(d,route,stations,"postos na rota");
        dep(d,route,external,"directions/geocoding");
        dep(d,stations,external,"preços/carregadores");
        dep(d,stations,db,"cache local");
    }

    private void makeDeployment() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_DEPLOYMENT_DIAGRAM, "deployment");
        IDiagramElement dev=elem(d,model(IModelElementFactory.MODEL_TYPE_NODE,"MacBook / Dev"),120,160,250,120);
        IDiagramElement docker=elem(d,model(IModelElementFactory.MODEL_TYPE_NODE,"Docker Compose"),450,160,260,120);
        IDiagramElement web=elem(d,model(IModelElementFactory.MODEL_TYPE_NODE,"optidrive-web :8080"),790,160,280,120);
        IDiagramElement db=elem(d,model(IModelElementFactory.MODEL_TYPE_ARTIFACT,"data/optidrive.db"),1150,160,260,100);
        IDiagramElement azure=elem(d,model(IModelElementFactory.MODEL_TYPE_NODE,"Azure App Service"),790,430,280,120);
        dep(d,dev,docker,"build/run"); dep(d,docker,web,"container"); dep(d,web,db,"volume"); dep(d,web,azure,"deploy");
    }

    private void bpmnSeq(IDiagramUIModel d, IDiagramElement from, IDiagramElement to, String name) { connect(d, from, to, f.createBPSequenceFlow(), name); }
    private IDiagramElement bpmn(IDiagramUIModel d, String type, String name, int x, int y, int w, int h) {
        IModelElement m;
        if (IModelElementFactory.MODEL_TYPE_BP_START_EVENT.equals(type)) m = f.createBPStartEvent();
        else if (IModelElementFactory.MODEL_TYPE_BP_END_EVENT.equals(type)) m = f.createBPEndEvent();
        else if (IModelElementFactory.MODEL_TYPE_BP_USER_TASK.equals(type)) m = f.createBPUserTask();
        else if (IModelElementFactory.MODEL_TYPE_BP_SERVICE_TASK.equals(type)) m = f.createBPServiceTask();
        else if (IModelElementFactory.MODEL_TYPE_BP_GATEWAY_DATA_BASED_XOR.equals(type)) m = f.createBPGatewayDataBasedXOR();
        else if (IModelElementFactory.MODEL_TYPE_BP_TASK.equals(type)) m = f.createBPTask();
        else m = model(type, name);
        m.setName(name);
        return elem(d, m, x, y, w, h);
    }
    private void makeBpmnRoute() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_BUSINESS_PROCESS_DIAGRAM, "bpmn-01-planeamento-rota");
        IDiagramElement s=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_START_EVENT,"Necessidade de viagem",120,220,80,80);
        IDiagramElement t1=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_USER_TASK,"Introduzir origem, destino e veículo",280,200,250,110);
        IDiagramElement t2=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Validar dados e nível atual",620,200,240,110);
        IDiagramElement g1=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_GATEWAY_DATA_BASED_XOR,"Dados suficientes?",940,215,110,90);
        IDiagramElement t3=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_SERVICE_TASK,"Geocoding e Directions",1120,190,240,110);
        IDiagramElement t4=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_SERVICE_TASK,"Consultar postos e carregadores",1120,390,260,110);
        IDiagramElement t5=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Calcular custo, autonomia e Smart Save",760,410,260,120);
        IDiagramElement g2=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_GATEWAY_DATA_BASED_XOR,"Autonomia suficiente?",500,425,120,90);
        IDiagramElement t6=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Sugerir paragens de reforço",280,410,250,110);
        IDiagramElement e=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_END_EVENT,"Rota pronta",120,430,80,80);
        bpmnSeq(d,s,t1,null); bpmnSeq(d,t1,t2,null); bpmnSeq(d,t2,g1,"sim"); bpmnSeq(d,g1,t3,"sim"); bpmnSeq(d,t3,t4,null); bpmnSeq(d,t4,t5,null); bpmnSeq(d,t5,g2,null); bpmnSeq(d,g2,e,"sim"); bpmnSeq(d,g2,t6,"não"); bpmnSeq(d,t6,e,null);
    }
    private void makeBpmnGarage() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_BUSINESS_PROCESS_DIAGRAM, "bpmn-02-garagem-veiculo");
        IDiagramElement s=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_START_EVENT,"Veículo a registar",120,250,80,80);
        IDiagramElement t1=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_USER_TASK,"Selecionar marca, modelo e combustível",300,230,280,110);
        IDiagramElement t2=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Validar dados obrigatórios",660,230,240,110);
        IDiagramElement g=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_GATEWAY_DATA_BASED_XOR,"Dados válidos?",980,245,110,90);
        IDiagramElement t3=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Guardar perfil do veículo",1160,230,260,110);
        IDiagramElement t4=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Criar histórico inicial",1160,430,260,110);
        IDiagramElement e=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_END_EVENT,"Garagem atualizada",660,455,90,90);
        IDiagramElement err=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Mostrar erros ao utilizador",300,430,280,110);
        bpmnSeq(d,s,t1,null); bpmnSeq(d,t1,t2,null); bpmnSeq(d,t2,g,null); bpmnSeq(d,g,t3,"sim"); bpmnSeq(d,t3,t4,null); bpmnSeq(d,t4,e,null); bpmnSeq(d,g,err,"não"); bpmnSeq(d,err,t1,"corrigir");
    }
    private void makeBpmnSocial() {
        IDiagramUIModel d = diagram(IDiagramTypeConstants.DIAGRAM_TYPE_BUSINESS_PROCESS_DIAGRAM, "bpmn-03-social-viagem");
        IDiagramElement s=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_START_EVENT,"Condutor cria viagem",120,250,80,80);
        IDiagramElement t1=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_USER_TASK,"Pesquisar pessoas e convidar",300,230,260,110);
        IDiagramElement t2=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Notificar participantes",620,230,240,110);
        IDiagramElement g=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_GATEWAY_DATA_BASED_XOR,"Participantes aceitam?",920,245,120,90);
        IDiagramElement t3=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Ativar chat e partilha de custos",1100,230,280,110);
        IDiagramElement t4=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Acompanhar viagem colaborativa",1100,430,280,110);
        IDiagramElement e=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_END_EVENT,"Viagem concluída",620,455,90,90);
        IDiagramElement alt=bpmn(d,IModelElementFactory.MODEL_TYPE_BP_TASK,"Manter viagem individual",300,430,260,110);
        bpmnSeq(d,s,t1,null); bpmnSeq(d,t1,t2,null); bpmnSeq(d,t2,g,null); bpmnSeq(d,g,t3,"sim"); bpmnSeq(d,t3,t4,null); bpmnSeq(d,t4,e,null); bpmnSeq(d,g,alt,"não"); bpmnSeq(d,alt,e,null);
    }
}
