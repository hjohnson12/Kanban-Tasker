using KanbanBoardUWP.Base;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanBoardUWP.ViewModel
{
    public class MainViewModel : Observable
    {
       
        public KanbanModel Task = new KanbanModel();
        private KanbanModel _originalCardModel;
        private KanbanModel _cardModel;
        private ObservableCollection<string> _tagsCollection;
        private ObservableCollection<KanbanModel> _tasks;
        private List<string> _categories;
        private List<string> _colorKeys;
        private string _selectedCategory;

        public MainViewModel()
        {
            Tasks = DataAccess.GetData();
        }

        public ObservableCollection<KanbanModel> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
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
        public List<string> ColorKeys
        {
            get { return _colorKeys; }
            set
            {
                _colorKeys = value;
                OnPropertyChanged();
            }
        }

        public void NewTaskHelper(List<string> categories, List<string> colorKeys)
        {
            CardModel = null; // Null card for new task
            Categories = categories;
            ColorKeys = colorKeys;
        }

        public void EditTaskHelper(KanbanModel selectedModel, List<string> categories, List<string> colorKeys, ObservableCollection<string> tags)
        {
            // Get content ready to show in splitview pane
            OriginalCardModel = selectedModel;
            CardModel = selectedModel;
            Categories = categories;
            ColorKeys = colorKeys;
            TagsCollection = tags;
        }

        public void AddTagToCollection(string tag)
        {
            TagsCollection.Add(tag);
        }

        public void SaveTask(string tags, object selectedCategory, object selectedColorKey)
        {
            // Tags are stroed as string[] in KanbanModel
            // Strip string into a string[]
            string[] tagsArray;
            if (tags == "")
                tagsArray = new string[] { };
            else
                tagsArray = tags.Split(",");


            // Create model
            var newModel = new KanbanModel
            {
                ID = ID,
                Title = Title,
                Description = Description,
                Category = selectedCategory,
                ColorKey = selectedColorKey,
                Tags = tagsArray
            };

            // Update item in collection
            var found = Tasks.FirstOrDefault(x => x.ID == ID);
            int i = Tasks.IndexOf(found);
            Tasks[i] = newModel;

            // Update item in database
            DataAccess.UpdateTask(ID, Title,
                Description, Category.ToString(),
                ColorKey.ToString(), tags);
        }

        public void DeleteTask(KanbanModel model)
        {
            Tasks.Remove(model);
        }

        public void AddTask(string tags, object selectedCategory, object selectedColorKey)
        {
            // Tags are stored as as string[] in KanbanModel
            // Strip string into a sting[]
            string[] tagsArray = new string[] { };
            if (tags != null) 
                tagsArray = tags.Split(',');
            else
                tags = ""; // No tags

            // Create model and add to Tasks collection
            var model = new KanbanModel
            {
                ID = ID,
                Title = Title,
                Description = Description,
                Category = selectedCategory,
                ColorKey = selectedColorKey,
                Tags = tagsArray
            };
            Tasks.Add(model);

            // Add task to database
            DataAccess.AddTask(Title,
                Description, selectedCategory.ToString(),
                selectedColorKey.ToString(), tags);
        }

        public KanbanModel OriginalCardModel
        {
            get;
            set;
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

        public KanbanModel CardModel
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

        public object Category
        {
            get {
                return Task.Category;
            }
            set
            {
                Task.Category = value;
                OnPropertyChanged("Category");
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

        public string[] Tags
        {
            get { return Task.Tags;  }
            set
            {
                Task.Tags = value;
                OnPropertyChanged();
            }
        }

    }
}
