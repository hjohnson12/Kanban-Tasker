using KanbanTasker.Base;
using KanbanTasker.DataAccess;
using KanbanTasker.Models;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.ViewModels
{
    public class BoardViewModel : Observable
    {
        //=====================================================================
        // VARIABLES & BACKING FIELDS
        //=====================================================================

        public CustomKanbanModel Task = new CustomKanbanModel();
        private CustomKanbanModel _originalCardModel;
        private CustomKanbanModel _cardModel;
        private ObservableCollection<string> _tagsCollection;
        private ObservableCollection<CustomKanbanModel> _tasks;
        private List<string> _categories;
        private List<string> _colorKeys;
        private string _paneTitle;
        private string _boardName;
        private string _boardDescription;
        private string _boardNotes;

        //=====================================================================
        // CONSTRUCTOR
        //=====================================================================

        public BoardViewModel()
        {
            Tasks = DataProvider.GetData();
        }

        //=====================================================================
        // PROPERTIES
        //=====================================================================

        public ObservableCollection<CustomKanbanModel> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public CustomKanbanModel CardModel
        {
            get { return _cardModel; }
            set
            {
                _cardModel = value;

                // Update Task Properties 

                if (_cardModel == null) // New Task
                {
                    ID = null;
                    Title = null;
                    Description = null;
                    Category = null;
                    ColorKey = null;
                    Tags = new string[] { };
                    TagsCollection = new ObservableCollection<string>();
                    OnPropertyChanged();
                }
                else // Edit Task
                {
                    ID = _cardModel.ID;
                    Title = _cardModel.Title;
                    Description = _cardModel.Description;
                    Category = _cardModel.Category.ToString();
                    ColorKey = _cardModel.ColorKey.ToString();
                    Tags = _cardModel.Tags;
                    OnPropertyChanged();
                }
            }
        }

        public string ID
        {
            get
            {
                if (Task.ID == null)
                    return "";
                else
                    return Task.ID;
            }
            set
            {
                Task.ID = value;
                OnPropertyChanged();
            }
        }

        public string BoardId
        {
            get
            {
                if (Task.BoardId == null)
                    return "";
                else
                    return Task.BoardId;
            }
            set
            {
                Task.BoardId = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                if (Task.Title == null)
                    return "";
                else
                    return Task.Title;
            }
            set
            {
                Task.Title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Description
        {
            get
            {
                if (Task.Description == null)
                    return "";
                else
                    return Task.Description;
            }
            set
            {
                Task.Description = value;
                OnPropertyChanged("Description");
            }
        }

        public List<string> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public object Category
        {
            get
            {
                return Task.Category;
            }
            set
            {
                Task.Category = value;
                OnPropertyChanged("Category");
            }
        }

        public List<string> ColorKeys
        {
            get { return _colorKeys; }
            set
            {
                _colorKeys = value;
                OnPropertyChanged();
            }
        }

        public object ColorKey
        {
            get
            {
                return Task.ColorKey;
            }
            set
            {
                Task.ColorKey = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> TagsCollection
        {
            get { return _tagsCollection; }
            set
            {
                _tagsCollection = value;
                OnPropertyChanged();
            }
        }

        public string[] Tags
        {
            get { return Task.Tags; }
            set
            {
                Task.Tags = value;
                OnPropertyChanged();
            }
        }

        public string PaneTitle
        {
            get { return _paneTitle; }
            set
            {
                _paneTitle = value;
                OnPropertyChanged();
            }
        }

        public string BoardName
        {
            get { return _boardName; }
            set
            {
                _boardName = value;
                OnPropertyChanged("BoardTitle");
            }
        }

        public string BoardNotes
        {
            get { return _boardNotes; }
            set
            {
                _boardNotes = value;
                OnPropertyChanged("BoardDescription");
            }
        }

        public CustomKanbanModel OriginalCardModel
        {
            get;
            set;
        }

        //=====================================================================
        // VIEW MODEL FUNCTIONS
        //=====================================================================

        public void AddTagToCollection(string tag)
        {
            TagsCollection.Add(tag);
        }

        public void EditTaskHelper(CustomKanbanModel selectedModel, List<string> categories, List<string> colorKeys, ObservableCollection<string> tags)
        {
            // Get content ready to show in splitview pane
            OriginalCardModel = selectedModel;
            CardModel = selectedModel;
            Categories = categories;
            ColorKeys = colorKeys;
            TagsCollection = tags;
            PaneTitle = "Edit Task";
        }

        public void SaveTask(string tags, object selectedCategory, object selectedColorKey, CustomKanbanModel selectedCard)
        {
            // Tags are stroed as string[] in CustomKanbanModel
            // Strip string into a string[]
            string[] tagsArray;
            if (tags == "")
                tagsArray = new string[] { };
            else
                tagsArray = tags.Split(",");

            // Update model
            var selectedModel = selectedCard;
            selectedModel.Title = Title;
            selectedModel.Description = Description;
            selectedModel.Category = selectedCategory;
            selectedModel.ColorKey = selectedColorKey;
            selectedModel.Tags = tagsArray;

            // Update item in database
            DataProvider.UpdateTask(ID, Title,
                Description, selectedCategory.ToString(),
                selectedColorKey.ToString(), tags);
        }

        public bool DeleteTask(CustomKanbanModel model)
        {
            var previousCount = Tasks.Count;
            Tasks.Remove(model);
            DataProvider.DeleteTask(model.ID); // Delete from database
            CardModel = null;

            // Determine if deletion was successful
            return (Tasks.Count == (previousCount - 1)) ? true : false;
        }

        public void NewTaskHelper(List<string> categories, List<string> colorKeys)
        {
            CardModel = null; // Null card for new task
            Categories = categories;
            ColorKeys = colorKeys;
            PaneTitle = "New Task";
        }

        public bool AddTask(string tags, object selectedCategory, object selectedColorKey)
        {
            // Tags are stored as as string[] in CustomKanbanModel
            // Strip string into a sting[]
            string[] tagsArray = new string[] { };
            if (tags != null)
                tagsArray = tags.Split(',');
            else
                tags = ""; // No tags

            var boardId = App.mainViewModel.Current.BoardId.ToString();

            // Create model and add to Tasks collection
            var model = new CustomKanbanModel
            {
                BoardId = boardId,
                Title = Title,
                Description = Description,
                Category = selectedCategory,
                ColorKey = selectedColorKey,
                Tags = tagsArray
            };

            // Add task to database
            int newTaskID = DataProvider.AddTask(boardId, Title,
                Description, selectedCategory.ToString(),
                selectedColorKey.ToString(), tags);

            var previousCount = Tasks.Count;
            model.ID = newTaskID.ToString();
            Tasks.Add(model);

            // Determine if insertion was successful
            return (Tasks.Count == (previousCount + 1)) ? true : false; 
        }

        public void DeleteTag(string tagName)
        {
            TagsCollection.Remove(tagName);
        }
    }
}
