# TjMott.Writer

This is my custom-developed word processor plus extras for writing/authoring as well as storing notes for writing jobs.

# NOTE: This application is a work in progress!

NOTE: This branch is not yet usable, but it's under active development. If you want a version that works, look at the master branch. This branch will replace master when it's ready.

# PLATFORM

This application runs on .NET 6.0 with an Avalonia GUI. It requires Visual Studio 2022 (Community Edition is fine) to compile. It is cross-platform and fully tested on Windows 10 and Linux Mint 20. Unofficially, it should work on any Linux that supports .NET 6, and will probably work on Mac with a bit of tweaking. Only 64-bit operating systems are supported. It should run on a 32-bit operating system with minimal changes, but why?

# DEPENDENCIES

If you want to build from source, you need the Chromium Embedded Framework (used to load the HTML/JS editor). Go to https://cef-builds.spotifycdn.com/index.html and download the correct one for your operating system, then extract the artifacts to src/TjMott.Writer/Assets/cef-yourplatform. Within that folder will be a lib folder for your DLL's or SO's (plus icudtl.dat, it's important that goes in the lib folder!), a resources folder, and a resources/locales folder.

This application uses CefNet, a .NET CLR binding for the Chromium Embedded Framework (CEF): https://github.com/CefNet/CefNet

The main editor control is based on Quill, a web-based rich text editor: https://quilljs.com/


# Basic Tutorial

## Concepts
This application organizes data into a number of structures described below.

### Save File
The application stores all of its data in an SQLite database file. This database contains everything you need--works, file repository, tickets, Markdown notes, and so on, all in a single file for convenience. The file extension is *.wdb for Works DataBase. If you are database-savvy, you can open this file in something like SQLite Browser if you're curious or need to adjust something the application doesn't directly support.

### Universe
A universe is a top-level container for a group of related works and supporting information. For example, a single universe will contain a list of stories, its own spellcheck dictionary, and the Markdown notes. Generally, unrelated works should go into different universes, though this is up to the author. For example, Star Wars and Star Trek would be different universes.

### Category
Categories are optional dividers inside a universe. They can be used in a number of different ways, it's all up to you. For example, you could organize stories by series name, by in-progress or completed status, or by work type (e.g. peoms, short stories, novels).

### Story
A story is  single publishable work, which could be a novel, novella, short story, or so on. Stories can optionally be organized into categories, or they can be attached directly to a universe.

Stories include properties such as title, author, ISBN, ASIN, copyright page, and edition number.

### Chapter
A chapter is a subcomponent of a story, used to organize scenes.

### Scene
A scene is a single continuous section of a story. Generally, if the narrator or setting changes, or if a large amount of time has passed, then a scene break should occur. Scenes are contained within chapters. Each scene contains one FlowDocument which has its actual text content.

### Flow Document (OUTDATED)
The actual text editor portion operates on a Microsoft datatype called a FlowDocument. This is an XML-based format for rich text editing. In addition, my implementation of the FlowDocument allows for optional AES encryption of its contents, allowing you to encrypt and password-protect contents if you deem necessary.

### Markdown Document (OUTDATED)
The notes portion of the application can be used to store and organize reference data, character details, plot ideas, timelines, and so on, using a text formatter called Markdown. The application has its own little HTML browser used to view and edit these Markdown documents. Markdown documents are not part of your work and are not included when you export a work to a Word document; they are merely for notes and supporting info you need as an author. Markdown documents are attached to a universe.

### Spellcheck Dictionary (OUTDATED)
Each universe has its own custom spellcheck dictionary. You can add words by right-clicking a misspelled word in the FlowDocument editor and adding it to the dictionary.

### Ticket Tracker
Each universe has an optional ticket tracker where you can keep track of your to-do items and status.

### File Repository (OUTDATED)
Each universe has a file repository where you can store files related to that universe. These can include images, published drafts/editions, and Word templates used for export. These are all contained within the application's save file format for convenience.

### Export to Word
Stories, chapters, and scenes can be exported to Word documents. Generally, exporting a story will give you the document you'd use to publish. However, this process is basic, so please look over and edit the document before submitting it!

You can use a Word template file when exporting, to set up things like font size, page size, margins, page numbers, etc. I'll document this later, but for now the installer includes one template file as an example. You'll need to import this template into the universe's file repository for it to be an option when exporting.