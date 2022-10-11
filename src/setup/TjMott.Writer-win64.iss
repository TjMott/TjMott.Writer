[Setup]
AppId="TjMott.Writer"
AppName="TJ Mott's Writer"
AppVersion=0.5.0
DefaultDirName="{commonpf}\TJ Mott\TJ Mott's Writer"
VersionInfoCompany="TJ Mott"
VersionInfoCopyright="Copyright (C) 2020, TJ Mott"
DefaultGroupName="TJ Mott's Writer"
OutputDir=.
OutputBaseFilename="TJ Mott's Writer v0.5.0 Setup"
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
AppContact="TJ Mott (tj@tjmott.com)"
AppPublisher="TJ Mott"
AppPublisherURL="https://www.tjmott.com/"
LicenseFile="..\..\LICENSE"

[Files]
Source: "..\win64\*.dll";                  DestDir: "{app}";
Source: "..\win64\*.exe";                  DestDir: "{app}";
Source: "..\win64\*.deps.json";            DestDir: "{app}";
Source: "..\win64\*.runtimeconfig.json";   DestDir: "{app}";
Source: "..\win64\Assets\editor.html";     DestDir: "{app}\Assets";
Source: "..\win64\Assets\quilljs\*.css";   DestDir: "{app}\Assets\quilljs";
Source: "..\win64\Assets\quilljs\*.js";    DestDir: "{app}\Assets\quilljs";
Source: "..\win64\Assets\quilljs\*.map";   DestDir: "{app}\Assets\quilljs";
Source: "..\win64\WordTemplates\*.dotx";   DestDir: "{app}\WordTemplates";
Source: "..\win64\README.md";              DestDir: "{app}";
Source: "..\win64\LICENSE";                DestDir: "{app}";                

[Icons]
Name: "{group}\Launch TJ Mott's Writer";      Filename: "{app}\TjMott.Writer.exe";
Name: "{group}\Uninstall TJ Mott's Writer";   Filename: "{uninstallexe}";
Name: "{group}\Visit TJ's Website";           Filename: "https://www.tjmott.com";
Name: "{group}\Visit Project Website";        Filename: "https://github.com/TjMott/TjMott.Writer";