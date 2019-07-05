using KanbanBoardUWP.Base;
using KanbanBoardUWP.DataAccess;
using KanbanBoardUWP.Models;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanBoardUWP.ViewModels
{
    public class MainViewModel : Observable
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

        //=====================================================================
        // CONSTRUCTOR
        //=====================================================================

        public MainViewModel()
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
                    OnPropertyChanged("CardModel");
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

        public string BoardID
        {
            get
            {
                if (Task.BoardID == null)
                    return "";
                else
                    return Task.BoardID;
            }
            set
            {
                Task.BoardID = value;
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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

        public void DeleteTask(CustomKanbanModel model)
        {
            Tasks.Remove(model);
            DataProvider.DeleteTask(model.ID); // Delete from database
            CardModel = null;
        }

        public void NewTaskHelper(List<string> categories, List<string> colorKeys)
        {
            CardModel = null; // Null card for new task
            Categories = categories;
            ColorKeys = colorKeys;
            PaneTitle = "New Task";
        }

        public void AddTask(string tags, object selectedCategory, object selectedColorKey)
        {
            // Tags are stored as as string[] in CustomKanbanModel
            // Strip string into a sting[]
            string[] tagsArray = new string[] { };
            if (tags != null) 
                tagsArray = tags.Split(',');
            else
                tags = ""; // No tags

            // Start ID at 1; if Task.Count = 0 then it will have an ID of 1, etc.
            int nextId = Tasks.Count + 1;

            // Create model and add to Tasks collection
            var model = new CustomKanbanModel
            {
                ID = nextId.ToString(),
                BoardID = BoardID,
                Title = Title,
                Description = Description,
                Category = selectedCategory,
                ColorKey = selectedColorKey,
                Tags = tagsArray
            };
            Tasks.Add(model);

            // Add task to database
            DataProvider.AddTask(nextId, BoardID, Title,
                Description, selectedCategory.ToString(),
                selectedColorKey.ToString(), tags);
        }
    }
}
