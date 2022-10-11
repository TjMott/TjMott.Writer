# TjMott.Writer

This is my custom-developed word processor plus extras for writing/authoring as well as storing notes for writing jobs.

## PLATFORM

This application runs on .NET 6.0 with an Avalonia GUI. It requires Visual Studio 2022 (Community Edition is fine) to compile. It is cross-platform and fully tested on Windows 10 and Linux Mint 20 (Cinnamon desktop). Unofficially, it should work on any Linux that supports .NET 6, and will probably work on Mac with a bit of tweaking. Only 64-bit operating systems are supported.

### LINUX NOTES

Officially, the only supported Linux is Linux Mint running the Cinnamon desktop. It will likely work on other Linuxes just fine, especially if you use Cinnamon, but it hasn't been tested. You may encounter UI issues related to different desktop managers. Avalonia UI is still a work-in-progress so some UI issues are likely with Avalonia and not this application.

### .NET INSTALLATION

#### Windows

Download and install the 64-bit .NET Desktop runtime from Microsoft here: https://dotnet.microsoft.com/en-us/download/dotnet/6.0

#### Debian-based Linuxes (Debian, Ubuntu, Mint)

`sudo apt-get install dotnet-sdk-6.0`

#### Red Hat-based Linuxes (Red Hat, Fedora, CentOS, Rocky)

`sudo dnf install dotnet-sdk-6.0`

## BUILDING FROM SOURCE

Building from source is pretty easy as long as you have Visual Studio 2022 Community Edition installed on Windows, or the .NET 6 SDK on Linux. Do a checkout from Git, open a shell or command prompt, cd into the src/setup folder, and execute the appropriate publish script. If you want to build an installer, use InnoSetup on Windows or call the appropriate build script for your Linux distribution.

## DEPENDENCIES

This application uses CefNet, a .NET CLR binding for the Chromium Embedded Framework (CEF), which is automatically downloaded and installed at runtime: https://github.com/CefNet/CefNet

The main editor control is based on Quill, a web-based rich text editor: https://quilljs.com/

Export to Word relies on Xceed.Docx: https://github.com/xceedsoftware/docx

## Intended Workflow

Use TJ Mott's Writer to store notes and create chapters and scenes. You can easily reorder chapters and scenes, or move scenes between chapters. Once you are happy with the flow of the story, export the story to a Word docx file. Look over the docx file and do any final formatting--page numbers, headers/footers, text formatting not supported by TJ Mott's writer--and then you can publish the docx to KDP or provide it to your publisher.

Note that this application has limitations in formatting. You almost certainly will need to do your final formatting in Word or LibreOffice Writer before publishing!

## Basic Concepts

This application organizes data into a number of structures described below.

### Save File
The application stores all of its data in an SQLite database file. This database contains everything you need--works, file repository, tickets, Markdown notes, and so on, all in a single file for convenience. The file extension is *.wdb for Works DataBase. If you are database-savvy, you can open this file in something like SQLite Browser if you're curious or need to adjust something the application doesn't directly support.

### Universe
A universe is a top-level container for a group of related works and supporting information. For example, a single universe will contain a list of stories and notes. Generally, unrelated works should go into different universes, though this is up to the author. For example, Star Wars and Star Trek would probably be different universes.

### Category
Categories are optional dividers inside a universe. They can be used in a number of different ways, it's all up to you. For example, you could organize stories by series name, by in-progress or completed status, or by work type (e.g. peoms, short stories, novels). I typically use them for series.

### Story
A story is  single publishable work, which could be a novel, novella, short story, or so on. Stories can optionally be organized into categories, or they can be attached directly to a universe.

Stories include properties such as title, author, ISBN, ASIN, copyright page, and edition number.

### Chapter
A chapter is a subcomponent of a story, used to organize scenes. When exported to Word, the chapter's name will be used as a formatted section header.

### Scene
A scene is a single continuous section of a story. Generally, if the narrator or setting changes, or if a large amount of time has passed, then a scene break should occur. Scenes are contained within chapters. Each scene contains one Document which has its actual text content. If a chapter contains multiple scenes, they will be separated with asterisks (a scene break) when exported to Word.

### Document
The actual text editor portion operates on a QuillJS datatype called Delta. This is a JSON-based format for rich text editing. Scenes, copyright pages, and notes are all stored as this same Document format. Additionally, documents can be AES-encrypted, allowing you to protect documents with a password. Documents can be printed.

### Export to Word
Stories, chapters, and scenes can be exported to Word documents. Generally, exporting a story will give you the document you'd use to publish. However, this process is basic, so please look over and edit the document before submitting it!

You can use a Word template file when exporting, to set up things like font size, page size, margins, page numbers, etc. I'll document this later, but for now the installer includes one template file as an example.

