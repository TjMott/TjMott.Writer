
# TJ Mott's Writer

This is my word processor with extra features helpful to writers and authors, especially geared towards novelists. It is an open source application with a BSD 3-clause license. It's completely free to download and use, but if you find it useful and you want to support me, you're welcome to head over to Amazon and purchase some of my books. :)

- Source code: [https://github.com/TjMott/TjMott.Writer](https://github.com/TjMott/TjMott.Writer)
- License: [https://github.com/TjMott/TjMott.Writer/blob/master/LICENSE](https://github.com/TjMott/TjMott.Writer/blob/master/LICENSE)
- My website: [https://www.tjmott.com](https://www.tjmott.com)
- Contact Me: [https://www.tjmott.com/contact-tj-mott](https://www.tjmott.com/contact-tj-mott/)
- My Amazon.com author page (shameless plug): [https://www.amazon.com/author/tjmott](https://www.amazon.com/author/tjmott)

The change log is at the end of this readme.

## Supported Platforms

TJ Mott's Writer is cross-platform and fully tested/supported on Windows 11 and Linux Mint 22. Unofficially, it should work on any Linux that supports .NET 8, and it'll probably work on Mac. Only 64-bit operating systems are supported.

Mobile (Android/iOS) support is not on my radar. There are dependencies that only work on desktop operating systems.


## Installation

#### Windows

1. Download and install the [64-bit .NET 8 Desktop runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) from Microsoft.
2. Then, grab the latest Windows installer from [my releases page](https://github.com/TjMott/TjMott.Writer/releases).
3. Once downloaded, you may have to right-click the file, go to "Properties", and check the "Unblock" checkbox before Windows will allow you to install it.
4. Double-click to install.
5. Now you can launch TJ Mott's Writer from your Start Menu. 

#### Debian-based Linuxes (Debian, Ubuntu, Mint)

1. Install the .NET 8 SDK and X11 dependency: `sudo apt-get install dotnet-sdk-8.0 libx11-dev`
2. Download the latest `.deb` file from [my releases page](https://github.com/TjMott/TjMott.Writer/releases).
3. Install the deb file: `sudo dpkg -i tjm-writer_1.0.0-1_amd64.deb`
4. Now you can launch TJ Mott's Writer from your launcher, or by executing `/opt/TjMott.Writer/TjMott.Writer`

#### Red Hat-based Linuxes (Red Hat, Fedora, CentOS, Rocky)

1. Install the .NET 8 SDK and X11 dependency: `sudo dnf install dotnet-sdk-8.0 libX11-devel`
2. Download the latest `.rpm` file from [my releases page](https://github.com/TjMott/TjMott.Writer/releases).
3. Install the rpm: `sudo rpm -i tjm-writer-1.0.0-1.x86_64.rpm`
4. Now you can launch TJ Mott's Writer from your launcher, or by executing `/opt/TjMott.Writer/TjMott.Writer`

#### Portable Installation

There are portable installations for both Windows and Linux available from my releases page. Download and extract the zip file for Windows and run `TjMott.Writer.exe`, or the tar.gz file on Linux and run `TjMott.Writer` from a terminal. You still need .NET 8 on all platforms, and the libx11 dependency on Linux. Note that you don't get Start Menu/launcher entries or .wdb file associations with the portable installation.


## Upgrading

TJ Mott's Writer will automatically upgrade your .wdb files if the file format changes between versions. However, it's always recommended to back them up first just in case! If you have any issues upgrading .wdb's or notice a loss of data, please contact me with details so I can investigate.

If you are upgrading from the old Windows-only version (<= 0.4.0), please be aware that some features went away! The file repository, ticket tracker, and internal spellcheck dictionaries no longer exist, and all data associated with them will be deleted! In addition, you may encounter formatting issues with documents and especially with notes as they are updated to the new format.

If you're updating from <= 0.4.0, you may see some corruption with notes. Specifically, note titles get shuffled around and don't match the note contents--there's no data loss, the titles just aren't right. This appears to be a latent SQL trigger bug in an older version of the software. You'll have to correct these on your own after the upgrade, but they should not occur again.

Finally, release 1.0.0 deprecates the old concept of notes. Note documents will be consolidated into a "Notes" category in the main view, so you won't lose any notes, they'll just be treated as any other document and will no longer be a separate feature.

## Basic Concepts

Here's a basic tutorial for the application. I hope it's fairly self-explanatory as you jump in and use it. Keep in mind my labels fit my workflow; you can certainly use the app any way you want!

### Intended Workflow

Use TJ Mott's Writer to store notes and create chapters and scenes/sections. You can easily reorder chapters and scenes, or move scenes between chapters. Once you are happy with the flow of the story, export the story to a Word docx file. Look over the docx file and do any final formatting--page numbers, headers/footers, text formatting not supported by TJ Mott's Writer--and then you can provide it to your publisher or upload to KDP.

Note that this application has limitations in formatting. It has most of what I need as a novelist, but you almost certainly will need to do some final formatting in Word or LibreOffice Writer before publishing! For example, lists and images are not currently supported.

### Save File

The application stores all of its data in a .wdb (Writer DataBase) file. You can use multiple .wdb's if you'd like (using the new/save/open options in the File menu) but my workflow is to keep all of my work in the same .wdb and use universes as logical separators.

### Universe

A universe is a top-level container within a .wdb for a group of related works and supporting information. A single universe will contain a list of stories and notes. Generally, unrelated works should probably go into different universes. For example, Star Wars and Star Trek would probably be different universes.

The Search tab will only search within the current universe.

### Category

Categories are optional dividers inside a universe. They can be used in a number of different ways, it's all up to you. For example, you could organize stories by series name, by in-progress or completed status, or by work type (e.g. peoms, short stories, novels). I typically use them for series.

### Story

A story is a single publishable work, which could be a novel, novella, short story, or so on. Stories can optionally be organized into categories, or they can be attached directly to a universe.

Stories include properties such as title, author, ISBN, ASIN, copyright page, and edition number.

### Chapter

A chapter is a subcomponent of a story, used to organize scenes. When exported to Word, the chapter's name will be used as a formatted section header.

### Scene

A scene is a single continuous section of a story. Generally, if the narrator or setting changes, or if a large amount of time has passed, then a scene break should occur. Scenes are contained within chapters. Each scene contains one Document which has its actual text content. If a chapter contains multiple scenes, they will be separated with asterisks (a scene break) when exported to Word.

Scenes can be colored in the main tree view using the right-click context menu. These colors are arbitrary and can mean whatever you want. I like to use them to help categorize while editing--green scenes are complete, yellow scenes need work, orange scenes need a ton of work or even rewriting, and red scenes are candidates for deletion.

Scene names and colors are for your internal use only, and are not included in exported documents.

### Document

The actual text editor window operates on a QuillJS datatype called Delta. This is a JSON-based format for rich text documents. Scenes, copyright pages, and notes are all stored as this same Document format. Additionally, documents can be encrypted with AES256, allowing you to protect documents or journal entries with a password.

### Menu Buttons vs. Context Menus

There is a horizontal button panel on the bottom side of the main window which will operate on the currently-selected item in the tree. Use these to open the editor, re-order items in the list, or export to word. You can also right-click on items in the tree and get more options in a context menu.

### Export to Word .docx

Stories, chapters, and scenes can be exported to Word documents. Generally, exporting a story will give you the document you'd use to publish. However, this process is basic and some formatting conversions might not be handled correctly yet, so please look over your document carefully before submitting it to your publisher!

You can use a Word template file when exporting, to set up things like font size, page size, margins, page numbers, etc. I'll document this later, but for now the installer includes one template file as an example.

Encrypted scenes cannot be exported to Word and will be skipped.

### Pacing Dialog

The Pacing Dialog is a context menu option on stories and chapters which gives you a visual representation of the size of each scene/section compared to the whole story. It can be useful when visualizing the pace of your story, or determining how far in a certain plot point occurs.

It can also be used to compare the relative size of your supporting plots. Sometimes when I write, I don't do things in chronological or linear order. I'll start with placeholder chapters where each chapter is a self-contained plot or idea, then I can use the Pacing Dialog to make sure they are appropriately-sized. For example, to make sure the main plot is most of the story, and that a subplot with a minor character isn't taking over instead!

### Word Count

There is a word count display in the editor window which shows you the word count of the currently-opened document. But you can also use the sidebar button in the main window to get a word count for the selected item, whether it's a scene, story, or even an entire category!


## SCARY TECHNICAL INFO FOR DEVELOPERS, POWER USERS, AND OTHER NERDS

### WDB is SQLite

The .wdb files are [SQLite](https://www.sqlite.org/index.html) databases. Everything is stored as records in a relational database in third normal form. You can crack this file open with any SQLite client, such as [DB Browser for SQLite](https://sqlitebrowser.org/). There are helper classes/attributes to map SQLite tables to C# classes, which I should probably publish as its own library sometime.

The main search feature makes heavy use of [SQLite's full-text search module](https://www.sqlite.org/fts5.html), and there are a number of fts5 tables and triggers to support it.

### Debug mode has more features (...ish)

The installers only include Release mode binaries. But if you build locally and use the Debug configuration, you'll get an extra "Debug" menu in the main window with some extra features. There's nothing too fascinating in there, just some stuff that has helped me while coding/testing/experimenting and might even corrupt your .wdb file.

### Main Dependencies

The application framework is [Avalonia UI](https://github.com/AvaloniaUI/Avalonia), a cross-platform GUI toolkit for .NET 8 which is heavily inspired by the Windows Presentation Foundation (WPF) framework. The older Windows-only versions of TJ Mott's Writer used WPF which made Avalonia a natural choice when I began working towards cross-platform support.

The main editor control is built upon [Quill](https://quilljs.com/), a web-based rich text editor created in HTML/JavaScript/CSS. Quill is checked into source and distributed with the application, not loaded from a CDN. Quill uses its own JSON-based file format called ["delta"](https://quilljs.com/docs/delta/) which I store as plain text records inside the .wdb's "Document" table.

I use the [Chromium Embedded Framework (CEF)](https://github.com/chromiumembedded) to create a browser frame which hosts the Quill editor. A Nuget package called [CefGlue](https://github.com/OutSystems/CefGlue) provides the bridge between the application's main C# logic and the editor's JavaScript/JSON.

Using an HTML/JS editor inside an embedded Chromium frame might seem pretty weird, and it definitely wasn't my first choice. I'd have preferred a pure C# document editor. But I wasn't able to find a cross-platform option that fit my needs, nor was I ready to invest the time to write my own editor control. I played with Quill and it fit all my needs, hence the weird mashup of C#/.NET and HTML/JavaScript with Chromium glue.

There are a few other Nuget dependencies which you can find in the .csproj file. Stuff like SQLite, font enumeration, creating Word .docx files, and extracting zip/tar.gz files.

### Formatting Limitations

QuillJS is very full-featured and mature, but I have intentionally disabled some of its formatting options. The main issue is with writing/testing/supporting the code that converts from Quill's JSON format to Word's .docx format during export operations, and so I have limited the application to the formatting features that I actually need and use as a novelist. There may still be some cases that do not convert correctly during an export, so please check your .docx files carefully!

If there are unimplemented formatting features you'd like, feel free to contact me or send me a pull request.

If you want to explore Quill's JSON format, either open the .wdb in [DB Browser for SQLite](https://sqlitebrowser.org/) and poke around in the Document table, or use a debug build of the application and choose the "Show JSON" item from the scene context menu in the main treeview. This information will be very helpful if you decide to add formatting/conversion code of your own.

### Building From Source

Building from source is pretty easy. You can download the source for an official release from https://github.com/TjMott/TjMott.Writer/releases as a zip or tarball, or use a git client to clone the repo and checkout a tag. All compile-time dependencies are managed by Nuget and should be automatically downloaded when you build. If you have the .NET 8 SDK installed, all you should have to do is run the appropriate build script for your operating system.

I highly suggest only building from tags. Using a build from master for anything other than testing/experimentation should be avoided because the .wdb schema may be in flux. Tagged releases will always include SQL scripts to update the .wdb schema from previous tagged releases. But if you work off master, the database schema may not be final and the upgrade scripts may be wrong or nonexistent. It's quite possible you'll encounter runtime errors or data loss if you use a .wdb from a previous version/build, and very likely you'll create a .wdb that can't be opened by future releases.

#### Windows

If you only want to build, you need the [64-bit Visual Studio 2022 SDK for .NET 8.0]( https://dotnet.microsoft.com/en-us/download/visual-studio-sdks). Run `src/setup/publish-win64.bat` to create binaries at `win64`. Start the application by running `win64/TjMott.Writer.exe`.

If you want to generate an installer, compile `src/setup/TjMott.Writer-win64.iss` with [InnoSetup 6](https://jrsoftware.org/isdl.php) and you'll have an MSI. The MSI does some nice things like Start Menu entries and registering the .wdb file extension with the application.

If you want to dig around in the source, then I highly recommend installing [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/). And if you want the GUI designer to work, you also need the [Avalonia extension](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaVS). At that point you can open `src/TjMott.Writer.sln` in Visual Studio. The build/package scripts in `src/setup` aren't aware of Visual Studio's output in `src/TjMott.Writer/bin`.

#### Linux

The .NET 8 SDK and X11 development dependencies must be installed first, using either `sudo apt-get install dotnet-sdk-8.0 libx11-dev` or `sudo dnf install dotnet-sdk-8.0 libX11-devel`. Then run `src/setup/publish-linux64.sh` to create binaries at `linux64` and a portable .tar.gz package in the repository root. Start the application by running `src/linux64/TjMott.Writer` from a terminal.

If you want to generate a .rpm or .deb installer, next run `src/setup/build-rpm-package.sh` or `src/setup/build-deb-package.sh`. The installers install to `/opt/TjMott.Writer`. They also register a new MIME type (application/tjm-writer) to `/usr/share/mime/packages` and place a .desktop file in `/usr/share/applications`, which adds the application to your launcher and associates .wdb files with it.

#### Mac

Apple/Mac is completely unsupported and untested, however the code should compile and run on Mac with relatively few changes. The Windows and Linux codepaths are 100% identical and I don't expect Mac to be any different.

Then I expect it'll require significant time and effort, and probably some expensive accounts and certificates to do whatever Apple requires to properly create a distributable package.

Apple support is not on my radar, but I will gladly accept pull requests.

### Privacy Notes

I take data privacy seriously. Once fully installed, TJ Mott's Writer should not require an Internet connection. It should never use the Internet except for initially downloading CEF. QuillJS is loaded locally from the application directory using a `file:///` URI, and not from a CDN.

A previous implementation using CefNet provided a lot of switches for my app to enable privacy features and prevent CEF from sending telemetry and other web requests to the Internet. Unfortunately, CefNet became unmaintained and the source was removed from GitHub. I had to migrate to CefGlue to update from AvaloniaUI 0.10 to the modern 11 version, and CefGlue doesn't provide nearly as much control over the embedded browser's behavior. I will make all reasonable efforts to provide data privacy, but I'm not responsible for any data leaks or IP theft that occur as a result of CEF doing unsavory things at runtime.

## Change Log

###Version 1.0.1

### Changes

* Downgraded Quill editor from 2.0.3 to 2.0.2 due to this bug: https://github.com/slab/quill/issues/4535
* Added an item to the document editor "Options" menu to replace non-breaking spaces with breaking spaces. If you edited documents with version 1.0.0 of this application, and they exhibit some really strange text-wrapping behavior in the editor, use this feature to fix it up.
* Enabled list formatting in editor. However, this does not export to Word correctly, so any exported docs will need some cleanup.

###Version 1.0.0

### Changes

* Upgraded from old beta 0.10 AvaloniaUI to 11.3.2.
* Updated Quill editor from 1.3.7 to 2.0.3.
* Migrated from CefNet to CefGlue due to lack of Avalonia 11 support from CefNet.
* CEF binaries are now packaged in the installer. No more CEF install process on first run.
* Lots of performance updates and improved features due to Avalonia framework updates, e.g. proper light/dark theme support.
* Updated to .NET 8.0.
* Main view reworked to improve loading speed when using a large works database.
* Greatly improved SQLite performance.
* Fixed cursor jumping to wrong position after auto-replacing certain characters (em dashes, left/right quotes, ellipses, etc.).
* Added menu option to enable or disable character autoreplace (quotes, em dashes, ellipses..etc).
* Document encryption now follows the standard practice of random data encryption keys for each item, which are encrypted with a password-derived key encryption key.
* Removed "Notes" feature to reduce maintenance footprint. Existing notes will be consolidated into a category in the main documents view.
* Editor assets hash verification fixed--you will get a warning if something (e.g. malware) modifies the editor HTML/JS/CSS files.

#### Known Issues

* Editor will not auto-replace when pasting in text containing characters that are normally auto-replaced when typed in directly (em dashes, left/right quotes, ellipses, etc.).
* The in-app Readme does not work (normally shows this document). This is due to a broken package dependency that really needs to update for the latest Avalonia.
* Printing does not work in Linux.
* Export to Word does not generate Table of Contents correctly.
* Linux console is pretty spammy about gpu_process issues. Something from CefGlue, but it doesn't seem to hurt anything.

### Version 0.5.2

#### New Features

* Enabled printing for notes documents.

#### Bug Fixes

* Fixed unable to delete a chapter
* Improved (but not necessarily fixed) weirdness when re-ordering chapters/stories/etc in the tree view.
* CEF installation cookie now contains the CEF version, so application updates will handle CEF updates correctly.

### Version 0.5.1

#### New Features

* Added editor autoreplace for smart quotes, ellipses, and em dashes while typing.

#### Bug Fixes

* Fixed crash when creating a new scene (Document.DocumentType was left null, but underlying SQLite column is NOT NULL).
* Fixed slightly-incorrect Debian installation instructions in this readme.

#### Known Issues

* Editor autoreplace for smart quotes, ellipses, and em dashes may jump cursor position ahead if your cursor is not at the end of a line when the autoreplace happens. Potentially a QuillJS bug -- quill.setSelection is unpredictable after calling quill.clipboard.dangerouslyPasteHTML.
* Editor autoreplace may not work as expected if you paste in a chunk of text containing autoreplaceable characters.
* Missing libraries/dependencies on Ubuntu 22.04 or Mint 21.
* Linux: Double-clicking a .wdb file launches the application, but does not load the file you double-clicked.

### Version 0.5.0

#### New Features

* Completed migration to .NET 6 and Avalonia UI, with official support for Windows 10, Rocky Linux 8, and Linux Mint 20.

#### Removed Features

* Ticket tracker
* File repository
* Spellcheck dictionary


