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
        public ObservableCollection<KanbanModel> Tasks { get; }
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> ColorKeys { get; set; }
        public KanbanModel Model = new KanbanModel();
        private KanbanModel _selectedCard;
        private ObservableCollection<string> _taskTags;

        public MainViewModel()
        {
            Tasks = DataAccess.GetData();
        }

        public void EditTaskHelper(KanbanModel selectedModel, ObservableCollection<string> categories, ObservableCollection<string> colorKeys, ObservableCollection<string> tags)
        {
            SelectedCard = selectedModel;
            Categories = categories;
            ColorKeys = colorKeys;
            TaskTags = tags;

            // Store tags as a single string using csv format
            // When calling GetData(), the string will be parsed into separate tags and stored into the list view
            //List<string> tagsList = new List<string>();
            //foreach (var tag in lstViewTags.Items)
            //    tagsList.Add(tag.ToString());
            //var tags = string.Join(',', tagsList); // Convert to a csv string to store in database cell
        }

        public void AddTagToCollection(string tag)
        {
            TaskTags.Add(tag);
        }

        public ObservableCollection<string> TaskTags
        {
            get { return _taskTags; }
            set
            {
                _taskTags = value;
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
                ID = _selectedCard.ID;
                Title = _selectedCard.Title;
                Description = _selectedCard.Description;
                Category = _selectedCard.Category.ToString();
                ColorKey = _selectedCard.ColorKey.ToString();
                Tags = _selectedCard.Tags;
                OnPropertyChanged();
            }
        }

        public string ID
        {
            get { return Model.ID; }
            set
            {
                Model.ID = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return Model.Title; }
            set
            {
                Model.Title = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return Model.Description; }
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
            get { return Model.Tags; }
            set
            {
                Model.Tags = value;
                OnPropertyChanged();
            }
        }

    }
}
