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
    private const string AppVersion = "v0.6 Beta";

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
    private string _communityFolder = "";
    private string _selectedFolder = "";
    private string _baseAircraftFolder = "";
    private ScanResult? _scanResult;

    private Label? _communityLabel;
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
        Size = new Size(1260, 900);
        MinimumSize = new Size(1180, 860);
        BackColor = Bg;
        ForeColor = TextMain;
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        try
        {
            var iconPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "icon.ico");

            if (File.Exists(iconPath))
                Icon = new Icon(iconPath);
        }
        catch
        {
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
            ["steps"] = "Step 1: Community Folder   →   Step 2: Traffic / Addon Folder   →   Step 3: Scan   →   Step 4: Export",
            ["community_title"] = "Step 1: Select your MSFS Community folder",
            ["selected_community"] = "Selected Community folder:",
            ["select_community"] = "▰  Select Community Folder",
            ["source_title"] = "Step 2: Select traffic library or supported addon livery",
            ["source_hint"] = "Now choose what you want to scan. This can be FSLTL, AIG, a traffic package, a Fenix livery folder or a PMDG livery folder.",
            ["select_title"] = "Select scan source",
            ["selected_folder"] = "Selected scan source:",
            ["detected"] = "Detected source:",
            ["select_folder"] = "▰  Select Traffic / Livery Folder",
            ["auto_community"] = "⌂  Auto Detect",
            ["community_not_found"] = "No MSFS Community folder was found automatically. Please select it manually.",
            ["scan_models"] = "⌕  Scan Models",
            ["export_vmr"] = "▤  Export VMR",
            ["workflow_hint"] = "First select your MSFS Community folder. TrafficMatch Builder uses it to find Fenix, PMDG and other related packages automatically.",
            ["help_hint"] = "Need help? Click ? Help in the top right corner for a quick tutorial.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Models",
            ["stat_type"] = "With TypeCode",
            ["stat_rules"] = "VMR Rules",
            ["ready"] = "Ready.",
            ["report_title"] = "Scan report",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} ready.",
            ["folder_selected"] = "Scan source selected. Ready to scan.",
            ["folder_report"] = "Scan source selected. Click Scan Models to discover traffic models, supported addon liveries and usable VMR rules.",
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
            ["about_credits_text"] = "Thanks to all beta testers, the VATSIM community and everyone providing feedback and bug reports.\n\nTrademark notice: TrafficMatch Builder is an independent community project and is not affiliated with, endorsed by or sponsored by Fenix, PMDG, VATSIM, vPilot, Microsoft or Asobo Studio. All trademarks and product names belong to their respective owners.",
            ["about_disclaimer"] = "Not affiliated with Microsoft, Asobo Studio, VATSIM, vPilot, Fenix or PMDG. All trademarks belong to their respective owners.",
            ["close"] = "Close",
            ["community_warning_title"] = "Community Folder Warning",
            ["community_warning_text"] = "You selected what appears to be the full Microsoft Flight Simulator Community folder as the scan source.\n\nFor best results, select a supported traffic package such as FSLTL, AIG or another dedicated AI traffic library.\n\nYou may also select a single supported addon livery or livery folder, such as Fenix A320, PMDG 737 or PMDG 777. Keep in mind that full-fidelity addon aircraft can use significantly more performance than dedicated traffic models.\n\nScanning the full Community folder can take longer and may include unrelated scenery, tools, effects, aircraft or configuration files. This can lead to confusing scan results or less useful VMR output.\n\nDo you want to continue scanning the full Community folder anyway?",
            ["scan_tip"] = "Tip: Select a dedicated traffic package or supported addon livery for the clearest results. The Community folder helps TrafficMatch Builder resolve related Fenix and PMDG packages automatically."
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
            ["steps"] = "Schritt 1: Community-Ordner   →   Schritt 2: Traffic-/Addon-Ordner   →   Schritt 3: Scannen   →   Schritt 4: Exportieren",
            ["community_title"] = "Schritt 1: MSFS Community-Ordner auswählen",
            ["selected_community"] = "Ausgewählter Community-Ordner:",
            ["select_community"] = "▰  Community-Ordner wählen",
            ["source_title"] = "Schritt 2: Traffic-Library oder unterstützte Addon-Livery auswählen",
            ["source_hint"] = "Wähle jetzt aus, was gescannt werden soll. Das kann FSLTL, AIG, ein Traffic-Paket, ein Fenix-Livery-Ordner oder ein PMDG-Livery-Ordner sein.",
            ["select_title"] = "Scan-Quelle auswählen",
            ["selected_folder"] = "Ausgewählte Scan-Quelle:",
            ["detected"] = "Erkannte Quelle:",
            ["select_folder"] = "▰  Traffic / Livery wählen",
            ["auto_community"] = "⌂  Auto Detect",
            ["community_not_found"] = "Es wurde kein MSFS Community-Ordner automatisch gefunden. Bitte wähle ihn manuell aus.",
            ["scan_models"] = "⌕  Modelle scannen",
            ["export_vmr"] = "▤  VMR exportieren",
            ["workflow_hint"] = "Wähle zuerst deinen MSFS Community-Ordner. TrafficMatch Builder nutzt ihn später, um Fenix, PMDG und passende Zusatzpakete automatisch zu finden.",
            ["help_hint"] = "Brauchst du Hilfe? Klicke oben rechts auf ? Hilfe für ein kurzes Tutorial.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modelle",
            ["stat_type"] = "Mit TypeCode",
            ["stat_rules"] = "VMR-Regeln",
            ["ready"] = "Bereit.",
            ["report_title"] = "Scanbericht",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} bereit.",
            ["folder_selected"] = "Scan-Quelle ausgewählt. Bereit zum Scannen.",
            ["folder_report"] = "Scan-Quelle ausgewählt. Klicke auf Modelle scannen, um Traffic-Modelle, unterstützte Addon-Liveries und nutzbare VMR-Regeln zu erkennen.",
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
            ["about_credits_text"] = "Danke an alle Beta-Tester, die VATSIM-Community und alle, die Feedback und Fehlermeldungen liefern.\n\nMarkenhinweis: TrafficMatch Builder ist ein unabhängiges Community-Projekt und steht in keiner Verbindung zu Fenix, PMDG, VATSIM, vPilot, Microsoft oder Asobo Studio. Alle Marken und Produktnamen gehören ihren jeweiligen Eigentümern.",
            ["about_disclaimer"] = "Nicht verbunden mit Microsoft, Asobo Studio, VATSIM, vPilot, Fenix oder PMDG. Alle Marken gehören ihren jeweiligen Eigentümern.",
            ["close"] = "Schließen",
            ["community_warning_title"] = "Community-Ordner-Warnung",
            ["community_warning_text"] = "Du hast offenbar den kompletten Microsoft Flight Simulator Community-Ordner als Scan-Quelle ausgewählt.\n\nFür die besten Ergebnisse solltest du ein unterstütztes Traffic-Paket auswählen, zum Beispiel FSLTL, AIG oder eine andere dedizierte AI-Traffic-Bibliothek.\n\nAlternativ kannst du auch eine einzelne unterstützte Addon-Livery oder einen Livery-Ordner auswählen, zum Beispiel Fenix A320, PMDG 737 oder PMDG 777. Beachte dabei: Full-Fidelity-Addon-Flugzeuge können deutlich mehr Performance kosten als reine Traffic-Modelle.\n\nDas Scannen des kompletten Community-Ordners kann länger dauern und auch fremde Szenerien, Tools, Effekte, Flugzeuge oder Konfigurationsdateien einschließen. Dadurch können unübersichtliche Scan-Ergebnisse oder weniger sinnvolle VMR-Dateien entstehen.\n\nMöchtest du den kompletten Community-Ordner trotzdem scannen?",
            ["scan_tip"] = "Tipp: Wähle für klare Ergebnisse ein konkretes Traffic-Paket oder eine unterstützte Addon-Livery. Der Community-Ordner hilft TrafficMatch Builder dabei, passende Fenix- und PMDG-Pakete automatisch aufzulösen."
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
            ["steps"] = "Steg 1: Community-mappe   →   Steg 2: Trafikk-/addon-mappe   →   Steg 3: Skann   →   Steg 4: Eksporter",
            ["community_title"] = "Steg 1: Velg MSFS Community-mappe",
            ["selected_community"] = "Valgt Community-mappe:",
            ["select_community"] = "▰  Velg Community-mappe",
            ["source_title"] = "Steg 2: Velg trafikkbibliotek eller støttet addon-livery",
            ["source_hint"] = "Velg nå hva som skal skannes. Det kan være FSLTL, AIG, en trafikkpakke, en Fenix-liverymappe eller en PMDG-liverymappe.",
            ["select_title"] = "Velg skannekilde",
            ["selected_folder"] = "Valgt skannekilde:",
            ["detected"] = "Oppdaget kilde:",
            ["select_folder"] = "▰  Velg trafikk / livery",
            ["auto_community"] = "⌂  Auto Detect",
            ["community_not_found"] = "Fant ingen MSFS Community-mappe automatisk. Velg den manuelt.",
            ["scan_models"] = "⌕  Skann oppdagede pakker",
            ["export_vmr"] = "▤  Eksporter VMR",
            ["workflow_hint"] = "Velg MSFS Community-mappen først. TrafficMatch Builder bruker den senere til å finne Fenix, PMDG og tilhørende pakker automatisk.",
            ["help_hint"] = "Trenger du hjelp? Klikk på ? Hjelp øverst til høyre for en kort veiledning.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modeller",
            ["stat_type"] = "Med TypeCode",
            ["stat_rules"] = "VMR-regler",
            ["ready"] = "Klar.",
            ["report_title"] = "Skannrapport",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} klar.",
            ["folder_selected"] = "Mappe valgt. Klar til skanning.",
            ["folder_report"] = "Skannekilde valgt. Klikk på Skann modeller for å finne trafikkmodeller, støttede addon-liveries og brukbare VMR-regler.",
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
            ["about_credits_text"] = "Takk til alle beta-testere, VATSIM-miljøet og alle som gir tilbakemeldinger og feilrapporter.\n\nVaremerkemerknad: TrafficMatch Builder er et uavhengig community-prosjekt og er ikke tilknyttet, godkjent av eller sponset av Fenix, PMDG, VATSIM, vPilot, Microsoft eller Asobo Studio. Alle varemerker og produktnavn tilhører sine respektive eiere.",
            ["about_disclaimer"] = "Ikke tilknyttet Microsoft, Asobo Studio, VATSIM, vPilot, Fenix eller PMDG. Alle varemerker tilhører sine respektive eiere.",
            ["close"] = "Lukk",
            ["community_warning_title"] = "Advarsel om Community-mappen",
            ["community_warning_text"] = "Du har tilsynelatende valgt hele Microsoft Flight Simulator Community-mappen som skannekilde.\n\nFor best resultat bør du velge en støttet trafikkpakke, for eksempel FSLTL, AIG eller et annet dedikert AI-trafikkbibliotek.\n\nDu kan også velge en enkelt støttet addon-livery eller liverymappe, for eksempel Fenix A320, PMDG 737 eller PMDG 777. Husk at full-fidelity addon-fly kan bruke betydelig mer ytelse enn dedikerte trafikkmodeller.\n\nSkanning av hele Community-mappen kan ta lengre tid og kan inkludere urelaterte scenery-pakker, verktøy, effekter, fly eller konfigurasjonsfiler. Dette kan gi forvirrende skannresultater eller mindre nyttige VMR-filer.\n\nVil du fortsette å skanne hele Community-mappen likevel?",
            ["scan_tip"] = "Tips: Velg helst en konkret trafikkpakke eller støttet addon-livery. Community-mappen hjelper TrafficMatch Builder med å finne tilhørende Fenix- og PMDG-pakker automatisk."
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
            ["steps"] = "Étape 1 : dossier Community   →   Étape 2 : dossier trafic/addon   →   Étape 3 : scanner   →   Étape 4 : exporter",
            ["community_title"] = "Étape 1 : choisir le dossier Community MSFS",
            ["selected_community"] = "Dossier Community sélectionné :",
            ["select_community"] = "▰  Choisir dossier Community",
            ["source_title"] = "Étape 2 : choisir une bibliothèque de trafic ou une livrée addon prise en charge",
            ["source_hint"] = "Choisissez maintenant ce qui doit être scanné : FSLTL, AIG, un package de trafic, un dossier de livrée Fenix ou un dossier de livrée PMDG.",
            ["select_title"] = "Choisir la source du scan",
            ["selected_folder"] = "Source de scan sélectionnée :",
            ["detected"] = "Source détectée :",
            ["select_folder"] = "▰  Choisir trafic / livrée",
            ["auto_community"] = "⌂  Détection auto",
            ["community_not_found"] = "Aucun dossier Community MSFS n’a été trouvé automatiquement. Sélectionnez-le manuellement.",
            ["scan_models"] = "⌕  Scanner modèles",
            ["export_vmr"] = "▤  Exporter VMR",
            ["workflow_hint"] = "Sélectionnez d’abord votre dossier Community MSFS. TrafficMatch Builder l’utilise ensuite pour trouver automatiquement Fenix, PMDG et les packages associés.",
            ["help_hint"] = "Besoin d'aide ? Cliquez sur ? Aide en haut à droite pour un court tutoriel.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modèles",
            ["stat_type"] = "Avec TypeCode",
            ["stat_rules"] = "Règles VMR",
            ["ready"] = "Prêt.",
            ["report_title"] = "Rapport de scan",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} prêt.",
            ["folder_selected"] = "Dossier sélectionné. Prêt à scanner.",
            ["folder_report"] = "Source de scan sélectionnée. Cliquez sur Scanner modèles pour trouver les modèles de trafic, les livrées prises en charge et les règles VMR utilisables.",
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
            ["about_credits_text"] = "Merci à tous les bêta-testeurs, à la communauté VATSIM et à toutes les personnes qui envoient des retours et des rapports de bugs.\n\nMention de marques : TrafficMatch Builder est un projet communautaire indépendant et n’est pas affilié, approuvé ou sponsorisé par Fenix, PMDG, VATSIM, vPilot, Microsoft ou Asobo Studio. Toutes les marques et tous les noms de produits appartiennent à leurs propriétaires respectifs.",
            ["about_disclaimer"] = "Non affilié à Microsoft, Asobo Studio, VATSIM, vPilot, Fenix ou PMDG. Toutes les marques appartiennent à leurs propriétaires respectifs.",
            ["close"] = "Fermer",
            ["community_warning_title"] = "Avertissement dossier Community",
            ["community_warning_text"] = "Vous semblez avoir sélectionné le dossier Community complet de Microsoft Flight Simulator comme source de scan.\n\nPour de meilleurs résultats, sélectionnez un package de trafic pris en charge, par exemple FSLTL, AIG ou une autre bibliothèque de trafic AI dédiée.\n\nVous pouvez également sélectionner une livrée addon prise en charge ou un dossier de livrées, par exemple Fenix A320, PMDG 737 ou PMDG 777. Gardez à l’esprit que les avions addon full-fidelity peuvent consommer beaucoup plus de ressources que les modèles de trafic dédiés.\n\nScanner tout le dossier Community peut prendre plus de temps et inclure des scènes, outils, effets, avions ou fichiers de configuration sans rapport. Cela peut produire des résultats confus ou un fichier VMR moins utile.\n\nVoulez-vous tout de même continuer à scanner le dossier Community complet ?",
            ["scan_tip"] = "Astuce : sélectionnez de préférence un package de trafic précis ou une livrée addon prise en charge. Le dossier Community aide TrafficMatch Builder à résoudre automatiquement les packages Fenix et PMDG associés."
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
            ["steps"] = "Paso 1: carpeta Community   →   Paso 2: carpeta tráfico/addon   →   Paso 3: escanear   →   Paso 4: exportar",
            ["community_title"] = "Paso 1: seleccionar la carpeta Community de MSFS",
            ["selected_community"] = "Carpeta Community seleccionada:",
            ["select_community"] = "▰  Seleccionar carpeta Community",
            ["source_title"] = "Paso 2: seleccionar biblioteca de tráfico o livery addon compatible",
            ["source_hint"] = "Elige ahora qué quieres escanear: FSLTL, AIG, un paquete de tráfico, una carpeta de livery Fenix o una carpeta de livery PMDG.",
            ["select_title"] = "Seleccionar fuente de escaneo",
            ["selected_folder"] = "Fuente de escaneo seleccionada:",
            ["detected"] = "Fuente detectada:",
            ["select_folder"] = "▰  Seleccionar tráfico / livery",
            ["auto_community"] = "⌂  Auto Detect",
            ["community_not_found"] = "No se encontró automáticamente ninguna carpeta Community de MSFS. Selecciónala manualmente.",
            ["scan_models"] = "⌕  Escanear paquetes detectados",
            ["export_vmr"] = "▤  Exportar VMR",
            ["workflow_hint"] = "Selecciona primero la carpeta Community de MSFS. TrafficMatch Builder la usa después para encontrar automáticamente Fenix, PMDG y paquetes relacionados.",
            ["help_hint"] = "¿Necesitas ayuda? Haz clic en ? Ayuda arriba a la derecha para ver un tutorial breve.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modelos",
            ["stat_type"] = "Con TypeCode",
            ["stat_rules"] = "Reglas VMR",
            ["ready"] = "Listo.",
            ["report_title"] = "Informe de escaneo",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} listo.",
            ["folder_selected"] = "Carpeta seleccionada. Listo para escanear.",
            ["folder_report"] = "Fuente de escaneo seleccionada. Haz clic en Escanear modelos para encontrar modelos de tráfico, liveries compatibles y reglas VMR utilizables.",
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
            ["about_credits_text"] = "Gracias a todos los beta testers, a la comunidad de VATSIM y a todos los que envían comentarios e informes de errores.\n\nAviso de marcas: TrafficMatch Builder es un proyecto comunitario independiente y no está afiliado, respaldado ni patrocinado por Fenix, PMDG, VATSIM, vPilot, Microsoft o Asobo Studio. Todas las marcas y nombres de productos pertenecen a sus respectivos propietarios.",
            ["about_disclaimer"] = "No está afiliado a Microsoft, Asobo Studio, VATSIM, vPilot, Fenix ni PMDG. Todas las marcas pertenecen a sus respectivos propietarios.",
            ["close"] = "Cerrar",
            ["community_warning_title"] = "Advertencia sobre la carpeta Community",
            ["community_warning_text"] = "Parece que has seleccionado toda la carpeta Community de Microsoft Flight Simulator como fuente de escaneo.\n\nPara obtener mejores resultados, selecciona un paquete de tráfico compatible, como FSLTL, AIG u otra biblioteca dedicada de tráfico AI.\n\nTambién puedes seleccionar una livery addon compatible o una carpeta de liveries, por ejemplo Fenix A320, PMDG 737 o PMDG 777. Ten en cuenta que los aviones addon full-fidelity pueden consumir muchos más recursos que los modelos de tráfico dedicados.\n\nEscanear toda la carpeta Community puede tardar más e incluir escenarios, herramientas, efectos, aviones o archivos de configuración no relacionados. Esto puede generar resultados confusos o un archivo VMR menos útil.\n\n¿Quieres continuar escaneando toda la carpeta Community de todos modos?",
            ["scan_tip"] = "Consejo: selecciona preferiblemente un paquete de tráfico concreto o una livery addon compatible. La carpeta Community ayuda a TrafficMatch Builder a resolver automáticamente paquetes Fenix y PMDG relacionados."
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
            ["steps"] = "Passo 1: cartella Community   →   Passo 2: cartella traffico/addon   →   Passo 3: scansiona   →   Passo 4: esporta",
            ["community_title"] = "Passo 1: seleziona la cartella Community MSFS",
            ["selected_community"] = "Cartella Community selezionata:",
            ["select_community"] = "▰  Seleziona cartella Community",
            ["source_title"] = "Passo 2: seleziona libreria traffico o livery addon supportata",
            ["source_hint"] = "Ora scegli cosa scansionare: FSLTL, AIG, un pacchetto traffico, una cartella livery Fenix o una cartella livery PMDG.",
            ["select_title"] = "Seleziona sorgente scansione",
            ["selected_folder"] = "Sorgente scansione selezionata:",
            ["detected"] = "Sorgente rilevata:",
            ["select_folder"] = "▰  Seleziona traffico / livery",
            ["auto_community"] = "⌂  Auto Detect",
            ["community_not_found"] = "Nessuna cartella Community MSFS trovata automaticamente. Selezionala manualmente.",
            ["scan_models"] = "⌕  Scansiona modelli",
            ["export_vmr"] = "▤  Esporta VMR",
            ["workflow_hint"] = "Seleziona prima la cartella Community MSFS. TrafficMatch Builder la usa poi per trovare automaticamente Fenix, PMDG e i pacchetti collegati.",
            ["help_hint"] = "Hai bisogno di aiuto? Clicca su ? Aiuto in alto a destra per un breve tutorial.",
            ["stat_cfg"] = "aircraft.cfg",
            ["stat_models"] = "Modelli",
            ["stat_type"] = "Con TypeCode",
            ["stat_rules"] = "Regole VMR",
            ["ready"] = "Pronto.",
            ["report_title"] = "Report scansione",
            ["ready_report"] = $"TrafficMatch Builder {AppVersion} pronto.",
            ["folder_selected"] = "Cartella selezionata. Pronto per la scansione.",
            ["folder_report"] = "Sorgente scansione selezionata. Clicca su Scansiona modelli per trovare modelli traffico, livery supportate e regole VMR utilizzabili.",
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
            ["about_credits_text"] = "Grazie a tutti i beta tester, alla community VATSIM e a chi fornisce feedback e segnalazioni di bug.\n\nNota sui marchi: TrafficMatch Builder è un progetto indipendente della community e non è affiliato, approvato o sponsorizzato da Fenix, PMDG, VATSIM, vPilot, Microsoft o Asobo Studio. Tutti i marchi e i nomi dei prodotti appartengono ai rispettivi proprietari.",
            ["about_disclaimer"] = "Non affiliato con Microsoft, Asobo Studio, VATSIM, vPilot, Fenix o PMDG. Tutti i marchi appartengono ai rispettivi proprietari.",
            ["close"] = "Chiudi",
            ["community_warning_title"] = "Avviso cartella Community",
            ["community_warning_text"] = "Sembra che tu abbia selezionato l'intera cartella Community di Microsoft Flight Simulator come sorgente di scansione.\n\nPer ottenere i risultati migliori, seleziona un pacchetto traffico supportato, per esempio FSLTL, AIG o un'altra libreria AI traffic dedicata.\n\nPuoi anche selezionare una singola livery addon supportata o una cartella di livery, per esempio Fenix A320, PMDG 737 o PMDG 777. Tieni presente che gli aerei addon full-fidelity possono consumare molte più risorse rispetto ai modelli traffico dedicati.\n\nLa scansione dell'intera cartella Community può richiedere più tempo e includere scenery, strumenti, effetti, aerei o file di configurazione non pertinenti. Questo può generare risultati confusi o file VMR meno utili.\n\nVuoi continuare comunque con la scansione dell'intera cartella Community?",
            ["scan_tip"] = "Suggerimento: seleziona preferibilmente un pacchetto traffico specifico o una livery addon supportata. La cartella Community aiuta TrafficMatch Builder a risolvere automaticamente i pacchetti Fenix e PMDG collegati."
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

        var extra = GetV04Text(lang, key);
        if (!string.IsNullOrWhiteSpace(extra))
            return extra;

        return dict.TryGetValue(key, out var value)
            ? value
            : en.TryGetValue(key, out var fallback)
                ? fallback
                : key;
    }

    private static string GetV04Text(string lang, string key)
    {
        var supportedText = GetSupportedPacksText(lang, key);
        if (!string.IsNullOrWhiteSpace(supportedText))
            return supportedText;

        var text = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new()
            {
                ["found_liveries"] = "Addon livery folders:",
                ["low_confidence"] = "Low confidence rules:",
                ["confidence"] = "Confidence",
                ["source"] = "Source",
                ["warning"] = "Warning",
                ["base_aircraft_folder"] = "Base aircraft folder:",
                ["base_models_missing_title"] = "Base aircraft models required",
                ["base_models_missing_text"] = "Addon liveries were found, but no confirmed aircraft.cfg ModelNames were found.\n\nTo create safe VMR rules, please select the matching base aircraft folder now, for example pmdg-aircraft-738, pmdg-aircraft-77w or the Fenix aircraft folder.\n\nWithout the base aircraft models, these livery names cannot be exported as confirmed VMR rules.",
                ["select_base_aircraft_folder"] = "Select the matching base aircraft folder",
                ["scanning_base_models"] = "Scanning base aircraft models...",
                ["base_models_still_missing"] = "Addon liveries were detected, but no confirmed ModelNames were found. Select the matching base aircraft folder and scan again before exporting.",
                ["modelname_warning"] = "ModelName could not be confirmed from aircraft.cfg. This rule is not exported until a matching base aircraft folder confirms the real ModelName.",
                ["auto_base_folders"] = "Auto-discovered base aircraft folders:",
                ["auto_livery_folders"] = "Auto-discovered livery folders:",
                ["auto_discovery_note"] = "Automatic discovery checked known MSFS package folders and matching PMDG/Fenix sibling folders.",
                ["auto_base_folder"] = "Auto base",
                ["auto_livery_folder"] = "Auto livery",
                ["addon_scan_note"] = "Addon liveries are scanned in addition to classic traffic libraries. A livery title is not automatically a valid ModelName. If only liveries are selected, the matching base aircraft folder is needed for safe export.",
                ["scan_summary"] = "Scan summary",
                ["paths_and_discovery"] = "Paths and discovery",
                ["source_overview"] = "Detected source overview:",
                ["source_none"] = "No supported aircraft source was detected.",
                ["source_models"] = "models",
                ["source_exportable"] = "exportable",
                ["source_low_confidence"] = "low confidence"
            },
            ["de"] = new()
            {
                ["found_liveries"] = "Addon-Livery-Ordner:",
                ["low_confidence"] = "Regeln mit niedriger Sicherheit:",
                ["confidence"] = "Sicherheit",
                ["source"] = "Quelle",
                ["warning"] = "Warnung",
                ["base_aircraft_folder"] = "Basisflugzeug-Ordner:",
                ["base_models_missing_title"] = "Basisflugzeug-Modelle erforderlich",
                ["base_models_missing_text"] = "Addon-Liveries wurden gefunden, aber keine bestätigten aircraft.cfg-ModelNames.\n\nDamit sichere VMR-Regeln entstehen, wähle jetzt bitte den passenden Basisflugzeug-Ordner aus, zum Beispiel pmdg-aircraft-738, pmdg-aircraft-77w oder den Fenix-Flugzeugordner.\n\nOhne die Basisflugzeug-Modelle können diese Livery-Namen nicht als bestätigte VMR-Regeln exportiert werden.",
                ["select_base_aircraft_folder"] = "Passenden Basisflugzeug-Ordner auswählen",
                ["scanning_base_models"] = "Basisflugzeug-Modelle werden gescannt...",
                ["base_models_still_missing"] = "Addon-Liveries wurden erkannt, aber keine bestätigten ModelNames gefunden. Wähle den passenden Basisflugzeug-Ordner und scanne erneut, bevor du exportierst.",
                ["modelname_warning"] = "ModelName konnte nicht über aircraft.cfg bestätigt werden. Diese Regel wird erst exportiert, wenn ein passender Basisflugzeug-Ordner den echten ModelName bestätigt.",
                ["auto_base_folders"] = "Automatisch gefundene Basisflugzeug-Ordner:",
                ["auto_livery_folders"] = "Automatisch gefundene Livery-Ordner:",
                ["auto_discovery_note"] = "Die automatische Suche hat bekannte MSFS-Package-Ordner und passende PMDG/Fenix-Nachbarordner geprüft.",
                ["auto_base_folder"] = "Auto-Basis",
                ["auto_livery_folder"] = "Auto-Livery",
                ["addon_scan_note"] = "Addon-Liveries werden zusätzlich zu klassischen Traffic-Bibliotheken gescannt. Ein Livery-Titel ist nicht automatisch ein gültiger ModelName. Wenn nur Liveries ausgewählt wurden, wird für den sicheren Export der passende Basisflugzeug-Ordner benötigt.",
                ["scan_summary"] = "Scan-Zusammenfassung",
                ["paths_and_discovery"] = "Pfade und Erkennung",
                ["source_overview"] = "Erkannte Quellenübersicht:",
                ["source_none"] = "Keine unterstützte Flugzeugquelle erkannt.",
                ["source_models"] = "Modelle",
                ["source_exportable"] = "exportierbar",
                ["source_low_confidence"] = "niedrige Sicherheit"
            },
            ["no"] = new()
            {
                ["found_liveries"] = "Mapper med addon-liveries:",
                ["low_confidence"] = "Regler med lav sikkerhet:",
                ["confidence"] = "Sikkerhet",
                ["source"] = "Kilde",
                ["warning"] = "Advarsel",
                ["base_aircraft_folder"] = "Basisflymappe:",
                ["base_models_missing_title"] = "Basisflymodeller kreves",
                ["base_models_missing_text"] = "Addon-liveries ble funnet, men ingen bekreftede aircraft.cfg-ModelNames ble funnet.\n\nFor å lage sikre VMR-regler må du velge riktig basisflymappe nå, for eksempel pmdg-aircraft-738, pmdg-aircraft-77w eller Fenix-flymappen.\n\nUten basisflymodellene kan disse livery-navnene ikke eksporteres som bekreftede VMR-regler.",
                ["select_base_aircraft_folder"] = "Velg riktig basisflymappe",
                ["scanning_base_models"] = "Skanner basisflymodeller...",
                ["base_models_still_missing"] = "Addon-liveries ble funnet, men ingen bekreftede ModelNames ble funnet. Velg riktig basisflymappe og skann på nytt før eksport.",
                ["modelname_warning"] = "ModelName kunne ikke bekreftes fra aircraft.cfg. Regelen eksporteres ikke før en passende basisflymappe bekrefter den ekte ModelName.",
                ["auto_base_folders"] = "Automatisk funnede basisflymapper:",
                ["auto_livery_folders"] = "Automatisk funnede livery-mapper:",
                ["auto_discovery_note"] = "Automatisk søk kontrollerte kjente MSFS-pakkemapper og matchende PMDG/Fenix-søskenmapper.",
                ["auto_base_folder"] = "Auto-basis",
                ["auto_livery_folder"] = "Auto-livery",
                ["addon_scan_note"] = "Addon-liveries skannes i tillegg til klassiske trafikkbiblioteker. En livery-tittel er ikke automatisk en gyldig ModelName. Hvis bare liveries er valgt, trengs riktig basisflymappe for sikker eksport.",
                ["scan_summary"] = "Skannsammendrag",
                ["paths_and_discovery"] = "Mapper og oppdagelse",
                ["source_overview"] = "Oversikt over oppdagede kilder:",
                ["source_none"] = "Ingen støttet flykilde ble oppdaget.",
                ["source_models"] = "modeller",
                ["source_exportable"] = "eksporterbare",
                ["source_low_confidence"] = "lav sikkerhet"
            },
            ["fr"] = new()
            {
                ["found_liveries"] = "Dossiers de livrées addon :",
                ["low_confidence"] = "Règles à faible confiance :",
                ["confidence"] = "Confiance",
                ["source"] = "Source",
                ["warning"] = "Avertissement",
                ["base_aircraft_folder"] = "Dossier avion de base :",
                ["base_models_missing_title"] = "Modèles avion de base requis",
                ["base_models_missing_text"] = "Des livrées addon ont été trouvées, mais aucun ModelName confirmé depuis aircraft.cfg.\n\nPour créer des règles VMR sûres, sélectionnez maintenant le dossier de l’avion de base correspondant, par exemple pmdg-aircraft-738, pmdg-aircraft-77w ou le dossier avion Fenix.\n\nSans les modèles de base, ces noms de livrées ne peuvent pas être exportés comme règles VMR confirmées.",
                ["select_base_aircraft_folder"] = "Sélectionner le dossier de l’avion de base correspondant",
                ["scanning_base_models"] = "Scan des modèles avion de base...",
                ["base_models_still_missing"] = "Des livrées addon ont été détectées, mais aucun ModelName confirmé n’a été trouvé. Sélectionnez le dossier de l’avion de base correspondant et relancez le scan avant l’export.",
                ["modelname_warning"] = "Le ModelName n’a pas pu être confirmé depuis aircraft.cfg. Cette règle n’est pas exportée tant qu’un dossier avion de base correspondant ne confirme pas le vrai ModelName.",
                ["auto_base_folders"] = "Dossiers avion de base trouvés automatiquement :",
                ["auto_livery_folders"] = "Dossiers de livrées trouvés automatiquement :",
                ["auto_discovery_note"] = "La recherche automatique a vérifié les dossiers de paquets MSFS connus et les dossiers frères PMDG/Fenix correspondants.",
                ["auto_base_folder"] = "Base auto",
                ["auto_livery_folder"] = "Livrée auto",
                ["addon_scan_note"] = "Les livrées addon sont scannées en plus des bibliothèques de trafic classiques. Un titre de livrée n’est pas automatiquement un ModelName valide. Si seules des livrées sont sélectionnées, le dossier de l’avion de base correspondant est nécessaire pour un export sûr.",
                ["scan_summary"] = "Résumé du scan",
                ["paths_and_discovery"] = "Dossiers et détection",
                ["source_overview"] = "Aperçu des sources détectées :",
                ["source_none"] = "Aucune source d’avion prise en charge détectée.",
                ["source_models"] = "modèles",
                ["source_exportable"] = "exportables",
                ["source_low_confidence"] = "faible confiance"
            },
            ["es"] = new()
            {
                ["found_liveries"] = "Carpetas de liveries addon:",
                ["low_confidence"] = "Reglas con baja confianza:",
                ["confidence"] = "Confianza",
                ["source"] = "Fuente",
                ["warning"] = "Advertencia",
                ["base_aircraft_folder"] = "Carpeta del avión base:",
                ["base_models_missing_title"] = "Se requieren modelos del avión base",
                ["base_models_missing_text"] = "Se encontraron liveries addon, pero no se encontraron ModelNames confirmados desde aircraft.cfg.\n\nPara crear reglas VMR seguras, selecciona ahora la carpeta del avión base correspondiente, por ejemplo pmdg-aircraft-738, pmdg-aircraft-77w o la carpeta del avión Fenix.\n\nSin los modelos base, estos nombres de livery no pueden exportarse como reglas VMR confirmadas.",
                ["select_base_aircraft_folder"] = "Seleccionar la carpeta del avión base correspondiente",
                ["scanning_base_models"] = "Escaneando modelos del avión base...",
                ["base_models_still_missing"] = "Se detectaron liveries addon, pero no se encontraron ModelNames confirmados. Selecciona la carpeta del avión base correspondiente y vuelve a escanear antes de exportar.",
                ["modelname_warning"] = "No se pudo confirmar el ModelName desde aircraft.cfg. Esta regla no se exporta hasta que una carpeta de avión base correspondiente confirme el ModelName real.",
                ["auto_base_folders"] = "Carpetas de avión base detectadas automáticamente:",
                ["auto_livery_folders"] = "Carpetas de liveries detectadas automáticamente:",
                ["auto_discovery_note"] = "La búsqueda automática revisó carpetas de paquetes MSFS conocidas y carpetas hermanas PMDG/Fenix coincidentes.",
                ["auto_base_folder"] = "Base auto",
                ["auto_livery_folder"] = "Livery auto",
                ["addon_scan_note"] = "Las liveries addon se escanean además de las bibliotecas de tráfico clásicas. Un título de livery no es automáticamente un ModelName válido. Si solo se seleccionan liveries, se necesita la carpeta del avión base correspondiente para una exportación segura.",
                ["scan_summary"] = "Resumen del escaneo",
                ["paths_and_discovery"] = "Rutas y detección",
                ["source_overview"] = "Resumen de fuentes detectadas:",
                ["source_none"] = "No se detectó ninguna fuente de aeronaves compatible.",
                ["source_models"] = "modelos",
                ["source_exportable"] = "exportables",
                ["source_low_confidence"] = "baja confianza"
            },
            ["it"] = new()
            {
                ["found_liveries"] = "Cartelle livery addon:",
                ["low_confidence"] = "Regole con bassa affidabilità:",
                ["confidence"] = "Affidabilità",
                ["source"] = "Fonte",
                ["warning"] = "Avviso",
                ["base_aircraft_folder"] = "Cartella aereo base:",
                ["base_models_missing_title"] = "Modelli aereo base necessari",
                ["base_models_missing_text"] = "Sono state trovate livery addon, ma nessun ModelName confermato da aircraft.cfg.\n\nPer creare regole VMR sicure, seleziona ora la cartella dell’aereo base corrispondente, per esempio pmdg-aircraft-738, pmdg-aircraft-77w o la cartella dell’aereo Fenix.\n\nSenza i modelli dell’aereo base, questi nomi livery non possono essere esportati come regole VMR confermate.",
                ["select_base_aircraft_folder"] = "Seleziona la cartella dell’aereo base corrispondente",
                ["scanning_base_models"] = "Scansione modelli aereo base...",
                ["base_models_still_missing"] = "Sono state rilevate livery addon, ma nessun ModelName confermato. Seleziona la cartella dell’aereo base corrispondente e ripeti la scansione prima dell’esportazione.",
                ["modelname_warning"] = "Il ModelName non è stato confermato da aircraft.cfg. Questa regola non viene esportata finché una cartella aereo base corrispondente non conferma il vero ModelName.",
                ["auto_base_folders"] = "Cartelle aereo base trovate automaticamente:",
                ["auto_livery_folders"] = "Cartelle livery trovate automaticamente:",
                ["auto_discovery_note"] = "La ricerca automatica ha controllato le cartelle pacchetto MSFS note e le cartelle PMDG/Fenix corrispondenti.",
                ["auto_base_folder"] = "Base auto",
                ["auto_livery_folder"] = "Livery auto",
                ["addon_scan_note"] = "Le livery addon vengono scansionate in aggiunta alle librerie traffico classiche. Il titolo di una livery non è automaticamente un ModelName valido. Se vengono selezionate solo livery, serve la cartella dell’aereo base corrispondente per un’esportazione sicura.",
                ["scan_summary"] = "Riepilogo scansione",
                ["paths_and_discovery"] = "Percorsi e rilevamento",
                ["source_overview"] = "Panoramica sorgenti rilevate:",
                ["source_none"] = "Nessuna sorgente aereo supportata rilevata.",
                ["source_models"] = "modelli",
                ["source_exportable"] = "esportabili",
                ["source_low_confidence"] = "bassa affidabilità"
            }
        };

        if (text.TryGetValue(lang, out var localized) && localized.TryGetValue(key, out var value))
            return value;
        return text["en"].TryGetValue(key, out var fallback) ? fallback : "";
    }


    private static string GetSupportedPacksText(string lang, string key)
    {
        var text = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new()
            {
                ["supported_packs"] = "Supported Packs",
                ["supported_title"] = "Supported Packs & Addons",
                ["supported_intro"] = "TrafficMatch Builder supports dedicated traffic libraries and selected addon livery packages.",
                ["traffic_libraries_title"] = "Supported Traffic Libraries",
                ["addon_liveries_title"] = "Supported Addon Liveries",
                ["custom_traffic_title"] = "Custom Traffic Libraries",
                ["custom_traffic_desc"] = "Other traffic libraries using standard aircraft.cfg model definitions may also work.",
                ["fsltl_desc"] = "Recommended traffic model library for good model matching coverage and simulator performance.",
                ["aig_desc"] = "AI traffic package and model library commonly used for model matching and offline traffic.",
                ["fenix_desc"] = "Supported for Fenix A320 Family addon livery scanning.",
                ["pmdg737_desc"] = "Supported for PMDG 737 Family addon livery scanning.",
                ["pmdg777_desc"] = "Supported for PMDG 777 Family addon livery scanning.",
                ["visit_website"] = "Visit Website",
                ["supported_note"] = "Recommendation: use FSLTL or AIG whenever possible. Full-fidelity addon aircraft such as Fenix and PMDG can use significantly more simulator performance and should mainly be used when the desired livery is not already available in a traffic library.",
                ["supported_legal"] = "TrafficMatch Builder is an independent community project. It is not affiliated with, endorsed by, sponsored by or officially connected to Fenix Simulations, PMDG, VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL or AIG. All trademarks and product names are the property of their respective owners."
            },
            ["de"] = new()
            {
                ["supported_packs"] = "Unterstützte Pakete",
                ["supported_title"] = "Unterstützte Pakete & Addons",
                ["supported_intro"] = "TrafficMatch Builder unterstützt dedizierte Traffic-Libraries und ausgewählte Addon-Livery-Pakete.",
                ["traffic_libraries_title"] = "Unterstützte Traffic-Libraries",
                ["addon_liveries_title"] = "Unterstützte Addon-Liveries",
                ["custom_traffic_title"] = "Eigene Traffic-Libraries",
                ["custom_traffic_desc"] = "Andere Traffic-Libraries mit normalen aircraft.cfg-Modelldefinitionen können ebenfalls funktionieren.",
                ["fsltl_desc"] = "Empfohlene Traffic-Modellbibliothek für gute Model-Matching-Abdeckung und Simulator-Performance.",
                ["aig_desc"] = "AI-Traffic-Paket und Modellbibliothek, häufig genutzt für Model Matching und Offline-Traffic.",
                ["fenix_desc"] = "Unterstützt für das Scannen von Fenix A320 Family Addon-Liveries.",
                ["pmdg737_desc"] = "Unterstützt für das Scannen von PMDG 737 Family Addon-Liveries.",
                ["pmdg777_desc"] = "Unterstützt für das Scannen von PMDG 777 Family Addon-Liveries.",
                ["visit_website"] = "Website öffnen",
                ["supported_note"] = "Empfehlung: Nutze nach Möglichkeit FSLTL oder AIG. Full-Fidelity-Addon-Flugzeuge wie Fenix und PMDG können deutlich mehr Simulator-Performance kosten und sollten vor allem genutzt werden, wenn die gewünschte Livery nicht bereits in einer Traffic-Library vorhanden ist.",
                ["supported_legal"] = "TrafficMatch Builder ist ein unabhängiges Community-Projekt. Es steht in keiner Verbindung zu Fenix Simulations, PMDG, VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL oder AIG und wird von diesen weder unterstützt noch gesponsert. Alle Marken und Produktnamen gehören ihren jeweiligen Eigentümern."
            },
            ["no"] = new()
            {
                ["supported_packs"] = "Støttede pakker",
                ["supported_title"] = "Støttede pakker og addons",
                ["supported_intro"] = "TrafficMatch Builder støtter dedikerte trafikkbiblioteker og utvalgte addon-liverypakker.",
                ["traffic_libraries_title"] = "Støttede trafikkbiblioteker",
                ["addon_liveries_title"] = "Støttede addon-liveries",
                ["custom_traffic_title"] = "Egne trafikkbiblioteker",
                ["custom_traffic_desc"] = "Andre trafikkbiblioteker med standard aircraft.cfg-modelldefinisjoner kan også fungere.",
                ["fsltl_desc"] = "Anbefalt trafikkmodellbibliotek for god model matching-dekning og simulator-ytelse.",
                ["aig_desc"] = "AI-trafikkpakke og modellbibliotek som ofte brukes til model matching og offline-trafikk.",
                ["fenix_desc"] = "Støttet for skanning av Fenix A320 Family addon-liveries.",
                ["pmdg737_desc"] = "Støttet for skanning av PMDG 737 Family addon-liveries.",
                ["pmdg777_desc"] = "Støttet for skanning av PMDG 777 Family addon-liveries.",
                ["visit_website"] = "Åpne nettside",
                ["supported_note"] = "Anbefaling: bruk FSLTL eller AIG når det er mulig. Full-fidelity addon-fly som Fenix og PMDG kan bruke betydelig mer simulator-ytelse og bør hovedsakelig brukes når ønsket livery ikke allerede finnes i et trafikkbibliotek.",
                ["supported_legal"] = "TrafficMatch Builder er et uavhengig community-prosjekt. Det er ikke tilknyttet, godkjent av, sponset av eller offisielt koblet til Fenix Simulations, PMDG, VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL eller AIG. Alle varemerker og produktnavn tilhører sine respektive eiere."
            },
            ["fr"] = new()
            {
                ["supported_packs"] = "Packs pris en charge",
                ["supported_title"] = "Packs et addons pris en charge",
                ["supported_intro"] = "TrafficMatch Builder prend en charge les bibliothèques de trafic dédiées et certaines livrées addon.",
                ["traffic_libraries_title"] = "Bibliothèques de trafic prises en charge",
                ["addon_liveries_title"] = "Livrées addon prises en charge",
                ["custom_traffic_title"] = "Bibliothèques de trafic personnalisées",
                ["custom_traffic_desc"] = "D’autres bibliothèques utilisant des définitions de modèles aircraft.cfg standard peuvent également fonctionner.",
                ["fsltl_desc"] = "Bibliothèque de modèles de trafic recommandée pour une bonne couverture de model matching et de bonnes performances.",
                ["aig_desc"] = "Package de trafic AI et bibliothèque de modèles souvent utilisés pour le model matching et le trafic hors ligne.",
                ["fenix_desc"] = "Pris en charge pour le scan des livrées addon Fenix A320 Family.",
                ["pmdg737_desc"] = "Pris en charge pour le scan des livrées addon PMDG 737 Family.",
                ["pmdg777_desc"] = "Pris en charge pour le scan des livrées addon PMDG 777 Family.",
                ["visit_website"] = "Ouvrir le site",
                ["supported_note"] = "Recommandation : utilisez FSLTL ou AIG lorsque c’est possible. Les avions addon full-fidelity comme Fenix et PMDG peuvent consommer beaucoup plus de ressources et devraient surtout être utilisés lorsque la livrée souhaitée n’est pas déjà disponible dans une bibliothèque de trafic.",
                ["supported_legal"] = "TrafficMatch Builder est un projet communautaire indépendant. Il n’est pas affilié, approuvé, sponsorisé ou officiellement lié à Fenix Simulations, PMDG, VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL ou AIG. Toutes les marques et tous les noms de produits appartiennent à leurs propriétaires respectifs."
            },
            ["es"] = new()
            {
                ["supported_packs"] = "Paquetes compatibles",
                ["supported_title"] = "Paquetes y addons compatibles",
                ["supported_intro"] = "TrafficMatch Builder admite bibliotecas de tráfico dedicadas y paquetes seleccionados de liveries addon.",
                ["traffic_libraries_title"] = "Bibliotecas de tráfico compatibles",
                ["addon_liveries_title"] = "Liveries addon compatibles",
                ["custom_traffic_title"] = "Bibliotecas de tráfico personalizadas",
                ["custom_traffic_desc"] = "Otras bibliotecas de tráfico con definiciones estándar en aircraft.cfg también pueden funcionar.",
                ["fsltl_desc"] = "Biblioteca de modelos de tráfico recomendada para buena cobertura de model matching y rendimiento.",
                ["aig_desc"] = "Paquete de tráfico AI y biblioteca de modelos usados habitualmente para model matching y tráfico offline.",
                ["fenix_desc"] = "Compatible con el escaneo de liveries addon de Fenix A320 Family.",
                ["pmdg737_desc"] = "Compatible con el escaneo de liveries addon de PMDG 737 Family.",
                ["pmdg777_desc"] = "Compatible con el escaneo de liveries addon de PMDG 777 Family.",
                ["visit_website"] = "Abrir sitio web",
                ["supported_note"] = "Recomendación: usa FSLTL o AIG siempre que sea posible. Los aviones addon full-fidelity como Fenix y PMDG pueden consumir bastante más rendimiento del simulador y deberían usarse principalmente cuando la livery deseada no esté ya disponible en una biblioteca de tráfico.",
                ["supported_legal"] = "TrafficMatch Builder es un proyecto comunitario independiente. No está afiliado, respaldado, patrocinado ni conectado oficialmente con Fenix Simulations, PMDG, VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL o AIG. Todas las marcas y nombres de productos pertenecen a sus respectivos propietarios."
            },
            ["it"] = new()
            {
                ["supported_packs"] = "Pacchetti supportati",
                ["supported_title"] = "Pacchetti e addon supportati",
                ["supported_intro"] = "TrafficMatch Builder supporta librerie traffico dedicate e alcuni pacchetti di livery addon.",
                ["traffic_libraries_title"] = "Librerie traffico supportate",
                ["addon_liveries_title"] = "Livery addon supportate",
                ["custom_traffic_title"] = "Librerie traffico personalizzate",
                ["custom_traffic_desc"] = "Anche altre librerie traffico con definizioni modello standard in aircraft.cfg possono funzionare.",
                ["fsltl_desc"] = "Libreria di modelli traffico consigliata per buona copertura model matching e prestazioni.",
                ["aig_desc"] = "Pacchetto AI traffic e libreria modelli spesso usati per model matching e traffico offline.",
                ["fenix_desc"] = "Supportato per la scansione di livery addon Fenix A320 Family.",
                ["pmdg737_desc"] = "Supportato per la scansione di livery addon PMDG 737 Family.",
                ["pmdg777_desc"] = "Supportato per la scansione di livery addon PMDG 777 Family.",
                ["visit_website"] = "Apri sito web",
                ["supported_note"] = "Consiglio: usa FSLTL o AIG quando possibile. Gli aerei addon full-fidelity come Fenix e PMDG possono consumare molte più risorse del simulatore e dovrebbero essere usati soprattutto quando la livery desiderata non è già disponibile in una libreria traffico.",
                ["supported_legal"] = "TrafficMatch Builder è un progetto indipendente della community. Non è affiliato, approvato, sponsorizzato o collegato ufficialmente a Fenix Simulations, PMDG, VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL o AIG. Tutti i marchi e i nomi dei prodotti appartengono ai rispettivi proprietari."
            }
        };

        if (text.TryGetValue(lang, out var localized) && localized.TryGetValue(key, out var value))
            return value;
        return text["en"].TryGetValue(key, out var fallback) ? fallback : "";
    }

    private string GetHelpText()
    {
        var nl = Environment.NewLine;

        return _language.ToLowerInvariant() switch
        {
            "de" => string.Join(nl,
                "TrafficMatch Builder erstellt VMR-Dateien für vPilot und VATSIM.",
                "",
                "Grundidee:",
                "Das Tool kann zwei unterschiedliche Dinge scannen. Wähle bewusst den passenden Weg:",
                "",
                "A) Traffic Library scannen",
                "Für FSLTL, AIG, FAIB, TFS oder andere AI-Traffic-Pakete.",
                "Das ist der empfohlene Standardweg, weil diese Modelle für Online-Traffic gebaut sind und deutlich weniger Performance kosten.",
                "",
                "So gehst du vor:",
                "1. Klicke auf „Community“ oder „Ordner wählen“.",
                "2. Wähle deinen MSFS Community-Ordner oder ein konkretes Traffic-Paket.",
                "3. Klicke auf „Modelle scannen“.",
                "4. Prüfe den Scanbericht.",
                "5. Exportiere die VMR-Datei und füge sie in vPilot unter Model Matching hinzu.",
                "",
                "B) PMDG/Fenix-Livery scannen",
                "Für installierte Addon-Liveries von Fenix A320 oder PMDG 737/777.",
                "Das Tool sucht automatisch den passenden Basisflugzeug-Ordner und löst den echten ModelName über aircraft.cfg, required_tags, Paketname und TypeCode auf.",
                "",
                "Wichtig:",
                "- Nutze PMDG/Fenix nur, wenn diese Airline oder Livery nicht bereits sinnvoll im Traffic-Pack vorhanden ist.",
                "- Fenix und PMDG sind detaillierte Flugzeuge und können als Online-Traffic deutlich mehr FPS kosten als AI-Traffic-Modelle.",
                "- Unterstützt sind aktuell Fenix A320 sowie PMDG 737 und PMDG 777.",
                "- Andere Addon-Flugzeuge können zufällig funktionieren, sind aber nicht offiziell unterstützt, weil sie nicht getestet werden können.",
                "",
                "Empfohlen:",
                "- FSLTL Traffic-Basisordner",
                "- AIG Traffic-Paketordner",
                "- eigener AI-Traffic-Modellordner",
                "- einzelne Fenix- oder PMDG-Livery",
                "- Fenix-/PMDG-Livery-Sammelordner",
                "",
                "Nicht empfohlen:",
                "- komplette Scenery-, Airport-, Effekt-, Sound- oder Toolbar-Ordner",
                "- kompletter MSFS-Packages-Ordner, wenn du eigentlich nur ein bestimmtes Paket testen willst",
                
                "",
                "Hinweis:",
                "Nur bestätigte ModelNames werden exportiert. Low-Confidence-Regeln bleiben draußen, damit keine kaputten VMR-Dateien entstehen.",
                "Das Programm verändert keine Simulator-Dateien und arbeitet lokal auf deinem PC."),

            "no" => string.Join(nl,
                "TrafficMatch Builder lager VMR-filer for vPilot og VATSIM.",
                "",
                "Grunnidé:",
                "Verktøyet kan skanne to forskjellige ting. Velg riktig arbeidsmåte:",
                "",
                "A) Skanne trafikkbibliotek",
                "For FSLTL, AIG, FAIB, TFS eller andre AI-trafikkpakker.",
                "Dette er standardvalget, fordi disse modellene er laget for online-trafikk og bruker langt mindre ytelse.",
                "",
                "Slik gjør du:",
                "1. Klikk på «Velg mappe».",
                "2. Velg MSFS Community-mappen eller en konkret trafikkpakke.",
                "3. Klikk på «Skann modeller».",
                "4. Kontroller skannrapporten.",
                "5. Eksporter VMR-filen og legg den til i vPilot under Model Matching.",
                "",
                "B) Skanne PMDG/Fenix-livery",
                "For installerte addon-liveries til Fenix A320 eller PMDG 737/777.",
                "Verktøyet søker automatisk etter riktig basisflymappe og løser ekte ModelName via aircraft.cfg, required_tags, pakkenavn og TypeCode.",
                "",
                "Viktig:",
                "- Bruk PMDG/Fenix bare hvis flyselskapet eller liveryen ikke allerede finnes fornuftig i trafikkpakken.",
                "- Fenix og PMDG er detaljerte fly og kan koste mye mer FPS som online-trafikk enn AI-trafikkmodeller.",
                "- Støttet nå: Fenix A320, PMDG 737 og PMDG 777.",
                "- Andre addon-fly kan fungere, men er ikke offisielt støttet fordi de ikke kan testes.",
                "",
                "Anbefalt:",
                "- FSLTL trafikk-basemappe",
                "- AIG trafikkpakkemappe",
                "- egen AI-trafikkmodellmappe",
                "- enkelt Fenix- eller PMDG-livery",
                "- Fenix-/PMDG-liverysamling",
                "",
                "Ikke anbefalt:",
                "- store scenery-, airport-, effekt-, lyd- eller toolbar-mapper",
                "- hele MSFS Packages-mappen hvis du bare vil teste én spesifikk pakke",
                "- scenery-, airport-, effekt-, lyd- eller toolbar-addons",
                "",
                "Merk:",
                "Bare bekreftede ModelNames eksporteres. Low-confidence-regler holdes ute for å unngå ødelagte VMR-filer.",
                "Programmet endrer ingen simulatorfiler og fungerer lokalt på PC-en din."),

            "fr" => string.Join(nl,
                "TrafficMatch Builder crée des fichiers VMR pour vPilot et VATSIM.",
                "",
                "Principe :",
                "L’outil peut scanner deux types de dossiers. Choisissez le bon mode de travail :",
                "",
                "A) Scanner une bibliothèque de trafic",
                "Pour FSLTL, AIG, FAIB, TFS ou d’autres paquets AI Traffic.",
                "C’est le mode recommandé, car ces modèles sont faits pour le trafic en ligne et coûtent beaucoup moins en performances.",
                "",
                "Étapes :",
                "1. Cliquez sur « Choisir dossier ».",
                "2. Sélectionnez le dossier Community MSFS ou un package de trafic précis.",
                "3. Cliquez sur « Scanner modèles ».",
                "4. Vérifiez le rapport de scan.",
                "5. Exportez le fichier VMR et ajoutez-le dans vPilot sous Model Matching.",
                "",
                "B) Scanner une livrée PMDG/Fenix",
                "Pour les livrées addon installées du Fenix A320 ou des PMDG 737/777.",
                "L’outil cherche automatiquement le dossier de l’avion de base et résout le vrai ModelName via aircraft.cfg, required_tags, nom de paquet et TypeCode.",
                "",
                "Important :",
                "- Utilisez PMDG/Fenix seulement si la compagnie ou la livrée n’existe pas déjà correctement dans un pack de trafic.",
                "- Fenix et PMDG sont des avions détaillés et peuvent coûter beaucoup plus de FPS comme trafic en ligne que des modèles AI.",
                "- Actuellement pris en charge : Fenix A320, PMDG 737 et PMDG 777.",
                "- D’autres avions addon peuvent fonctionner, mais ne sont pas officiellement pris en charge car ils ne peuvent pas être testés.",
                "",
                "Recommandé :",
                "- dossier de base FSLTL Traffic",
                "- dossier de paquet AIG Traffic",
                "- dossier personnalisé de modèles AI Traffic",
                "- livrée individuelle Fenix ou PMDG",
                "- dossier de livrées Fenix/PMDG",
                "",
                "Non recommandé :",
                "- grands dossiers scenery, airport, effets, sons ou toolbar",
                "- dossier MSFS Packages complet si vous voulez seulement tester un paquet précis",
                "- addons scenery, airport, effets, sons ou toolbar",
                "",
                "Note :",
                "Seuls les ModelNames confirmés sont exportés. Les règles à faible confiance restent exclues afin d’éviter des fichiers VMR cassés.",
                "Le programme ne modifie aucun fichier du simulateur et fonctionne localement."),

            "es" => string.Join(nl,
                "TrafficMatch Builder crea archivos VMR para vPilot y VATSIM.",
                "",
                "Idea básica:",
                "La herramienta puede escanear dos cosas distintas. Elige el flujo correcto:",
                "",
                "A) Escanear una biblioteca de tráfico",
                "Para FSLTL, AIG, FAIB, TFS u otros paquetes AI Traffic.",
                "Es el método recomendado, porque estos modelos están hechos para tráfico online y consumen mucho menos rendimiento.",
                "",
                "Pasos:",
                "1. Haz clic en «Elegir carpeta».",
                "2. Selecciona la carpeta Community de MSFS o un paquete de tráfico concreto.",
                "3. Haz clic en «Escanear modelos».",
                "4. Revisa el informe de escaneo.",
                "5. Exporta el archivo VMR y añádelo en vPilot en Model Matching.",
                "",
                "B) Escanear una livery PMDG/Fenix",
                "Para liveries addon instaladas del Fenix A320 o PMDG 737/777.",
                "La herramienta busca automáticamente la carpeta del avión base y resuelve el ModelName real mediante aircraft.cfg, required_tags, nombre del paquete y TypeCode.",
                "",
                "Importante:",
                "- Usa PMDG/Fenix solo si esa aerolínea o livery no existe ya de forma útil en el paquete de tráfico.",
                "- Fenix y PMDG son aviones detallados y pueden consumir muchos más FPS como tráfico online que los modelos AI.",
                "- Actualmente soportado: Fenix A320, PMDG 737 y PMDG 777.",
                "- Otros aviones addon pueden funcionar, pero no están soportados oficialmente porque no se pueden probar.",
                "",
                "Recomendado:",
                "- carpeta base de FSLTL Traffic",
                "- carpeta de paquete AIG Traffic",
                "- carpeta personalizada de modelos AI Traffic",
                "- livery individual de Fenix o PMDG",
                "- carpeta de liveries Fenix/PMDG",
                "",
                "No recomendado:",
                "- carpetas grandes de scenery, airports, efectos, sonido o toolbar",
                "- toda la carpeta MSFS Packages si solo quieres probar un paquete específico",
                "- addons de scenery, airports, efectos, sonido o toolbar",
                "",
                "Nota:",
                "Solo se exportan ModelNames confirmados. Las reglas de baja confianza quedan fuera para evitar archivos VMR rotos.",
                "El programa no modifica archivos del simulador y funciona localmente."),

            "it" => string.Join(nl,
                "TrafficMatch Builder crea file VMR per vPilot e VATSIM.",
                "",
                "Idea di base:",
                "Lo strumento può scansionare due cose diverse. Scegli il flusso corretto:",
                "",
                "A) Scansionare una libreria traffico",
                "Per FSLTL, AIG, FAIB, TFS o altri pacchetti AI Traffic.",
                "È il metodo consigliato, perché questi modelli sono creati per il traffico online e consumano molte meno prestazioni.",
                "",
                "Passaggi:",
                "1. Clicca su «Seleziona cartella».",
                "2. Seleziona la cartella Community MSFS o un pacchetto traffico specifico.",
                "3. Clicca su «Scansiona modelli».",
                "4. Controlla il report della scansione.",
                "5. Esporta il file VMR e aggiungilo in vPilot sotto Model Matching.",
                "",
                "B) Scansionare una livery PMDG/Fenix",
                "Per livery addon installate del Fenix A320 o dei PMDG 737/777.",
                "Lo strumento cerca automaticamente la cartella dell’aereo base e risolve il vero ModelName tramite aircraft.cfg, required_tags, nome pacchetto e TypeCode.",
                "",
                "Importante:",
                "- Usa PMDG/Fenix solo se quella compagnia o livery non è già disponibile in modo utile nel pacchetto traffico.",
                "- Fenix e PMDG sono aerei dettagliati e possono consumare molti più FPS come traffico online rispetto ai modelli AI.",
                "- Supportati ora: Fenix A320, PMDG 737 e PMDG 777.",
                "- Altri aerei addon possono funzionare, ma non sono ufficialmente supportati perché non possono essere testati.",
                "",
                "Consigliato:",
                "- cartella base FSLTL Traffic",
                "- cartella pacchetto AIG Traffic",
                "- cartella personalizzata di modelli AI Traffic",
                "- singola livery Fenix o PMDG",
                "- cartella livery Fenix/PMDG",
                "",
                "Non consigliato:",
                "- grandi cartelle scenery, airport, effetti, audio o toolbar",
                "- intera cartella MSFS Packages se vuoi testare solo un pacchetto specifico",
                "- addon scenery, airport, effetti, audio o toolbar",
                "",
                "Nota:",
                "Vengono esportati solo ModelNames confermati. Le regole a bassa affidabilità restano escluse per evitare file VMR non validi.",
                "Il programma non modifica file del simulatore e funziona localmente."),

            _ => string.Join(nl,
                "TrafficMatch Builder creates VMR files for vPilot and VATSIM.",
                "",
                "Basic idea:",
                "The tool can scan two different things. Choose the workflow that fits your goal:",
                "",
                "A) Scan a traffic library",
                "For FSLTL, AIG, FAIB, TFS or other AI traffic packages.",
                "This is the recommended default because these models are built for online traffic and use far less performance.",
                "",
                "How to use it:",
                "1. Click “Community” or “Select Folder”.",
                "2. Select your MSFS Community folder or a specific traffic package.",
                "3. Click “Scan Models”.",
                "4. Review the scan report.",
                "5. Export the VMR file and add it to vPilot under Model Matching.",
                "",
                "B) Scan a PMDG/Fenix livery",
                "For installed addon liveries of the Fenix A320 or PMDG 737/777.",
                "The tool automatically looks for the matching base aircraft folder and resolves the real ModelName via aircraft.cfg, required_tags, package name and TypeCode.",
                "",
                "Important:",
                "- Use PMDG/Fenix only if that airline or livery is not already available in a useful traffic pack.",
                "- Fenix and PMDG are detailed aircraft and can cost much more FPS as online traffic than AI traffic models.",
                "- Currently supported: Fenix A320, PMDG 737 and PMDG 777.",
                "- Other addon aircraft may work, but are not officially supported because they cannot be tested.",
                "",
                "Recommended:",
                "- FSLTL traffic base folder",
                "- AIG traffic package folder",
                "- Custom AI traffic model folder",
                "- Single Fenix or PMDG livery",
                "- Fenix/PMDG livery collection folder",
                "",
                "Not recommended:",
                "- Large scenery, airport, effects, sound or toolbar addon folders",
                "- The entire MSFS Packages folder if you only want to test one specific package",
                "- Scenery, airport, effects, sound or toolbar addons",
                "",
                "Note:",
                "Only confirmed ModelNames are exported. Low-confidence rules stay excluded so broken VMR files are avoided.",
                "The application does not modify simulator files and works locally on your computer.")
        };
    }

    private PictureBox CreateImageBox(string fileName, Size size)
    {
        var box = new PictureBox
        {
            Size = size,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
            if (File.Exists(path))
                box.Image = Image.FromFile(path);
        }
        catch
        {
            // Optional branding image could not be loaded.
        }

        return box;
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
            Location = new Point(405, 235)
        });

        root.Controls.Add(new Label
        {
            Text = L("language_hint"),
            Font = new Font("Segoe UI", 10),
            ForeColor = TextSoft,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(405, 277)
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

        var report = CreateHeaderButton(L("report"), 770, 18);
        report.Click += (_, _) => ShowReportMenu();
        wrapper.Controls.Add(report);

        var supported = CreateHeaderButton(L("supported_packs"), 870, 18);
        supported.Size = new Size(145, 34);
        supported.Click += (_, _) => ShowSupportedPacksWindow();
        wrapper.Controls.Add(supported);

        var credits = CreateHeaderButton(L("credits"), 1020, 18);
        credits.Click += (_, _) => ShowAboutWindow();
        wrapper.Controls.Add(credits);

        var help = CreateHeaderButton(L("help"), 1120, 18);
        help.Click += (_, _) => ShowHelpWindow();
        wrapper.Controls.Add(help);

        var mainLogo = CreateImageBox("logo.png", new Size(64, 64));
        mainLogo.Location = new Point(24, 68);
        wrapper.Controls.Add(mainLogo);

        wrapper.Controls.Add(new Label
        {
            Text = "TrafficMatch Builder",
            Font = new Font("Segoe UI", 29, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(100, 72)
        });

        wrapper.Controls.Add(new Label
        {
            Text = $"{L("subtitle")}  •  {AppVersion}",
            Font = new Font("Segoe UI", 10),
            ForeColor = TextSoft,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(104, 126)
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

        var selectPanel = CreatePanelBox(24, 190, 1165, 300);
        wrapper.Controls.Add(selectPanel);

        selectPanel.Controls.Add(new Label
        {
            Text = L("community_title"),
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 16)
        });

        selectPanel.Controls.Add(new Label
        {
            Text = L("workflow_hint"),
            Font = new Font("Segoe UI", 9),
            ForeColor = TextSoft,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 45)
        });

        _communityLabel = new Label
        {
            Text = $"{L("selected_community")}\n-",
            Font = new Font("Segoe UI", 9),
            ForeColor = TextMuted,
            BackColor = PanelDark,
            AutoSize = false,
            Size = new Size(720, 38),
            Location = new Point(24, 73)
        };
        selectPanel.Controls.Add(_communityLabel);

        var communityButton = CreateModernButton(L("auto_community"), Color.FromArgb(31, 41, 55), new Size(175, 40));
        communityButton.Location = new Point(760, 72);
        communityButton.Click += (_, _) => SelectDetectedCommunityFolder();
        selectPanel.Controls.Add(communityButton);

        var selectCommunityButton = CreateModernButton(L("select_community"), Color.FromArgb(31, 41, 55), new Size(185, 40));
        selectCommunityButton.Location = new Point(955, 72);
        selectCommunityButton.Click += (_, _) => SelectCommunityFolder();
        selectPanel.Controls.Add(selectCommunityButton);

        selectPanel.Controls.Add(new Label
        {
            Text = L("source_title"),
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 128)
        });

        selectPanel.Controls.Add(new Label
        {
            Text = L("source_hint"),
            Font = new Font("Segoe UI", 9),
            ForeColor = TextSoft,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 157)
        });

        _folderLabel = new Label
        {
            Text = $"{L("selected_folder")}\n-",
            Font = new Font("Segoe UI", 9),
            ForeColor = TextMuted,
            BackColor = PanelDark,
            AutoSize = false,
            Size = new Size(720, 38),
            Location = new Point(24, 185)
        };
        selectPanel.Controls.Add(_folderLabel);

        _detectedLabel = new Label
        {
            Text = $"{L("detected")} -",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = BlueLight,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 228)
        };
        selectPanel.Controls.Add(_detectedLabel);

        var selectButton = CreateModernButton(L("select_folder"), Color.FromArgb(31, 41, 55), new Size(205, 40));
        selectButton.Location = new Point(760, 184);
        selectButton.Click += (_, _) => SelectFolder();
        selectPanel.Controls.Add(selectButton);

        _scanButton = CreateModernButton(L("scan_models"), Disabled, new Size(165, 40));
        _scanButton.Location = new Point(980, 184);
        _scanButton.Click += (_, _) => ScanFolder();
        selectPanel.Controls.Add(_scanButton);

        _exportButton = CreateModernButton(L("export_vmr"), Disabled, new Size(165, 40));
        _exportButton.Location = new Point(980, 232);
        _exportButton.Click += (_, _) => ExportVmr();
        selectPanel.Controls.Add(_exportButton);

        SetActionButton(_scanButton, false, Blue);
        SetActionButton(_exportButton, false, Green);

        selectPanel.Controls.Add(new Label
        {
            Text = L("help_hint"),
            Font = new Font("Segoe UI", 8),
            ForeColor = TextSoft,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(24, 274)
        });

        CreateStats(wrapper, 24, 515);

        var statusPanel = CreatePanelBox(24, 615, 1165, 70);
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
            Size = new Size(1125, 12),
            Style = ProgressBarStyle.Continuous
        };
        statusPanel.Controls.Add(_progress);

        var reportPanel = CreatePanelBox(24, 700, 1165, 145);
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
            Size = new Size(1125, 84)
        };
        reportPanel.Controls.Add(_reportBox);
    }

    private void CreateStats(Control parent, int x, int y)
    {
        var labels = new[] { L("stat_cfg"), L("stat_models"), L("stat_type"), L("stat_rules") };

        for (var i = 0; i < 4; i++)
        {
            var panel = CreateStatCard(x + i * 290, y, 270, 82);
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

    private void SelectDetectedCommunityFolder()
    {
        var community = GetPreferredCommunityFolder();
        if (string.IsNullOrWhiteSpace(community) || !Directory.Exists(community))
        {
            MessageBox.Show(L("community_not_found"), "Community", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SelectCommunityFolder();
            return;
        }

        ApplyCommunityFolder(community);
    }

    private string GetPreferredCommunityFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var candidates = new[]
        {
            Path.Combine(appData, "Microsoft Flight Simulator 2024", "Packages", "Community"),
            Path.Combine(localAppData, "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache", "Packages", "Community"),
            Path.Combine(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache", "Packages", "Community"),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        return candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private void ApplyCommunityFolder(string folder)
    {
        _communityFolder = folder;
        _selectedFolder = "";
        _baseAircraftFolder = "";
        _scanResult = null;

        _communityLabel!.Text = $"{L("selected_community")}\n{_communityFolder}";
        _folderLabel!.Text = $"{L("selected_folder")}\n-";
        _detectedLabel!.Text = $"{L("detected")} -";

        SetActionButton(_scanButton, false, Blue);
        SetActionButton(_exportButton, false, Green);

        _statusLabel!.Text = L("folder_selected");
        _reportBox!.Text = L("folder_report");

        SetStats(0, 0, 0, 0);

        if (_progress is not null)
            _progress.Value = 0;
    }

    private void ApplySelectedFolder(string folder)
    {
        _selectedFolder = folder;
        _baseAircraftFolder = "";
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

    private void SelectCommunityFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = L("community_title"),
            SelectedPath = Directory.Exists(_communityFolder) ? _communityFolder : GetPreferredCommunityFolder()
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        ApplyCommunityFolder(dialog.SelectedPath);
    }

    private void SelectFolder()
    {
        if (string.IsNullOrWhiteSpace(_communityFolder) || !Directory.Exists(_communityFolder))
            SelectDetectedCommunityFolder();

        using var dialog = new FolderBrowserDialog
        {
            Description = L("select_title"),
            SelectedPath = Directory.Exists(_communityFolder) ? _communityFolder : GetPreferredCommunityFolder()
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        ApplySelectedFolder(dialog.SelectedPath);
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
            _scanResult = scanner.Scan(_selectedFolder, string.IsNullOrWhiteSpace(_baseAircraftFolder) ? null : _baseAircraftFolder);

            if (_scanResult.NeedsBaseAircraftFolder && AskForBaseAircraftFolder())
            {
                _statusLabel.Text = L("scanning_base_models");
                _progress.Value = 55;
                Application.DoEvents();
                _scanResult = scanner.Scan(_selectedFolder, _baseAircraftFolder);
            }

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

    private bool AskForBaseAircraftFolder()
    {
        var answer = MessageBox.Show(
            L("base_models_missing_text"),
            L("base_models_missing_title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (answer != DialogResult.Yes)
            return false;

        using var dialog = new FolderBrowserDialog
        {
            Description = L("select_base_aircraft_folder"),
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return false;

        _baseAircraftFolder = dialog.SelectedPath;
        return true;
    }

    private void BuildReport()
    {
        if (_scanResult is null)
            return;

        var lines = new List<string>
        {
            L("scan_complete_short"),
            "",
            $"=== {L("scan_summary").ToUpperInvariant()} ===",
            $"{L("found_cfg")} {_scanResult.AircraftCfgFiles}",
            $"{L("found_liveries")} {_scanResult.AddonLiveryFolders}",
            $"{L("found_models")} {_scanResult.ModelsFound}",
            $"{L("usable_rules")} {_scanResult.UsableRules}",
            $"{L("low_confidence")} {_scanResult.LowConfidenceRules}",
            "",
            $"=== {L("paths_and_discovery").ToUpperInvariant()} ===",
            $"{L("root")} {_selectedFolder}",
            string.IsNullOrWhiteSpace(_baseAircraftFolder) ? $"{L("base_aircraft_folder")} -" : $"{L("base_aircraft_folder")} {_baseAircraftFolder}",
            $"{L("auto_base_folders")} {_scanResult.AutoDiscoveredBaseFolders.Count}",
            $"{L("auto_livery_folders")} {_scanResult.AutoDiscoveredLiveryFolders.Count}",
            "",
            L("scan_tip"),
            "",
            L("addon_scan_note"),
            ""
        };

        lines.AddRange(BuildSourceOverviewLines());
        if (_scanResult.AutoDiscoveredBaseFolders.Count > 0 || _scanResult.AutoDiscoveredLiveryFolders.Count > 0)
        {
            lines.Add("");
            lines.Add(L("auto_discovery_note"));
            foreach (var folder in _scanResult.AutoDiscoveredBaseFolders.Take(8))
                lines.Add($"  {L("auto_base_folder")}: {folder}");
            foreach (var folder in _scanResult.AutoDiscoveredLiveryFolders.Take(8))
                lines.Add($"  {L("auto_livery_folder")}: {folder}");
        }
        lines.Add("");

        if (_scanResult.NeedsBaseAircraftFolder)
        {
            lines.Add(L("base_models_still_missing"));
            lines.Add("");
        }

        lines.Add($"=== {L("vmr_rules").ToUpperInvariant()} ===");

        foreach (var entry in _scanResult.Entries.Where(entry => entry.HasMinimumData))
        {
            var code = string.IsNullOrWhiteSpace(entry.ExportCode) ? "-" : entry.ExportCode;
            var display = string.IsNullOrWhiteSpace(entry.DisplayName) || entry.DisplayName == entry.Title ? "" : $" | {entry.DisplayName}";
            lines.Add($"- {code} / {entry.TypeCode} / {entry.Title}{display} [{L("confidence")}: {entry.Confidence}, {L("source")}: {entry.ModelNameSource}]");

            if (string.Equals(entry.Confidence, "Low", StringComparison.OrdinalIgnoreCase))
                lines.Add($"  {L("warning")}: {L("modelname_warning")}");
            else if (!string.IsNullOrWhiteSpace(entry.Warning))
                lines.Add($"  {L("warning")}: {entry.Warning}");
        }

        _reportBox!.Text = string.Join(Environment.NewLine, lines);
    }

    private IEnumerable<string> BuildSourceOverviewLines()
    {
        if (_scanResult is null)
            yield break;

        yield return L("source_overview");

        var sourceGroups = _scanResult.Entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.AddonFamily))
            .GroupBy(entry => entry.AddonFamily)
            .OrderBy(group => group.Key)
            .ToList();

        if (sourceGroups.Count == 0)
        {
            yield return $"  - {L("source_none")}";
            yield break;
        }

        foreach (var group in sourceGroups)
        {
            var exportable = group.Count(entry => entry.IsExportable);
            var low = group.Count(entry => string.Equals(entry.Confidence, "Low", StringComparison.OrdinalIgnoreCase));
            yield return $"  - {group.Key}: {group.Count()} {L("source_models")} / {exportable} {L("source_exportable")} / {low} {L("source_low_confidence")}";
        }
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
        if (string.IsNullOrWhiteSpace(_selectedFolder) || string.IsNullOrWhiteSpace(_communityFolder))
            return true;

        var selected = Path.GetFullPath(_selectedFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var community = Path.GetFullPath(_communityFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!string.Equals(selected, community, StringComparison.OrdinalIgnoreCase))
            return true;

        var result = MessageBox.Show(
            L("community_warning_text"),
            L("community_warning_title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }


    private void ShowSupportedPacksWindow()
    {
        using var window = new Form
        {
            Text = L("supported_title"),
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(980, 760),
            MinimumSize = new Size(900, 700),
            BackColor = Bg,
            ForeColor = TextMain,
            Font = new Font("Segoe UI", 9F),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var title = new Label
        {
            Text = L("supported_title"),
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 64
        };

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Bg,
            Padding = new Padding(24),
            AutoScroll = true
        };

        body.Controls.Add(new Label
        {
            Text = L("supported_intro"),
            Font = new Font("Segoe UI", 10),
            ForeColor = TextMuted,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(890, 32),
            Location = new Point(24, 16)
        });

        var left = CreatePanelBox(24, 62, 440, 330);
        var right = CreatePanelBox(484, 62, 440, 330);
        body.Controls.Add(left);
        body.Controls.Add(right);

        left.Controls.Add(new Label
        {
            Text = L("traffic_libraries_title"),
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(18, 16)
        });

        right.Controls.Add(new Label
        {
            Text = L("addon_liveries_title"),
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(18, 16)
        });

        AddSupportedEntry(left, 18, 58, "FSLTL", L("fsltl_desc"), "https://fslivetrafficliveries.com/");
        AddSupportedEntry(left, 18, 145, "AIG", L("aig_desc"), "https://www.alpha-india.net/");
        AddSupportedEntry(left, 18, 232, L("custom_traffic_title"), L("custom_traffic_desc"), "");

        AddSupportedEntry(right, 18, 58, "Fenix A320 Family", L("fenix_desc"), "https://fenixsim.com/");
        AddSupportedEntry(right, 18, 145, "PMDG 737 Family", L("pmdg737_desc"), "https://pmdg.com/");
        AddSupportedEntry(right, 18, 232, "PMDG 777 Family", L("pmdg777_desc"), "https://pmdg.com/");

        var note = CreatePanelBox(24, 410, 900, 88);
        body.Controls.Add(note);
        note.Controls.Add(new Label
        {
            Text = L("supported_note"),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = TextMuted,
            BackColor = PanelDark,
            AutoSize = false,
            Size = new Size(860, 58),
            Location = new Point(18, 16)
        });

        var legal = new Label
        {
            Text = L("supported_legal"),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextSoft,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(900, 70),
            Location = new Point(24, 516)
        };
        body.Controls.Add(legal);

        var close = CreateModernButton(L("close"), Blue, new Size(120, 42));
        close.Dock = DockStyle.Bottom;
        close.Click += (_, _) => window.Close();

        window.Controls.Add(body);
        window.Controls.Add(close);
        window.Controls.Add(title);
        window.ShowDialog(this);
    }

    private void AddSupportedEntry(Control parent, int x, int y, string title, string description, string url)
    {
        parent.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = PanelDark,
            AutoSize = true,
            Location = new Point(x, y)
        });

        parent.Controls.Add(new Label
        {
            Text = description,
            Font = new Font("Segoe UI", 9),
            ForeColor = TextMuted,
            BackColor = PanelDark,
            AutoSize = false,
            Size = new Size(300, 42),
            Location = new Point(x, y + 25)
        });

        if (!string.IsNullOrWhiteSpace(url))
        {
            var link = new LinkLabel
            {
                Text = L("visit_website"),
                LinkColor = BlueLight,
                ActiveLinkColor = Color.White,
                VisitedLinkColor = BlueLight,
                BackColor = PanelDark,
                AutoSize = true,
                Location = new Point(x + 315, y + 25),
                Cursor = Cursors.Hand
            };
            link.Click += (_, _) => OpenExternalLink(url);
            parent.Controls.Add(link);
        }
    }

    private void ShowAboutWindow()
    {
        _aboutLogoClickCount = 0;

        using var about = new Form
        {
            Text = $"TrafficMatch Builder {AppVersion}",
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(820, 820),
            MinimumSize = new Size(800, 760),
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

        var logoPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 118,
            BackColor = PanelDark,
            Cursor = Cursors.Hand
        };

        var logoImage = CreateImageBox("logo.png", new Size(96, 96));
        logoImage.Location = new Point(342, 10);
        logoPanel.Controls.Add(logoImage);

        void LogoClick(object? sender, EventArgs e)
        {
            _aboutLogoClickCount++;

            if (_aboutLogoClickCount == 10)
            {
                MessageBox.Show(
                    "Achievement Unlocked\n\n" +
                    "Certified Traffic Matching Engineer\n\n" +
                    "You survived:\n\n" +
                    "• XML files\n" +
                    "• model matching\n" +
                    "• VMR generation\n" +
                    "• VirusTotal false positives\n\n" +
                    "Welcome to the maintenance department.\n\n" +
                    "Coffee is mandatory.",
                    "Achievement Unlocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        logoPanel.Click += LogoClick;
        logoImage.Click += LogoClick;

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Bg,
            Padding = new Padding(28, 22, 28, 18),
            AutoScroll = true
        };

        body.Controls.Add(new Label
        {
            Text = L("about_subtitle"),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            ForeColor = TextMuted,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(700, 26),
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
            Size = new Size(720, 230),
            Location = new Point(28, 246)
        });

        body.Controls.Add(new Label
        {
            Text = $"{L("about_disclaimer")}\n© 2026 TrafficMatch Builder",
            Font = new Font("Segoe UI", 9),
            ForeColor = TextSoft,
            BackColor = Bg,
            AutoSize = false,
            Size = new Size(720, 150),
            Location = new Point(28, 505)
        });

        var close = CreateModernButton(L("close"), Blue, new Size(120, 42));
        close.Dock = DockStyle.Bottom;
        close.Click += (_, _) => about.Close();

        about.Controls.Add(body);
        about.Controls.Add(close);
        about.Controls.Add(logoPanel);
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





    private void ShowHelpWindow()
    {
        using var helpWindow = new Form
        {
            Text = L("help_title"),
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(820, 720),
            MinimumSize = new Size(760, 640),
            BackColor = Bg,
            ForeColor = TextMain,
            Font = new Font("Segoe UI", 9F),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var title = new Label
        {
            Text = L("help_title"),
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = TextMain,
            BackColor = Bg,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 58
        };

        var textBox = new TextBox
        {
            Text = GetHelpText(),
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            BackColor = PanelDark,
            ForeColor = TextMuted,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10F),
            TabStop = false,
            HideSelection = true
        };

        var close = CreateModernButton(L("close"), Blue, new Size(120, 42));
        close.Dock = DockStyle.Bottom;
        close.Click += (_, _) => helpWindow.Close();

        helpWindow.Controls.Add(textBox);
        helpWindow.Controls.Add(close);
        helpWindow.Controls.Add(title);

        helpWindow.Shown += (_, _) =>
        {
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;
            close.Focus();
            helpWindow.ActiveControl = close;
        };

        helpWindow.ShowDialog(this);
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

        if (lowerPath.Contains("pmdg-aircraft-738") || lowerPath.Contains("b738") || lowerPath.Contains("737"))
            return "PMDG_737";

        if (lowerPath.Contains("pmdg-aircraft-77w") || lowerPath.Contains("b77w") || lowerPath.Contains("777"))
            return "PMDG_777";

        if (lowerPath.Contains("fenix") || lowerPath.Contains("fnx") || lowerPath.Contains("a320"))
            return "Fenix_A320";

        return "AutoDetect";
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
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        var rect = ClientRectangle;
        var bannerPath = Path.Combine(AppContext.BaseDirectory, "Assets", "banner.png");

        if (File.Exists(bannerPath))
        {
            using var img = Image.FromFile(bannerPath);
            DrawImageCover(e.Graphics, img, rect);
        }
        else
        {
            using var bgBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(18, 32, 55),
                Color.FromArgb(7, 15, 28),
                LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(bgBrush, rect);
        }

        using var overlay = new LinearGradientBrush(
            rect,
            Color.FromArgb(210, 5, 12, 24),
            Color.FromArgb(95, 5, 12, 24),
            LinearGradientMode.Horizontal);
        e.Graphics.FillRectangle(overlay, rect);

        using var borderPen = new Pen(Color.FromArgb(56, 189, 248), 1);
        e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
        if (File.Exists(logoPath))
        {
            using var logo = Image.FromFile(logoPath);
            e.Graphics.DrawImage(logo, new Rectangle(34, 26, 118, 118));
        }

        using var titleFont = new Font("Segoe UI", 34, FontStyle.Bold);
        using var subtitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
        using var badgeFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var whiteBrush = new SolidBrush(Color.White);
        using var blueBrush = new SolidBrush(Color.FromArgb(56, 189, 248));
        using var mutedBrush = new SolidBrush(Color.FromArgb(215, 230, 245));

        e.Graphics.DrawString("TrafficMatch", titleFont, whiteBrush, 180, 38);
        e.Graphics.DrawString("Builder", titleFont, blueBrush, 500, 38);
        e.Graphics.DrawString("VMR Generator for MSFS, vPilot & VATSIM  •  v0.6 Beta", subtitleFont, mutedBrush, 185, 100);
    }

    private static void DrawImageCover(Graphics graphics, Image image, Rectangle target)
    {
        var scale = Math.Max((float)target.Width / image.Width, (float)target.Height / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        var x = target.X + (target.Width - width) / 2f;
        var y = target.Y + (target.Height - height) / 2f;
        graphics.DrawImage(image, x, y, width, height);
    }
 }
