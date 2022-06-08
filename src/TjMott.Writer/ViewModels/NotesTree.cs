﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class NotesTree : ViewModelBase
    {
        public UniverseViewModel UniverseVm { get; private set; }

        public ObservableCollection<NotesTreeItem> Items { get; private set; }
        public ObservableCollection<NoteCategoryViewModel> Categories { get; private set; }

        public NotesTree(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Items = new ObservableCollection<NotesTreeItem>();
            Categories = new ObservableCollection<NoteCategoryViewModel>();
        }

        public async Task LoadAsync()
        {
            Items.Clear();
            Categories.Clear();

            // Load DB models for this universe.
            List<NoteCategory> dbCategories = (await NoteCategory.LoadAll(UniverseVm.Database.Connection)).Where(i => i.UniverseId == UniverseVm.Model.id).ToList();
            List<NoteCategoryDocument> dbCatDocs = (await NoteCategoryDocument.LoadAll(UniverseVm.Database.Connection)).Where(i => dbCategories.Select(k => k.id).Contains(i.NoteCategoryId)).ToList();
            List<NoteDocument> dbDocs = (await NoteDocument.LoadAll(UniverseVm.Database.Connection)).Where(i => i.UniverseId == UniverseVm.Model.id).ToList();

            // Create ViewModels.
            List<NoteCategoryViewModel> categories = dbCategories.Select(i => new NoteCategoryViewModel(i, UniverseVm)).ToList();
            List<NoteDocumentViewModel> docs = dbDocs.Select(i => new NoteDocumentViewModel(i, UniverseVm)).ToList();

            // Link up categories.
            foreach (var cat in categories)
            {
                Categories.Add(cat);
                if (cat.Model.ParentId == null)
                {
                    Items.Add(cat);
                }
                else
                {
                    NoteCategoryViewModel parent = categories.Single(i => i.Model.id == cat.Model.ParentId);
                    cat.Parent = parent;
                    parent.Children.Add(cat);
                }

                // Find any documents that belong to this category.
                var catDocs = dbCatDocs.Where(i => i.NoteCategoryId == cat.Model.id).ToList();
                foreach (var item in catDocs)
                {
                    NoteDocumentViewModel doc = docs.Single(i => i.Model.id == item.NoteDocumentId);
                    doc.Parent = cat;
                    cat.Children.Add(doc);
                }
            }

            // Find any documents that are not in a category.
            foreach (var doc in docs.Where(i => i.Parent == null).ToList())
            {
                Items.Add(doc);
            }
        }
    }
}
