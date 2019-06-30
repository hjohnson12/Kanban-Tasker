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
        public ObservableCollection<KanbanModel> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> ColorKeys { get; set; }
        public KanbanModel Model = new KanbanModel();
        private KanbanModel _originalSelectedCard;
        private KanbanModel _selectedCard;
        private ObservableCollection<string> _tagsCollection;
        private ObservableCollection<KanbanModel> _tasks;

        public MainViewModel()
        {
            Tasks = DataAccess.GetData();
        }

        public void EditTaskHelper(KanbanModel selectedModel, ObservableCollection<string> categories, ObservableCollection<string> colorKeys, ObservableCollection<string> tags)
        {
            // Get content ready to show in splitview pane
            OriginalSelectedCard = selectedModel;
            SelectedCard = selectedModel;
            Categories = categories;
            ColorKeys = colorKeys;
            TagsCollection = tags;

            // Store tags as a single string using csv format
            // When calling GetData(), the string will be parsed into separate tags and stored into the list view
            //List<string> tagsList = new List<string>();
            //foreach (var tag in lstViewTags.Items)
            //    tagsList.Add(tag.ToString());
            //var tags = string.Join(',', tagsList); // Convert to a csv string to store in database cell
        }

        public void AddTagToCollection(string tag)
        {
            TagsCollection.Add(tag);
        }

        public void NewTaskHelper()
        {
            SelectedCard = null;
            // Create null items 
            //ID = null;
            //Title = null;
            //Description = null;
            //Category = null;
            //ColorKey = null;
            //Tags = null;

            // Try? 
            //_selectedCard = null;
        }


        public void SaveTask(string tags)
        {
            var tagsArray = tags.Split(',');
            var newModel = new KanbanModel();
            newModel.ID = ID;
            newModel.Title = Title;
            newModel.Description = Description;
            newModel.Category = Category;
            newModel.ColorKey = ColorKey;
            newModel.Tags = tagsArray;

            // DEBUG ISSUE -- Deletes item
            var found = Tasks.FirstOrDefault(x => x.ID == ID);
            int i = Tasks.IndexOf(found);
            Tasks[i] = newModel;

            // Update item in database
            //DataAccess.UpdateTask(ID, Title,
            //    Description, "Open",
            //    "Low", tags);
        }

        public void AddTask(string tags)
        {
            var tagsArray = tags.Split(',');
            var newModel = new KanbanModel();
            newModel.ID = ID;
            newModel.Title = Title;
            newModel.Description = Description;
            newModel.Category = "Open";
            newModel.ColorKey = "Low";
            newModel.Tags = tagsArray;
            Tasks.Add(newModel);

            var categ = (string)Category;
            var colorKey = (string)ColorKey;
            // Add task to database
            DataAccess.AddTask(Title,
                Description, "Open",
                "Low", tags);
        }

        public KanbanModel OriginalSelectedCard
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

        public KanbanModel SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                _selectedCard = value;

                // Update Task Properties to Selected Cards
                if (_selectedCard == null)
                {
                    ID = null;
                    Title = null;
                    Description = null;
                    Category = null;
                    ColorKey = null;
                    Tags = null;
                    TagsCollection = new ObservableCollection<string>();
                    OnPropertyChanged();
                }
                else
                {
                    ID = _selectedCard.ID;
                    Title = _selectedCard.Title;
                    Description = _selectedCard.Description;
                    Category = _selectedCard.Category.ToString();
                    ColorKey = _selectedCard.ColorKey.ToString();
                    Tags = _selectedCard.Tags;
                    OnPropertyChanged();
                }
            }
        }

        public string ID
        {
            get
            {
                if (Model.ID == null)
                    return "";
                else
                    return Model.ID;
            }
            set
            {
                Model.ID = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                if (Model.Title == null)
                    return "";
                else
                    return Model.Title;
            }
            set
            {
                Model.Title = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get
            {
                if (Model.Description == null)
                    return "";
                else
                    return Model.Description;
            }
            set
            {
                Model.Description = value;
                OnPropertyChanged();
            }
        }

        public object Category
        {
            get {
                return Model.Category;
            }
            set
            {
                Model.Category = value;
                OnPropertyChanged();
            }
        }

        public object ColorKey
        {
            get
            {
                return Model.ColorKey;
            }
            set
            {
                Model.ColorKey = value;
                OnPropertyChanged();
            }
        }

        public string[] Tags
        {
            get
            {
                //if (Model.Tags == null)
                //    return;
                //else
                //    return Model.Tags;
                return Model.Tags;
            }
            set
            {
                Model.Tags = value;
                OnPropertyChanged();
            }
        }

    }
}
