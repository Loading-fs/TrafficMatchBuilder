using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TrafficMatchBuilder.Models;
using TrafficMatchBuilder.Services;

namespace TrafficMatchBuilder;

public sealed class MainForm : Form
{
    private const string AppVersion = "v0.3 Beta";

    private readonly Color Bg = Color.FromArgb(11, 18, 32);
    private readonly Color Bg2 = Color.FromArgb(15, 23, 42);
    private readonly Color Panel = Color.FromArgb(30, 41, 59);
    private readonly Color PanelDark = Color.FromArgb(17, 24, 39);
    private readonly Color Border = Color.FromArgb(51, 65, 85);
    private readonly Color TextMain = Color.White;
    private readonly Color TextMuted = Color.FromArgb(180, 200, 230);
    private readonly Color TextSoft = Color.FromArgb(125, 155, 190);
    private readonly Color Blue = Color.FromArgb(37, 99, 235);
    private readonly Color BlueLight = Color.FromArgb(56, 189, 248);
    private readonly Color Green = Color.FromArgb(22, 163, 74);
    private readonly Color Disabled = Color.FromArgb(55, 65, 81);
    private readonly Color DisabledText = Color.FromArgb(120, 130, 150);

    private string _language = "en";
    private string _selectedFolder = "";
    private ScanResult? _scanResult;

    private Label? _folderLabel;
    private Label? _detectedLabel;
    private Label? _statusLabel;
    private TextBox? _reportBox;
    private ProgressBar? _progress;
    private Button? _scanButton;
    private Button? _exportButton;

    private readonly List<Label> _statNumbers = new();
    private int _aboutLogoClickCount = 0;

    public MainForm()
    {
        Text = $"TrafficMatch Builder {AppVersion}";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1120, 790);
        MinimumSize = new Size(1060, 740);
        BackColor = Bg;
        ForeColor = TextMain;
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        try
        {
            Icon = new Icon("icon.ico");
        }
        catch
        {
            // The EXE icon is embedded via the project file.
        }

        ShowLanguageScreen();
    }

    private void ClearScreen()
    {
        Controls.Clear();
        _statNumbers.Clear();
    }

    private string L(string key)
    {
        var lang = _language.ToLowerInvariant();

        var en = new Dictionary<string, string>
        {
            ["choose_language"] = "Choose your language",
            ["language_hint"] = "You can change this later from the main screen.",
            ["continue"] = "Continue  ›",
            ["footer"] = "Built by simmers, for simmers.",
            ["back"] = "← Back to language selection",
            ["report"] = "▣ Report",
            ["credits"] = "Credits",
            ["help"] = "? Help",
            ["subtitle"] = "VMR Generator for vPilot & VATSIM",
            ["steps"] = "1. Select Folder   →   2. Scan Models   →   3. Export VMR",
            ["select_title"] = "Select traffic model folder",
            ["selected_folder"] = "Selected folder:",
            ["detected"] = "Detected model collection:",
            ["select_folder"] = "▰  Select Folder",
            ["scan_models"] = "⌕  Scan Models",
            ["export_vmr"] = "▤  Export VMR",
            ["workflow_hint"] = "Start with folder selection. Scan and export unlock automatically.",
            ["help_hint"] = "Need help? Click ? Help in the top right corner for a quick tutorial.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Models",
            ["stat_type"] = "With TypeCode",
            ["stat_rules"] = "VMR Rules",
            ["ready"] = "Ready.",
            ["report_title"] = "Scan report",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} ready.",
            ["folder_selected"] = "Folder selected. Ready to scan.",
            ["folder_report"] = "Folder selected. Click Scan Models to continue.",
            ["scanning"] = "Scanning folder...",
            ["scan_complete"] = "Scan complete. Ready for export.",
            ["scan_failed"] = "Scan failed.",
            ["scan_complete_short"] = "Scan complete.",
            ["root"] = "Root:",
            ["found_cfg"] = "Found aircraft.cfg files:",
            ["found_models"] = "Found models:",
            ["usable_rules"] = "Usable VMR rules:",
            ["vmr_rules"] = "VMR rules:",
            ["copy_report"] = "Copy Report",
            ["save_report"] = "Save Report",
            ["export_title"] = "Export VMR",
            ["export_success"] = "VMR successfully exported.",
            ["export_complete"] = "Export complete",
            ["export_failed"] = "Export failed",
            ["credits_text"] = $"TrafficMatch Builder {AppVersion}\n\nCreated by Nils.\nBuilt by simmers, for simmers.",
            ["help_title"] = "Quick Tutorial",
            ["about_subtitle"] = "Automatic VMR Generator for Microsoft Flight Simulator",
            ["developer"] = "Developer",
            ["project_links"] = "Project Links",
            ["open_flightsim"] = "Open Flightsim.to",
            ["open_github"] = "Open GitHub",
            ["about_credits_title"] = "Credits",
            ["about_credits_text"] = "Thanks to all beta testers, the VATSIM community and everyone providing feedback and bug reports.",
            ["about_disclaimer"] = "Not affiliated with Microsoft, Asobo Studio, VATSIM or vPilot.",
            ["close"] = "Close",
            ["community_warning_title"] = "Community Folder Warning",
            ["community_warning_text"] = "You selected what appears to be the full Microsoft Flight Simulator Community folder.\n\nFor best results, please select a specific traffic model package instead, such as FSLTL, AIG or another installed AI traffic library.\n\nScanning the full Community folder can take longer and may include unrelated addons, aircraft or configuration files. This can lead to confusing scan results or less useful VMR output.\n\nTraffic model packages do not always have to be located directly inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear in the simulator.\n\nDo you want to continue scanning the full Community folder anyway?",
            ["scan_tip"] = "Tip: If the scan result looks incorrect, try selecting the specific traffic model package instead of the full Community folder."
        };

        var de = new Dictionary<string, string>
        {
            ["choose_language"] = "Wähle deine Sprache",
            ["language_hint"] = "Du kannst die Sprache später im Hauptfenster ändern.",
            ["continue"] = "Weiter  ›",
            ["footer"] = "Gebaut von Simmern, für Simmer.",
            ["back"] = "← Zurück zur Sprachauswahl",
            ["report"] = "▣ Bericht",
            ["credits"] = "Credits",
            ["help"] = "? Hilfe",
            ["subtitle"] = "VMR-Generator für vPilot & VATSIM",
            ["steps"] = "1. Ordner wählen   →   2. Modelle scannen   →   3. VMR exportieren",
            ["select_title"] = "Traffic-Modellordner auswählen",
            ["selected_folder"] = "Ausgewählter Ordner:",
            ["detected"] = "Erkannte Modellsammlung:",
            ["select_folder"] = "▰  Ordner wählen",
            ["scan_models"] = "⌕  Modelle scannen",
            ["export_vmr"] = "▤  VMR exportieren",
            ["workflow_hint"] = "Beginne mit der Ordnerauswahl. Scan und Export werden automatisch freigeschaltet.",
            ["help_hint"] = "Brauchst du Hilfe? Klicke oben rechts auf ? Hilfe für ein kurzes Tutorial.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modelle",
            ["stat_type"] = "Mit TypeCode",
            ["stat_rules"] = "VMR-Regeln",
            ["ready"] = "Bereit.",
            ["report_title"] = "Scanbericht",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} bereit.",
            ["folder_selected"] = "Ordner ausgewählt. Bereit zum Scannen.",
            ["folder_report"] = "Ordner ausgewählt. Klicke auf Modelle scannen, um fortzufahren.",
            ["scanning"] = "Ordner wird gescannt...",
            ["scan_complete"] = "Scan abgeschlossen. Bereit für den Export.",
            ["scan_failed"] = "Scan fehlgeschlagen.",
            ["scan_complete_short"] = "Scan abgeschlossen.",
            ["root"] = "Hauptordner:",
            ["found_cfg"] = "Gefundene aircraft.cfg-Dateien:",
            ["found_models"] = "Gefundene Modelle:",
            ["usable_rules"] = "Nutzbare VMR-Regeln:",
            ["vmr_rules"] = "VMR-Regeln:",
            ["copy_report"] = "Bericht kopieren",
            ["save_report"] = "Bericht speichern",
            ["export_title"] = "VMR exportieren",
            ["export_success"] = "VMR erfolgreich exportiert.",
            ["export_complete"] = "Export abgeschlossen",
            ["export_failed"] = "Export fehlgeschlagen",
            ["credits_text"] = $"TrafficMatch Builder {AppVersion}\n\nErstellt von Nils.\nGebaut von Simmern, für Simmer.",
            ["help_title"] = "Kurzes Tutorial",
            ["about_subtitle"] = "Automatischer VMR-Generator für Microsoft Flight Simulator",
            ["developer"] = "Entwickler",
            ["project_links"] = "Projektlinks",
            ["open_flightsim"] = "Flightsim.to öffnen",
            ["open_github"] = "GitHub öffnen",
            ["about_credits_title"] = "Credits",
            ["about_credits_text"] = "Danke an alle Beta-Tester, die VATSIM-Community und alle, die Feedback und Fehlermeldungen liefern.",
            ["about_disclaimer"] = "Nicht verbunden mit Microsoft, Asobo Studio, VATSIM oder vPilot.",
            ["close"] = "Schließen",
            ["community_warning_title"] = "Community-Ordner-Warnung",
            ["community_warning_text"] = "Du hast offenbar den kompletten Microsoft Flight Simulator Community-Ordner ausgewählt.\n\nFür die besten Ergebnisse solltest du ein bestimmtes Traffic-Modellpaket auswählen, zum Beispiel FSLTL, AIG oder eine andere installierte AI-Traffic-Bibliothek.\n\nDas Scannen des kompletten Community-Ordners kann länger dauern und auch fremde Addons, Flugzeuge oder Konfigurationsdateien einschließen. Dadurch können unübersichtliche Scan-Ergebnisse oder weniger sinnvolle VMR-Dateien entstehen.\n\nTraffic-Modellpakete müssen nicht zwingend direkt im Community-Ordner liegen, aber Microsoft Flight Simulator muss auf sie zugreifen können, damit die Modelle im Simulator angezeigt werden.\n\nMöchtest du den kompletten Community-Ordner trotzdem scannen?",
            ["scan_tip"] = "Tipp: Wenn das Scan-Ergebnis merkwürdig aussieht, wähle statt des kompletten Community-Ordners besser das konkrete Traffic-Modellpaket aus."
        };

        var no = new Dictionary<string, string>
        {
            ["choose_language"] = "Velg språk",
            ["language_hint"] = "Du kan endre dette senere fra hovedskjermen.",
            ["continue"] = "Fortsett  ›",
            ["footer"] = "Bygget av simmere, for simmere.",
            ["back"] = "← Tilbake til språkvalg",
            ["report"] = "▣ Rapport",
            ["credits"] = "Credits",
            ["help"] = "? Hjelp",
            ["subtitle"] = "VMR-generator for vPilot & VATSIM",
            ["steps"] = "1. Velg mappe   →   2. Skann modeller   →   3. Eksporter VMR",
            ["select_title"] = "Velg mappe for trafikkmodeller",
            ["selected_folder"] = "Valgt mappe:",
            ["detected"] = "Oppdaget modellsamling:",
            ["select_folder"] = "▰  Velg mappe",
            ["scan_models"] = "⌕  Skann modeller",
            ["export_vmr"] = "▤  Eksporter VMR",
            ["workflow_hint"] = "Start med mappevalg. Skann og eksport låses opp automatisk.",
            ["help_hint"] = "Trenger du hjelp? Klikk på ? Hjelp øverst til høyre for en kort veiledning.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modeller",
            ["stat_type"] = "Med TypeCode",
            ["stat_rules"] = "VMR-regler",
            ["ready"] = "Klar.",
            ["report_title"] = "Skannrapport",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} klar.",
            ["folder_selected"] = "Mappe valgt. Klar til skanning.",
            ["folder_report"] = "Mappe valgt. Klikk på Skann modeller for å fortsette.",
            ["scanning"] = "Skanner mappe...",
            ["scan_complete"] = "Skanning fullført. Klar for eksport.",
            ["scan_failed"] = "Skanning mislyktes.",
            ["scan_complete_short"] = "Skanning fullført.",
            ["root"] = "Rotmappe:",
            ["found_cfg"] = "Fant aircraft.cfg-filer:",
            ["found_models"] = "Fant modeller:",
            ["usable_rules"] = "Brukbare VMR-regler:",
            ["vmr_rules"] = "VMR-regler:",
            ["copy_report"] = "Kopier rapport",
            ["save_report"] = "Lagre rapport",
            ["export_title"] = "Eksporter VMR",
            ["export_success"] = "VMR ble eksportert.",
            ["export_complete"] = "Eksport fullført",
            ["export_failed"] = "Eksport mislyktes",
            ["credits_text"] = $"TrafficMatch Builder {AppVersion}\n\nLaget av Nils.\nBygget av simmere, for simmere.",
            ["help_title"] = "Kort veiledning",
            ["about_subtitle"] = "Automatisk VMR-generator for Microsoft Flight Simulator",
            ["developer"] = "Utvikler",
            ["project_links"] = "Prosjektlenker",
            ["open_flightsim"] = "Åpne Flightsim.to",
            ["open_github"] = "Åpne GitHub",
            ["about_credits_title"] = "Credits",
            ["about_credits_text"] = "Takk til alle beta-testere, VATSIM-miljøet og alle som gir tilbakemeldinger og feilrapporter.",
            ["about_disclaimer"] = "Ikke tilknyttet Microsoft, Asobo Studio, VATSIM eller vPilot.",
            ["close"] = "Lukk",
            ["community_warning_title"] = "Advarsel om Community-mappen",
            ["community_warning_text"] = "Du har tilsynelatende valgt hele Microsoft Flight Simulator Community-mappen.\n\nFor best resultat bør du velge en spesifikk trafikkmodellpakke, for eksempel FSLTL, AIG eller et annet installert AI-trafikkbibliotek.\n\nSkanning av hele Community-mappen kan ta lengre tid og kan inkludere urelaterte tillegg, fly eller konfigurasjonsfiler. Dette kan gi forvirrende skannresultater eller mindre nyttige VMR-filer.\n\nTrafikkmodellpakker må ikke nødvendigvis ligge direkte i Community-mappen, men Microsoft Flight Simulator må ha tilgang til dem for at modellene skal vises i simulatoren.\n\nVil du fortsette å skanne hele Community-mappen likevel?",
            ["scan_tip"] = "Tips: Hvis skannresultatet ser feil ut, prøv å velge den spesifikke trafikkmodellpakken i stedet for hele Community-mappen."
        };

        var fr = new Dictionary<string, string>
        {
            ["choose_language"] = "Choisissez votre langue",
            ["language_hint"] = "Vous pourrez la modifier plus tard depuis l'écran principal.",
            ["continue"] = "Continuer  ›",
            ["footer"] = "Créé par des simmers, pour des simmers.",
            ["back"] = "← Retour au choix de la langue",
            ["report"] = "▣ Rapport",
            ["credits"] = "Credits",
            ["help"] = "? Aide",
            ["subtitle"] = "Générateur VMR pour vPilot & VATSIM",
            ["steps"] = "1. Choisir le dossier   →   2. Scanner les modèles   →   3. Exporter le VMR",
            ["select_title"] = "Sélectionner le dossier des modèles de trafic",
            ["selected_folder"] = "Dossier sélectionné :",
            ["detected"] = "Collection détectée :",
            ["select_folder"] = "▰  Choisir dossier",
            ["scan_models"] = "⌕  Scanner modèles",
            ["export_vmr"] = "▤  Exporter VMR",
            ["workflow_hint"] = "Commencez par choisir un dossier. Le scan et l'export se déverrouillent automatiquement.",
            ["help_hint"] = "Besoin d'aide ? Cliquez sur ? Aide en haut à droite pour un court tutoriel.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modèles",
            ["stat_type"] = "Avec TypeCode",
            ["stat_rules"] = "Règles VMR",
            ["ready"] = "Prêt.",
            ["report_title"] = "Rapport de scan",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} prêt.",
            ["folder_selected"] = "Dossier sélectionné. Prêt à scanner.",
            ["folder_report"] = "Dossier sélectionné. Cliquez sur Scanner modèles pour continuer.",
            ["scanning"] = "Scan du dossier...",
            ["scan_complete"] = "Scan terminé. Prêt pour l'export.",
            ["scan_failed"] = "Échec du scan.",
            ["scan_complete_short"] = "Scan terminé.",
            ["root"] = "Dossier racine :",
            ["found_cfg"] = "Fichiers aircraft.cfg trouvés :",
            ["found_models"] = "Modèles trouvés :",
            ["usable_rules"] = "Règles VMR utilisables :",
            ["vmr_rules"] = "Règles VMR :",
            ["copy_report"] = "Copier rapport",
            ["save_report"] = "Enregistrer rapport",
            ["export_title"] = "Exporter VMR",
            ["export_success"] = "VMR exporté avec succès.",
            ["export_complete"] = "Export terminé",
            ["export_failed"] = "Échec de l'export",
            ["credits_text"] = $"TrafficMatch Builder {AppVersion}\n\nCréé par Nils.\nCréé par des simmers, pour des simmers.",
            ["help_title"] = "Tutoriel rapide",
            ["about_subtitle"] = "Générateur VMR automatique pour Microsoft Flight Simulator",
            ["developer"] = "Développeur",
            ["project_links"] = "Liens du projet",
            ["open_flightsim"] = "Ouvrir Flightsim.to",
            ["open_github"] = "Ouvrir GitHub",
            ["about_credits_title"] = "Credits",
            ["about_credits_text"] = "Merci à tous les bêta-testeurs, à la communauté VATSIM et à toutes les personnes qui envoient des retours et des rapports de bugs.",
            ["about_disclaimer"] = "Non affilié à Microsoft, Asobo Studio, VATSIM ou vPilot.",
            ["close"] = "Fermer",
            ["community_warning_title"] = "Avertissement dossier Community",
            ["community_warning_text"] = "Vous semblez avoir sélectionné le dossier Community complet de Microsoft Flight Simulator.\n\nPour de meilleurs résultats, sélectionnez plutôt un package de modèles de trafic spécifique, par exemple FSLTL, AIG ou une autre bibliothèque de trafic AI installée.\n\nScanner tout le dossier Community peut prendre plus de temps et inclure des addons, avions ou fichiers de configuration sans rapport. Cela peut produire des résultats confus ou un fichier VMR moins utile.\n\nLes packages de modèles de trafic ne doivent pas forcément se trouver directement dans le dossier Community, mais Microsoft Flight Simulator doit pouvoir y accéder pour que les modèles apparaissent dans le simulateur.\n\nVoulez-vous tout de même continuer à scanner le dossier Community complet ?",
            ["scan_tip"] = "Astuce : si le résultat du scan semble incorrect, essayez de sélectionner le package de modèles de trafic spécifique au lieu du dossier Community complet."
        };

        var es = new Dictionary<string, string>
        {
            ["choose_language"] = "Elige tu idioma",
            ["language_hint"] = "Puedes cambiarlo más tarde desde la pantalla principal.",
            ["continue"] = "Continuar  ›",
            ["footer"] = "Creado por simmers, para simmers.",
            ["back"] = "← Volver a la selección de idioma",
            ["report"] = "▣ Informe",
            ["credits"] = "Credits",
            ["help"] = "? Ayuda",
            ["subtitle"] = "Generador VMR para vPilot & VATSIM",
            ["steps"] = "1. Elegir carpeta   →   2. Escanear modelos   →   3. Exportar VMR",
            ["select_title"] = "Seleccionar carpeta de modelos de tráfico",
            ["selected_folder"] = "Carpeta seleccionada:",
            ["detected"] = "Colección detectada:",
            ["select_folder"] = "▰  Elegir carpeta",
            ["scan_models"] = "⌕  Escanear modelos",
            ["export_vmr"] = "▤  Exportar VMR",
            ["workflow_hint"] = "Empieza seleccionando una carpeta. El escaneo y la exportación se habilitan automáticamente.",
            ["help_hint"] = "¿Necesitas ayuda? Haz clic en ? Ayuda arriba a la derecha para ver un tutorial breve.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modelos",
            ["stat_type"] = "Con TypeCode",
            ["stat_rules"] = "Reglas VMR",
            ["ready"] = "Listo.",
            ["report_title"] = "Informe de escaneo",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} listo.",
            ["folder_selected"] = "Carpeta seleccionada. Listo para escanear.",
            ["folder_report"] = "Carpeta seleccionada. Haz clic en Escanear modelos para continuar.",
            ["scanning"] = "Escaneando carpeta...",
            ["scan_complete"] = "Escaneo completo. Listo para exportar.",
            ["scan_failed"] = "Escaneo fallido.",
            ["scan_complete_short"] = "Escaneo completo.",
            ["root"] = "Carpeta raíz:",
            ["found_cfg"] = "Archivos aircraft.cfg encontrados:",
            ["found_models"] = "Modelos encontrados:",
            ["usable_rules"] = "Reglas VMR utilizables:",
            ["vmr_rules"] = "Reglas VMR:",
            ["copy_report"] = "Copiar informe",
            ["save_report"] = "Guardar informe",
            ["export_title"] = "Exportar VMR",
            ["export_success"] = "VMR exportado correctamente.",
            ["export_complete"] = "Exportación completa",
            ["export_failed"] = "Exportación fallida",
            ["credits_text"] = $"TrafficMatch Builder {AppVersion}\n\nCreado por Nils.\nCreado por simmers, para simmers.",
            ["help_title"] = "Tutorial rápido",
            ["about_subtitle"] = "Generador VMR automático para Microsoft Flight Simulator",
            ["developer"] = "Desarrollador",
            ["project_links"] = "Enlaces del proyecto",
            ["open_flightsim"] = "Abrir Flightsim.to",
            ["open_github"] = "Abrir GitHub",
            ["about_credits_title"] = "Créditos",
            ["about_credits_text"] = "Gracias a todos los beta testers, a la comunidad de VATSIM y a todos los que envían comentarios e informes de errores.",
            ["about_disclaimer"] = "No está afiliado a Microsoft, Asobo Studio, VATSIM ni vPilot.",
            ["close"] = "Cerrar",
            ["community_warning_title"] = "Advertencia sobre la carpeta Community",
            ["community_warning_text"] = "Parece que has seleccionado toda la carpeta Community de Microsoft Flight Simulator.\n\nPara obtener mejores resultados, selecciona un paquete específico de modelos de tráfico, como FSLTL, AIG u otra biblioteca de tráfico AI instalada.\n\nEscanear toda la carpeta Community puede tardar más e incluir addons, aviones o archivos de configuración no relacionados. Esto puede generar resultados confusos o un archivo VMR menos útil.\n\nLos paquetes de modelos de tráfico no tienen que estar necesariamente dentro de la carpeta Community, pero Microsoft Flight Simulator debe poder acceder a ellos para que los modelos aparezcan en el simulador.\n\n¿Quieres continuar escaneando toda la carpeta Community de todos modos?",
            ["scan_tip"] = "Consejo: si el resultado del escaneo parece incorrecto, prueba a seleccionar el paquete específico de modelos de tráfico en lugar de toda la carpeta Community."
        };

        var it = new Dictionary<string, string>
        {
            ["choose_language"] = "Scegli la lingua",
            ["language_hint"] = "Puoi cambiarla più tardi dalla schermata principale.",
            ["continue"] = "Continua  ›",
            ["footer"] = "Creato da simmers, per simmers.",
            ["back"] = "← Torna alla selezione lingua",
            ["report"] = "▣ Report",
            ["credits"] = "Credits",
            ["help"] = "? Aiuto",
            ["subtitle"] = "Generatore VMR per vPilot & VATSIM",
            ["steps"] = "1. Seleziona cartella   →   2. Scansiona modelli   →   3. Esporta VMR",
            ["select_title"] = "Seleziona cartella dei modelli traffico",
            ["selected_folder"] = "Cartella selezionata:",
            ["detected"] = "Collezione rilevata:",
            ["select_folder"] = "▰  Seleziona cartella",
            ["scan_models"] = "⌕  Scansiona modelli",
            ["export_vmr"] = "▤  Esporta VMR",
            ["workflow_hint"] = "Inizia selezionando una cartella. Scansione ed esportazione si attivano automaticamente.",
            ["help_hint"] = "Hai bisogno di aiuto? Clicca su ? Aiuto in alto a destra per un breve tutorial.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modelli",
            ["stat_type"] = "Con TypeCode",
            ["stat_rules"] = "Regole VMR",
            ["ready"] = "Pronto.",
            ["report_title"] = "Report scansione",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} pronto.",
            ["folder_selected"] = "Cartella selezionata. Pronto per la scansione.",
            ["folder_report"] = "Cartella selezionata. Clicca su Scansiona modelli per continuare.",
            ["scanning"] = "Scansione cartella...",
            ["scan_complete"] = "Scansione completata. Pronto per l'esportazione.",
            ["scan_failed"] = "Scansione non riuscita.",
            ["scan_complete_short"] = "Scansione completata.",
            ["root"] = "Cartella principale:",
            ["found_cfg"] = "File aircraft.cfg trovati:",
            ["found_models"] = "Modelli trovati:",
            ["usable_rules"] = "Regole VMR utilizzabili:",
            ["vmr_rules"] = "Regole VMR:",
            ["copy_report"] = "Copia report",
            ["save_report"] = "Salva report",
            ["export_title"] = "Esporta VMR",
            ["export_success"] = "VMR esportato correttamente.",
            ["export_complete"] = "Esportazione completata",
            ["export_failed"] = "Esportazione fallita",
            ["credits_text"] = $"TrafficMatch Builder {AppVersion}\n\nCreato da Nils.\nCreato da simmers, per simmers.",
            ["help_title"] = "Tutorial rapido",
            ["about_subtitle"] = "Generatore VMR automatico per Microsoft Flight Simulator",
            ["developer"] = "Sviluppatore",
            ["project_links"] = "Link del progetto",
            ["open_flightsim"] = "Apri Flightsim.to",
            ["open_github"] = "Apri GitHub",
            ["about_credits_title"] = "Credits",
            ["about_credits_text"] = "Grazie a tutti i beta tester, alla community VATSIM e a chi fornisce feedback e segnalazioni di bug.",
            ["about_disclaimer"] = "Non affiliato con Microsoft, Asobo Studio, VATSIM o vPilot.",
            ["close"] = "Chiudi",
            ["community_warning_title"] = "Avviso cartella Community",
            ["community_warning_text"] = "Sembra che tu abbia selezionato l'intera cartella Community di Microsoft Flight Simulator.\n\nPer risultati migliori, seleziona un pacchetto specifico di modelli traffico, ad esempio FSLTL, AIG o un'altra libreria di traffico AI installata.\n\nLa scansione dell'intera cartella Community può richiedere più tempo e includere addon, aerei o file di configurazione non pertinenti. Questo può produrre risultati confusi o un file VMR meno utile.\n\nI pacchetti di modelli traffico non devono necessariamente trovarsi direttamente nella cartella Community, ma Microsoft Flight Simulator deve potervi accedere affinché i modelli vengano visualizzati nel simulatore.\n\nVuoi continuare comunque con la scansione dell'intera cartella Community?",
            ["scan_tip"] = "Suggerimento: se il risultato della scansione sembra errato, prova a selezionare il pacchetto specifico di modelli traffico invece dell'intera cartella Community."
        };

        var dict = lang switch
        {
            "de" => de,
            "no" => no,
            "fr" => fr,
            "es" => es,
            "it" => it,
            _ => en
        };

        return dict.TryGetValue(key, out var value)
            ? value
            : en.TryGetValue(key, out var fallback)
                ? fallback
                : key;
    }

    private string GetHelpText()
    {
        var nl = Environment.NewLine;

        return _language.ToLowerInvariant() switch
        {
            "de" => string.Join(nl,
                "TrafficMatch Builder erstellt VMR-Dateien für vPilot und VATSIM.",
                "",
                "So funktioniert es:",
                "",
                "1. Klicke auf „Ordner wählen“.",
                "2. Wähle den Ordner deiner Traffic-Modelle aus.",
                "3. Klicke auf „Modelle scannen“.",
                "4. Prüfe die gefundenen Modelle im Scanbericht.",
                "5. Klicke auf „VMR exportieren“.",
                "6. Speichere die VMR-Datei an einem Ort deiner Wahl.",
                "7. Füge die VMR-Datei in vPilot unter Model Matching hinzu.",
                "",
                "",
                "Which folder should I scan?",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "",
                "Not recommended:",
                "- The full Community folder",
                "- The entire MSFS Packages folder",
                "- Random addon folders that do not contain traffic models",
                "",
                "Traffic packages do not necessarily have to be located inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear.",
                "Das Programm verändert keine Simulator-Dateien und arbeitet lokal auf deinem PC."),

            "no" => string.Join(nl,
                "TrafficMatch Builder lager VMR-filer for vPilot og VATSIM.",
                "",
                "Slik fungerer det:",
                "",
                "1. Klikk på «Velg mappe».",
                "2. Velg mappen med trafikkmodellene dine.",
                "3. Klikk på «Skann modeller».",
                "4. Sjekk modellene i skannrapporten.",
                "5. Klikk på «Eksporter VMR».",
                "6. Lagre VMR-filen hvor du ønsker.",
                "7. Legg VMR-filen til i vPilot under Model Matching.",
                "",
                "",
                "Which folder should I scan?",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "",
                "Not recommended:",
                "- The full Community folder",
                "- The entire MSFS Packages folder",
                "- Random addon folders that do not contain traffic models",
                "",
                "Traffic packages do not necessarily have to be located inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear.",
                "Programmet endrer ingen simulatorfiler og fungerer lokalt på PC-en din."),

            "fr" => string.Join(nl,
                "TrafficMatch Builder crée des fichiers VMR pour vPilot et VATSIM.",
                "",
                "Comment l'utiliser :",
                "",
                "1. Cliquez sur « Choisir dossier ».",
                "2. Sélectionnez le dossier de vos modèles de trafic.",
                "3. Cliquez sur « Scanner modèles ».",
                "4. Vérifiez les modèles détectés dans le rapport.",
                "5. Cliquez sur « Exporter VMR ».",
                "6. Enregistrez le fichier VMR à l'emplacement souhaité.",
                "7. Ajoutez le fichier VMR dans vPilot sous Model Matching.",
                "",
                "",
                "Which folder should I scan?",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "",
                "Not recommended:",
                "- The full Community folder",
                "- The entire MSFS Packages folder",
                "- Random addon folders that do not contain traffic models",
                "",
                "Traffic packages do not necessarily have to be located inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear.",
                "Le programme ne modifie aucun fichier du simulateur et fonctionne localement."),

            "es" => string.Join(nl,
                "TrafficMatch Builder crea archivos VMR para vPilot y VATSIM.",
                "",
                "Cómo funciona:",
                "",
                "1. Haz clic en «Elegir carpeta».",
                "2. Selecciona la carpeta de tus modelos de tráfico.",
                "3. Haz clic en «Escanear modelos».",
                "4. Revisa los modelos detectados en el informe.",
                "5. Haz clic en «Exportar VMR».",
                "6. Guarda el archivo VMR donde prefieras.",
                "7. Añade el archivo VMR en vPilot en Model Matching.",
                "",
                "",
                "Which folder should I scan?",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "",
                "Not recommended:",
                "- The full Community folder",
                "- The entire MSFS Packages folder",
                "- Random addon folders that do not contain traffic models",
                "",
                "Traffic packages do not necessarily have to be located inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear.",
                "El programa no modifica archivos del simulador y funciona localmente."),

            "it" => string.Join(nl,
                "TrafficMatch Builder crea file VMR per vPilot e VATSIM.",
                "",
                "Come funziona:",
                "",
                "1. Clicca su «Seleziona cartella».",
                "2. Seleziona la cartella dei tuoi modelli di traffico.",
                "3. Clicca su «Scansiona modelli».",
                "4. Controlla i modelli rilevati nel report.",
                "5. Clicca su «Esporta VMR».",
                "6. Salva il file VMR dove preferisci.",
                "7. Aggiungi il file VMR in vPilot nella sezione Model Matching.",
                "",
                "",
                "Which folder should I scan?",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "",
                "Not recommended:",
                "- The full Community folder",
                "- The entire MSFS Packages folder",
                "- Random addon folders that do not contain traffic models",
                "",
                "Traffic packages do not necessarily have to be located inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear.",
                "Il programma non modifica file del simulatore e funziona localmente."),

            _ => string.Join(nl,
                "TrafficMatch Builder creates VMR files for vPilot and VATSIM.",
                "",
                "How it works:",
                "",
                "1. Click “Select Folder”.",
                "2. Choose your traffic model folder.",
                "3. Click “Scan Models”.",
                "4. Review the detected models in the Scan Report.",
                "5. Click “Export VMR”.",
                "6. Save the VMR file to a location of your choice.",
                "7. Add the VMR file to vPilot under Model Matching.",
                "",
                "",
                "Which folder should I scan?",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "",
                "Not recommended:",
                "- The full Community folder",
                "- The entire MSFS Packages folder",
                "- Random addon folders that do not contain traffic models",
                "",
                "Traffic packages do not necessarily have to be located inside the Community folder, but Microsoft Flight Simulator must be able to access them for the models to appear.",
                "The application does not modify simulator files and works locally on your computer.")
        };
    }

    private void ShowLanguageScreen()
    {
        ClearScreen();

        var root = new GradientPanel
        {
            Dock = DockStyle.Fill,
            StartColor = Color.FromArgb(11, 18, 32),
            EndColor = Color.FromArgb(15, 23, 42)
        };
        Controls.Add(root);

        var banner = new HeaderBanner
        {
            Location = new Point(165, 42),
            Size = new Size(790, 170)
        };
        root.Controls.Add(banner);

        root.Controls.Add(new Label
        {
            Text = L("choose_language"),
            Font = new Font("Segoe UI", 21, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(405, 270)
        });

        root.Controls.Add(new Label
        {
            Text = L("language_hint"),
            Font = new Font("Segoe UI", 10),
            ForeColor = TextSoft,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(405, 312)
        });

        var languages = new[]
        {
            ("GB", "English", "Default", "en"),
            ("DE", "Deutsch", "German", "de"),
            ("NO", "Norsk", "Bokmål", "no"),
            ("FR", "Français", "French", "fr"),
            ("ES", "Español", "Spanish", "es"),
            ("IT", "Italiano", "Italian", "it")
        };

        var x = 95;

        foreach (var language in languages)
        {
            var card = CreateLanguageCard(language.Item1, language.Item2, language.Item3, language.Item4);
            card.Location = new Point(x, 380);
            root.Controls.Add(card);
            x += 152;
        }

        var continueButton = CreateModernButton(L("continue"), Blue, new Size(390, 68));
        continueButton.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        continueButton.Location = new Point(365, 590);
        continueButton.Click += (_, _) => ShowMainScreen();
        root.Controls.Add(continueButton);

        root.Controls.Add(new Label
        {
            Text = L("footer"),
            ForeColor = TextSoft,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(465, 710)
        });
    }

    private Panel CreateLanguageCard(string codeLabel, string title, string subtitle, string code)
    {
        var selected = code == _language;

        var card = new HoverPanel
        {
            BackColor = selected ? Blue : Panel,
            BorderColor = selected ? BlueLight : Border,
            Size = new Size(130, 158),
            Cursor = Cursors.Hand
        };

        var labelCode = new Label
        {
            Text = codeLabel,
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 66
        };

        var labelTitle = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 32
        };

        var labelSubtitle = new Label
        {
            Text = subtitle,
            Font = new Font("Segoe UI", 9),
            ForeColor = selected ? Color.FromArgb(210, 225, 255) : TextSoft,
            BackColor = Color.Transparent,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 30
        };

        card.Controls.Add(labelSubtitle);
        card.Controls.Add(labelTitle);
        card.Controls.Add(labelCode);

        void SelectLanguage(object? sender, EventArgs e)
        {
            _language = code;
            ShowLanguageScreen();
        }

        card.Click += SelectLanguage;
        labelCode.Click += SelectLanguage;
        labelTitle.Click += SelectLanguage;
        labelSubtitle.Click += SelectLanguage;

        return card;
    }

    private void ShowMainScreen()
    {
        ClearScreen();

        var wrapper = new GradientPanel
        {
            Dock = DockStyle.Fill,
            StartColor = Bg,
            EndColor = Bg2,
            Padding = new Padding(28)
        };
        Controls.Add(wrapper);

        var back = CreateTopLink(L("back"), 24, 18);
        back.Click += (_, _) => ShowLanguageScreen();
        wrapper.Controls.Add(back);

        var report = CreateHeaderButton(L("report"), 810, 18);
        report.Click += (_, _) => ShowReportMenu();
        wrapper.Controls.Add(report);

        var credits = CreateHeaderButton(L("credits"), 905, 18);
        credits.Click += (_, _) => ShowAboutWindow();
        wrapper.Controls.Add(credits);

        var help = CreateHeaderButton(L("help"), 1000, 18);
        help.Click += (_, _) => MessageBox.Show(
            GetHelpText(),
            L("help_title"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        wrapper.Controls.Add(help);

        wrapper.Controls.Add(new Label
        {
            Text = "TrafficMatch Builder",
            Font = new Font("Segoe UI", 29, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(24, 72)
        });

        wrapper.Controls.Add(new Label
        {
            Text = $"{L("subtitle")}  •  {AppVersion}",
            Font = new Font("Segoe UI", 10),
            ForeColor = TextSoft,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(28, 126)
        });

        wrapper.Controls.Add(new Label
        {
            Text = L("steps"),
            Font = new Font("Segoe UI", 10),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(28, 153)
        });

        var selectPanel = CreatePanelBox(24, 190, 1030, 220);
        wrapper.Controls.Add(selectPanel);

        selectPanel.Controls.Add(new Label
        {
            Text = L("select_title"),
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 22)
        });

        _folderLabel = new Label
        {
            Text = $"{L("selected_folder")}\n-",
            Font = new Font("Segoe UI", 9),
            ForeColor = TextMuted,
            BackColor = PanelDark,
            AutoSize = false,
            Size = new Size(930, 44),
            Location = new Point(24, 60)
        };
        selectPanel.Controls.Add(_folderLabel);

        _detectedLabel = new Label
        {
            Text = $"{L("detected")} -",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = BlueLight,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 108)
        };
        selectPanel.Controls.Add(_detectedLabel);

        var selectButton = CreateModernButton(L("select_folder"), Color.FromArgb(31, 41, 55), new Size(175, 45));
        selectButton.Location = new Point(24, 145);
        selectButton.Click += (_, _) => SelectFolder();
        selectPanel.Controls.Add(selectButton);

        _scanButton = CreateModernButton(L("scan_models"), Disabled, new Size(175, 45));
        _scanButton.Location = new Point(215, 145);
        _scanButton.Click += (_, _) => ScanFolder();
        selectPanel.Controls.Add(_scanButton);

        _exportButton = CreateModernButton(L("export_vmr"), Disabled, new Size(175, 45));
        _exportButton.Location = new Point(406, 145);
        _exportButton.Click += (_, _) => ExportVmr();
        selectPanel.Controls.Add(_exportButton);

        SetActionButton(_scanButton, false, Blue);
        SetActionButton(_exportButton, false, Green);

        selectPanel.Controls.Add(new Label
        {
            Text = L("workflow_hint"),
            Font = new Font("Segoe UI", 9),
            ForeColor = TextSoft,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 190)
        });

        selectPanel.Controls.Add(new Label
        {
            Text = L("help_hint"),
            Font = new Font("Segoe UI", 8),
            ForeColor = TextSoft,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 206)
        });

        CreateStats(wrapper, 24, 425);

        var statusPanel = CreatePanelBox(24, 525, 1030, 70);
        wrapper.Controls.Add(statusPanel);

        _statusLabel = new Label
        {
            Text = L("ready"),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(18, 17)
        };
        statusPanel.Controls.Add(_statusLabel);

        _progress = new ProgressBar
        {
            Location = new Point(18, 43),
            Size = new Size(992, 12),
            Style = ProgressBarStyle.Continuous
        };
        statusPanel.Controls.Add(_progress);

        var reportPanel = CreatePanelBox(24, 610, 1030, 125);
        wrapper.Controls.Add(reportPanel);

        reportPanel.Controls.Add(new Label
        {
            Text = L("report_title"),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(18, 14)
        });

        _reportBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(8, 15, 30),
            ForeColor = TextMain,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 9),
            Text = L("ready_report"),
            Location = new Point(18, 45),
            Size = new Size(992, 64)
        };
        reportPanel.Controls.Add(_reportBox);
    }

    private void CreateStats(Control parent, int x, int y)
    {
        var labels = new[] { L("stat_cfg"), L("stat_models"), L("stat_type"), L("stat_rules") };

        for (var i = 0; i < 4; i++)
        {
            var panel = CreateStatCard(x + i * 263, y, 245, 82);
            parent.Controls.Add(panel);

            var number = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 23, FontStyle.Bold),
                ForeColor = TextMain,
                BackColor = Panel,
                AutoSize = true,
                Location = new Point(18, 14)
            };

            panel.Controls.Add(number);
            _statNumbers.Add(number);

            panel.Controls.Add(new Label
            {
                Text = labels[i],
                ForeColor = TextMuted,
                BackColor = Panel,
                AutoSize = true,
                Location = new Point(20, 55)
            });
        }
    }

    private HoverPanel CreatePanelBox(int x, int y, int width, int height)
    {
        return new HoverPanel
        {
            BackColor = PanelDark,
            BorderColor = Border,
            Location = new Point(x, y),
            Size = new Size(width, height)
        };
    }

    private HoverPanel CreateStatCard(int x, int y, int width, int height)
    {
        return new HoverPanel
        {
            BackColor = Panel,
            BorderColor = Border,
            Location = new Point(x, y),
            Size = new Size(width, height)
        };
    }

    private Button CreateModernButton(string text, Color color, Size size)
    {
        var button = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = color,
            FlatStyle = FlatStyle.Flat,
            Size = size,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.12f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.10f);

        return button;
    }

    private void SetActionButton(Button? button, bool enabled, Color activeColor)
    {
        if (button is null)
            return;

        button.Enabled = enabled;
        button.BackColor = enabled ? activeColor : Disabled;
        button.ForeColor = enabled ? Color.White : DisabledText;
        button.Cursor = enabled ? Cursors.Hand : Cursors.No;
        button.FlatAppearance.MouseOverBackColor = enabled ? ControlPaint.Light(activeColor, 0.12f) : Disabled;
        button.FlatAppearance.MouseDownBackColor = enabled ? ControlPaint.Dark(activeColor, 0.10f) : Disabled;
    }

    private Button CreateHeaderButton(string text, int x, int y)
    {
        var button = new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(x, y),
            Size = new Size(95, 34),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Panel;

        return button;
    }

    private LinkLabel CreateTopLink(string text, int x, int y)
    {
        return new LinkLabel
        {
            Text = text,
            LinkColor = TextMain,
            ActiveLinkColor = BlueLight,
            VisitedLinkColor = TextMain,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    private void SelectFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = L("select_title")
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        _selectedFolder = dialog.SelectedPath;
        _scanResult = null;

        _folderLabel!.Text = $"{L("selected_folder")}\n{_selectedFolder}";
        _detectedLabel!.Text = $"{L("detected")} {DetectCollection(_selectedFolder)}";

        SetActionButton(_scanButton, true, Blue);
        SetActionButton(_exportButton, false, Green);

        _statusLabel!.Text = L("folder_selected");
        _reportBox!.Text = L("folder_report");

        SetStats(0, 0, 0, 0);

        if (_progress is not null)
            _progress.Value = 0;
    }

    private void ScanFolder()
    {
        if (string.IsNullOrWhiteSpace(_selectedFolder))
            return;

        if (!ConfirmCommunityFolderScan())
            return;

        try
        {
            _statusLabel!.Text = L("scanning");
            _progress!.Value = 25;
            Application.DoEvents();

            var scanner = new AircraftScanner();
            _scanResult = scanner.Scan(_selectedFolder);

            _progress.Value = 100;
            _statusLabel.Text = L("scan_complete");

            SetActionButton(_exportButton, _scanResult.UsableRules > 0, Green);

            SetStats(
                _scanResult.AircraftCfgFiles,
                _scanResult.ModelsFound,
                _scanResult.WithTypeCode,
                _scanResult.UsableRules);

            BuildReport();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, L("scan_failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            _statusLabel!.Text = L("scan_failed");
        }
    }

    private void BuildReport()
    {
        if (_scanResult is null)
            return;

        var lines = new List<string>
        {
            L("scan_complete_short"),
            $"{L("root")} {_selectedFolder}",
            $"{L("found_cfg")} {_scanResult.AircraftCfgFiles}",
            $"{L("found_models")} {_scanResult.ModelsFound}",
            $"{L("usable_rules")} {_scanResult.UsableRules}",
            "",
            L("scan_tip"),
            "",
            L("vmr_rules")
        };

        foreach (var entry in _scanResult.Entries.Where(entry => entry.IsUsable))
            lines.Add($"- {entry.AirlineCode} / {entry.TypeCode} / {entry.Title}");

        _reportBox!.Text = string.Join(Environment.NewLine, lines);
    }

    private void ExportVmr()
    {
        if (_scanResult is null || _scanResult.UsableRules == 0)
            return;

        using var dialog = new SaveFileDialog
        {
            Title = L("export_title"),
            Filter = "VMR files (*.vmr)|*.vmr|XML files (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = $"TrafficMatchBuilder_{DetectCollection(_selectedFolder)}_{DateTime.Now:yyyyMMdd_HHmm}.vmr"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var exporter = new VmrExporter();
            exporter.Export(dialog.FileName, _scanResult.Entries);

            _statusLabel!.Text = $"{L("export_success")} {dialog.FileName}";
            MessageBox.Show(
                $"{L("export_success")}{Environment.NewLine}{Environment.NewLine}{dialog.FileName}",
                L("export_complete"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, L("export_failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private bool ConfirmCommunityFolderScan()
    {
        var folderName = Path.GetFileName(_selectedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        if (!string.Equals(folderName, "Community", StringComparison.OrdinalIgnoreCase))
            return true;

        var result = MessageBox.Show(
            L("community_warning_text"),
            L("community_warning_title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }

    private void ShowAboutWindow()
    {
        _aboutLogoClickCount = 0;

        using var about = new Form
        {
            Text = "About TrafficMatch Builder",
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(620, 520),
            MinimumSize = new Size(560, 480),
            BackColor = Bg,
            ForeColor = TextMain,
            Font = new Font("Segoe UI", 9F),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var title = new Label
        {
            Text = $"TrafficMatch Builder {AppVersion}",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 64
        };

        var logo = new Label
        {
            Text = "✈  ⚙  </>",
            Font = new Font("Segoe UI", 25, FontStyle.Bold),
            ForeColor = BlueLight,
            BackColor = PanelDark,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 76,
            Cursor = Cursors.Hand
        };

        logo.Click += (_, _) =>
        {
            _aboutLogoClickCount++;

            if (_aboutLogoClickCount == 10)
            {
                MessageBox.Show(
                    "Achievement Unlocked\r\n\r\n" +
                    "Certified Traffic Matching Engineer\r\n\r\n" +
                    "You survived:\r\n\r\n" +
                    "• XML files\r\n" +
                    "• model matching\r\n" +
                    "• VMR generation\r\n" +
                    "• VirusTotal false positives\r\n\r\n" +
                    "Welcome to the maintenance department.\r\n\r\n" +
                    "Coffee is mandatory.",
                    "Achievement Unlocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        };

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Bg,
            Padding = new Padding(28, 22, 28, 18)
        };

        body.Controls.Add(new Label
        {
            Text = L("about_subtitle"),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = TextMuted,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(540, 26),
            Location = new Point(28, 18)
        });

        body.Controls.Add(new Label
        {
            Text = L("developer"),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            AutoSize = true,
            Location = new Point(28, 58)
        });

        body.Controls.Add(new Label
        {
            Text = "Nils",
            Font = new Font("Segoe UI", 10),
            ForeColor = TextMuted,
            BackColor = Bg,
            AutoSize = true,
            Location = new Point(28, 84)
        });

        body.Controls.Add(new Label
        {
            Text = L("project_links"),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            AutoSize = true,
            Location = new Point(28, 124)
        });

        var flightsimButton = CreateModernButton(L("open_flightsim"), Blue, new Size(180, 42));
        flightsimButton.Location = new Point(28, 154);
        flightsimButton.Click += (_, _) => OpenExternalLink("https://flightsim.to/addon/109917/trafficmatch-builder-msfs-traffic-library-vmr-generator");
        body.Controls.Add(flightsimButton);

        var githubButton = CreateModernButton(L("open_github"), Color.FromArgb(31, 41, 55), new Size(180, 42));
        githubButton.Location = new Point(222, 154);
        githubButton.Click += (_, _) => OpenExternalLink("https://github.com/Loading-fs/TrafficMatchBuilder");
        body.Controls.Add(githubButton);

        body.Controls.Add(new Label
        {
            Text = L("about_credits_title"),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            AutoSize = true,
            Location = new Point(28, 220)
        });

        body.Controls.Add(new Label
        {
            Text = L("about_credits_text"),
            Font = new Font("Segoe UI", 10),
            ForeColor = TextMuted,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(540, 48),
            Location = new Point(28, 246)
        });

        body.Controls.Add(new Label
        {
            Text = $"{L("about_disclaimer")}\r\n© 2026 TrafficMatch Builder",
            Font = new Font("Segoe UI", 9),
            ForeColor = TextSoft,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(540, 44),
            Location = new Point(28, 316)
        });

        var close = CreateModernButton(L("close"), Blue, new Size(120, 42));
        close.Dock = DockStyle.Bottom;
        close.Click += (_, _) => about.Close();

        about.Controls.Add(body);
        about.Controls.Add(close);
        about.Controls.Add(logo);
        about.Controls.Add(title);

        about.ShowDialog(this);
    }

    private static void OpenExternalLink(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Could not open link",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }





    private void ShowReportMenu()
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add(L("copy_report"), null, (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(_reportBox?.Text))
                Clipboard.SetText(_reportBox.Text);
        });

        menu.Items.Add(L("save_report"), null, (_, _) =>
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = "TrafficMatchBuilder_Report.txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dialog.FileName, _reportBox?.Text ?? "");
        });

        menu.Show(this, PointToClient(Cursor.Position));
    }

    private void SetStats(int cfg, int models, int type, int rules)
    {
        var values = new[] { cfg, models, type, rules };

        for (var i = 0; i < _statNumbers.Count; i++)
            _statNumbers[i].Text = values[i].ToString();
    }

    private static string DetectCollection(string path)
    {
        var lowerPath = path.ToLowerInvariant();

        if (lowerPath.Contains("fsltl"))
            return "FSLTL";

        if (lowerPath.Contains("aig"))
            return "AIG";

        return "-";
    }
}

public sealed class GradientPanel : Panel
{
    public Color StartColor { get; set; } = Color.FromArgb(11, 18, 32);
    public Color EndColor { get; set; } = Color.FromArgb(15, 23, 42);

    public GradientPanel()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new LinearGradientBrush(ClientRectangle, StartColor, EndColor, LinearGradientMode.Vertical);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

public sealed class HoverPanel : Panel
{
    public Color BorderColor { get; set; } = Color.FromArgb(51, 65, 85);

    public HoverPanel()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(BorderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}

public sealed class HeaderBanner : Panel
{
    public HeaderBanner()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = ClientRectangle;

        using var bgBrush = new LinearGradientBrush(
            rect,
            Color.FromArgb(18, 32, 55),
            Color.FromArgb(7, 15, 28),
            LinearGradientMode.Horizontal);

        using var borderPen = new Pen(Color.FromArgb(35, 95, 150), 1);

        e.Graphics.FillRectangle(bgBrush, rect);
        e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        var logoRect = new Rectangle(48, 28, 116, 116);

        using var circleBrush = new SolidBrush(Color.FromArgb(25, 55, 90));
        using var circlePen = new Pen(Color.FromArgb(56, 189, 248), 3);

        e.Graphics.FillEllipse(circleBrush, logoRect);
        e.Graphics.DrawEllipse(circlePen, logoRect);

        using var planePen = new Pen(Color.White, 4);

        e.Graphics.DrawLine(planePen, 82, 85, 140, 85);
        e.Graphics.DrawLine(planePen, 112, 62, 140, 85);
        e.Graphics.DrawLine(planePen, 112, 108, 140, 85);

        using var arcPen = new Pen(Color.FromArgb(56, 189, 248), 4);
        e.Graphics.DrawArc(arcPen, 70, 68, 58, 48, 130, 110);

        using var vmrFont = new Font("Segoe UI", 13, FontStyle.Bold);
        using var whiteBrush = new SolidBrush(Color.White);

        e.Graphics.DrawString("VMR", vmrFont, whiteBrush, 92, 100);

        using var titleFont = new Font("Segoe UI", 34, FontStyle.Bold);
        using var subtitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
        using var blueBrush = new SolidBrush(Color.FromArgb(56, 189, 248));
        using var mutedBrush = new SolidBrush(Color.FromArgb(180, 200, 230));

        e.Graphics.DrawString("TrafficMatch", titleFont, whiteBrush, 200, 42);
        e.Graphics.DrawString("Builder", titleFont, blueBrush, 510, 42);
        e.Graphics.DrawString("VMR Generator for vPilot & VATSIM  •  v0.3 Beta", subtitleFont, mutedBrush, 205, 105);
        e.Graphics.DrawString("Scan  •  Match  •  Export", subtitleFont, blueBrush, 205, 132);
    }
}
