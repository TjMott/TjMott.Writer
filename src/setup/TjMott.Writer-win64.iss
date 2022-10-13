[Setup]
AppId="TjMott.Writer"
AppName="TJ Mott's Writer"
AppVersion=0.5.0
DefaultDirName="{commonpf}\TJ Mott\TJ Mott's Writer"
VersionInfoCompany="TJ Mott"
VersionInfoCopyright="Copyright (C) 2020, TJ Mott"
DefaultGroupName="TJ Mott's Writer"
OutputDir=..\..
OutputBaseFilename="tjm-writer-v0.5.0-win64"
Compression=lzma
SolidCompression=yes
AppContact="TJ Mott (tj@tjmott.com)"
AppPublisher="TJ Mott"
AppPublisherURL="https://www.tjmott.com/"
LicenseFile="..\..\LICENSE"
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
SetupIconFile="..\..\win64\TjMott.Writer.ico"

[Registry]
Root: HKCR; Subkey: ".wdb";                                            ValueType: string; ValueData: "TjMottsWriter.WriterDatabase"; Flags: uninsdeletekey;
Root: HKCR; Subkey: "TjMottsWriter.WriterDatabase";                    ValueType: string; ValueData: "TJ Mott's Writer Database"; Flags: uninsdeletekey;
Root: HKCR; Subkey: "TjMottsWriter.WriterDatabase\Shell\Open\Command"; ValueType: string; ValueData: """{app}\TjMott.Writer.exe"" ""%1"""; Flags: uninsdeletekey;
Root: HKCR; Subkey: "TjMottsWriter.WriterDatabase\DefaultIcon";        ValueType: string; ValueData: "{app}\TjMott.Writer.ico"; Flags: uninsdeletekey;

[Files]
Source: "..\..\win64\*.dll";                  DestDir: "{app}";
Source: "..\..\win64\*.exe";                  DestDir: "{app}";
Source: "..\..\win64\*.deps.json";            DestDir: "{app}";
Source: "..\..\win64\*.runtimeconfig.json";   DestDir: "{app}";
Source: "..\..\win64\Assets\editor.html";     DestDir: "{app}\Assets";
Source: "..\..\win64\Assets\quilljs\*.css";   DestDir: "{app}\Assets\quilljs";
Source: "..\..\win64\Assets\quilljs\*.js";    DestDir: "{app}\Assets\quilljs";
Source: "..\..\win64\Assets\quilljs\*.map";   DestDir: "{app}\Assets\quilljs";
Source: "..\..\win64\WordTemplates\*.dotx";   DestDir: "{app}\WordTemplates";
Source: "..\..\win64\README.md";              DestDir: "{app}";
Source: "..\..\win64\LICENSE";                DestDir: "{app}";                
Source: "..\..\win64\*.ico";                  DestDir: "{app}";                

[Icons]
Name: "{group}\Launch TJ Mott's Writer";      Filename: "{app}\TjMott.Writer.exe";      IconFilename: "{app}\TjMott.Writer.ico";
Name: "{group}\Uninstall TJ Mott's Writer";   Filename: "{uninstallexe}";
Name: "{group}\Visit TJ's Website";           Filename: "https://www.tjmott.com";
Name: "{group}\Visit Project Website";        Filename: "https://github.com/TjMott/TjMott.Writer";

[UninstallDelete]
Type: filesandordirs;  Name: "{app}\Assets\cef-win64";
Type: filesandordirs;  Name: "{app}\GPUCache";
Type: files;           Name: "{app}\cefinstalled";
Type: files;           Name: "{app}\installingcef";
Type: files;           Name: "{app}\cef.tar.bz2";