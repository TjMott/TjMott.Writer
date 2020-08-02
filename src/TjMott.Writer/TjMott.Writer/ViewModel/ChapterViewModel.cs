﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class ChapterViewModel : ViewModelBase, ISortable, IExportToWordDocument, IGetWordCount
    {
        #region ISortable implementation - pass through to model
        public long SortIndex
        {
            get { return Model.SortIndex; }
            set { Model.SortIndex = value; }
        }
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SortIndex")
                OnPropertyChanged("SortIndex");
        }
        public void Save()
        {
            Model.Save();
        }
        #endregion

        #region ICommands
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

        private ICommand _createSceneCommand;
        public ICommand CreateSceneCommand
        {
            get
            {
                if (_createSceneCommand == null)
                {
                    _createSceneCommand = new RelayCommand(param => CreateScene());
                }
                return _createSceneCommand;
            }
        }
        #endregion

        public Chapter Model { get; private set; }
        public StoryViewModel StoryVm { get; set; }
        public SortBySortIndexBindingList<SceneViewModel> Scenes { get; private set; }

        public ChapterViewModel(Chapter model)
        {
            Model = model;
            Scenes = new SortBySortIndexBindingList<SceneViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public void Rename()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, Model.Name);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Name = dialog.UserInput;
                Model.Save();
            }
        }

        public void Delete()
        {
            ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("Chapter: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                StoryVm.DeleteChapter(this);
            }
        }

        public void CreateScene()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Scene");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FlowDocument doc = new FlowDocument(Model.Connection);
                doc.UniverseId = StoryVm.Model.UniverseId;
                doc.WordCount = 0;
                doc.PlainText = "";
                doc.Xml = FlowDocumentViewModel.GetEmptyFlowDocXml();
                doc.Create();

                Scene scene = new Scene(Model.Connection);
                scene.ChapterId = Model.id;
                scene.Name = dialog.UserInput;
                if (Scenes.Count == 0)
                    scene.SortIndex = 0;
                else
                    scene.SortIndex = Scenes.Max(i => i.Model.SortIndex) + 1;
                scene.FlowDocumentId = doc.id;
                scene.Create();
                SceneViewModel sceneVm = new SceneViewModel(scene);
                sceneVm.ChapterVm = this;
                Scenes.Add(sceneVm);
            }
        }

        public void DeleteScene(SceneViewModel scene)
        {
            Scenes.Remove(scene);
        }

        public void UpdateSceneSortIndices()
        {
            for (int i = 0; i < Scenes.Count; i++)
            {
                Scenes[i].Model.SortIndex = i;
                Scenes[i].Save();
            }
        }
    }
}
