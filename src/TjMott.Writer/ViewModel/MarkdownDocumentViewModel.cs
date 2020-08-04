﻿using Markdig;
using System;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;
using TjMott.Writer.Windows;

namespace TjMott.Writer.ViewModel
{
    public class MarkdownDocumentViewModel : MarkdownTreeItem
    {

        #region Markdown stuff
        private static MarkdownPipeline _markdownPipeline;
        static MarkdownDocumentViewModel()
        {
            MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder();
            builder.UsePipeTables();
            builder.UseSoftlineBreakAsHardlineBreak();
            builder.UseTaskLists();
            _markdownPipeline = builder.Build();
        }
        #endregion

        #region ICommands
        private ICommand _openInWindowCommand;
        public ICommand OpenInWindowCommand
        {
            get
            {
                if (_openInWindowCommand == null)
                {
                    _openInWindowCommand = new RelayCommand(param => OpenInWindow());
                }
                return _openInWindowCommand;
            }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => Delete());
                }
                return _deleteCommand;
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(param => Save());
                }
                return _saveCommand;
            }
        }
        private ICommand _revertCommand;
        public ICommand RevertCommand
        {
            get
            {
                if (_revertCommand == null)
                {
                    _revertCommand = new RelayCommand(param => Revert());
                }
                return _revertCommand;
            }
        }
        private ICommand _renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if (_renameCommand == null)
                {
                    _renameCommand = new RelayCommand(param => Rename());
                }
                return _renameCommand;
            }
        }
        private ICommand _editCategoriesCommand;
        public ICommand EditCategoriesCommand
        {
            get
            {
                if (_editCategoriesCommand == null)
                {
                    _editCategoriesCommand = new RelayCommand(param => EditCategories(), param => CanEditCategories());
                }
                return _editCategoriesCommand;
            }
        }
        #endregion

        public MarkdownDocument Model { get; set; }
        public MarkdownDocumentViewModel(MarkdownDocument model, UniverseViewModel universe)
        {
            Model = model;
            UniverseVm = universe;
        }

        public string Html
        {
            get
            {
                return UniverseVm.Model.MarkdownCss + Markdig.Markdown.ToHtml(Model.MarkdownText, _markdownPipeline);
            }
        }

        public void OpenInWindow()
        {
            MarkdownDocumentWindow.ShowMarkdownDocument(this);
        }

        public void Delete()
        {

        }

        public void Save()
        {
            Model.Save();
            OnPropertyChanged("Html");
        }

        public void Revert()
        {
            Model.Load();
            OnPropertyChanged("Html");
        }

        public void Rename()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, Model.Name);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Name = dialog.UserInput;
            }
        }

        public void EditCategories()
        {
            EditMarkdownDocumentCategoriesDialog dialog = new EditMarkdownDocumentCategoriesDialog(DialogOwner, this, UniverseVm.MarkdownTree);
            dialog.ShowDialog();
        }
        public bool CanEditCategories()
        {
            return UniverseVm.MarkdownTree.Categories.Count > 0;
        }
    }
}
