[Setup]
AppId="TjMott.Writer"
AppName="TJ Mott's Writer"
AppVersion=1.0.0
DefaultDirName="{commonpf}\TJ Mott\TJ Mott's Writer"
VersionInfoCompany="TJ Mott"
VersionInfoCopyright="Copyright (C) 2025, TJ Mott"
DefaultGroupName="TJ Mott's Writer"
OutputDir=..\..
OutputBaseFilename="tjm-writer-v1.0.0-win64"
Compression=lzma
SolidCompression=yes
AppContact="TJ Mott (tj@tjmott.com)"
AppPublisher="TJ Mott"
AppPublisherURL="https://www.tjmott.com/"
LicenseFile="..\..\LICENSE"
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
SetupIconFile="..\..\win64\TjMott.Writer.ico"

[Registry]
Root: HKA; Subkey: "Software\Classes\.wdb";                                            ValueType: string; ValueData: "TjMottsWriter.WriterDatabase";          Flags: uninsdeletekey;
Root: HKA; Subkey: "Software\Classes\TjMottsWriter.WriterDatabase";                    ValueType: string; ValueData: "TJ Mott's Writer Database";             Flags: uninsdeletekey;
Root: HKA; Subkey: "Software\Classes\TjMottsWriter.WriterDatabase\Shell\Open\Command"; ValueType: string; ValueData: """{app}\TjMott.Writer.exe"" ""%1""";    Flags: uninsdeletekey;
Root: HKA; Subkey: "Software\Classes\TjMottsWriter.WriterDatabase\DefaultIcon";        ValueType: string; ValueData: "{app}\TjMott.Writer.ico";               Flags: uninsdeletekey;

[Files]
Source: "..\..\win64\*";   DestDir: "{app}";  Flags: recursesubdirs;   Excludes: "*.pdb";

; Icons section isn't too useful anymore in Win11's turd of a Start Menu, unfortunately.
[Icons]
Name: "{group}\Launch TJ Mott's Writer";      Filename: "{app}\TjMott.Writer.exe";      IconFilename: "{app}\TjMott.Writer.ico";
Name: "{group}\Uninstall TJ Mott's Writer";   Filename: "{uninstallexe}";
Name: "{group}\Visit TJ's Website";           Filename: "https://www.tjmott.com";
Name: "{group}\Visit Project Website";        Filename: "https://github.com/TjMott/TjMott.Writer";

