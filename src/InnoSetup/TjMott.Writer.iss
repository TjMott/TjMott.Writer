[Setup]
AppId="TjMott.Writer"
AppName="TJ Mott's Writer"
AppVersion=0.2.2
DefaultDirName="{commonpf}\TJ Mott\TJ Mott's Writer"
VersionInfoCompany="TJ Mott"
VersionInfoCopyright="Copyright (C) 2020, TJ Mott"
DefaultGroupName="TJ Mott's Writer"
OutputDir=.
OutputBaseFilename="TJ Mott's Writer v0.2.2 Setup"
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
AppContact="TJ Mott (tj@tjmott.com)"
AppPublisher="TJ Mott"
AppPublisherURL="https://www.tjmott.com/"
LicenseFile="..\..\LICENSE"

[Files]
Source: "..\TjMott.Writer\bin\Release\net5.0-windows\*.dll";                  DestDir: "{app}";
Source: "..\TjMott.Writer\bin\Release\net5.0-windows\*.exe";                  DestDir: "{app}";
Source: "..\TjMott.Writer\bin\Release\net5.0-windows\*.deps.json";            DestDir: "{app}";
Source: "..\TjMott.Writer\bin\Release\net5.0-windows\*.runtimeconfig.json";   DestDir: "{app}";
Source: "..\TjMott.Writer\bin\Release\net5.0-windows\README.md";              DestDir: "{app}";
Source: "..\TjMott.Writer\bin\Release\net5.0-windows\runtimes\*";             DestDir: "{app}\runtimes";         Flags: recursesubdirs;
Source: "..\..\LICENSE";                                                      DestDir: "{app}";                  DestName: "license.txt";

[Icons]
Name: "{group}\Launch TJ Mott's Writer";      Filename: "{app}\TjMott.Writer.exe";
Name: "{group}\Uninstall TJ Mott's Writer";   Filename: "{uninstallexe}";
Name: "{group}\Visit TJ's Website";           Filename: "https://www.tjmott.com";
Name: "{group}\Visit Project Website";        Filename: "https://github.com/TjMott/TjMott.Writer";