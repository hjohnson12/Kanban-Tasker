using KanbanTasker.Base;
using KanbanTasker.DataAccess;
using KanbanTasker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KanbanTasker.ViewModels
{
    public class BoardViewModel : Observable
    {
        /// <summary>
        /// Variables/Private backing fields
        /// </summary>
        public CustomKanbanModel Task = new CustomKanbanModel();
        private CustomKanbanModel _cardModel;
        private ObservableCollection<string> _tagsCollection;
        private ObservableCollection<CustomKanbanModel> _tasks;
        private List<string> _categories;
        private List<string> _colorKeys;
        private string _paneTitle;
        private string _boardName;
        private string _boardNotes;
        private bool _isPointerEntered = false;
        private bool _isEditingTask;
        private string _currentCategory;

        /// <summary>
        /// Constructor / Initialization of tasks
        /// </summary>
        public BoardViewModel()
        {
            Tasks = new ObservableCollection<CustomKanbanModel>();
        }

        #region Properties

        public ObservableCollection<CustomKanbanModel> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Used as the selected card model. If null, initialize a new task; 
        /// otherwise, initialize properties to edit task in pane 
        /// </summary>
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
                    DateCreated = null;
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
                    DateCreated = _cardModel.DateCreated;
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

        /// <summary>
        /// Used to fill indicator key combo box
        /// </summary>
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

        /// <summary>
        /// Fills the tags list view
        /// </summary>
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

        public string DateCreated
        {
            get
            {
                if (Task.DateCreated == null)
                    return "";
                else
                    return Task.DateCreated;
            }
            set
            {
                Task.DateCreated = value;
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

        /// <summary>
        /// Used to determine if pointer entered
        /// inside of a card
        /// </summary>
        public bool IsPointerEntered
        {
            get { return _isPointerEntered; }
            set
            {
                _isPointerEntered = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Used to display Edit/New text on splitview pane
        /// </summary>
        public bool IsEditingTask
        {
            get { return _isEditingTask; }
            set { _isEditingTask = value; OnPropertyChanged(); }
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

        /// <summary>
        /// The category to be displayed on the edit/new task pane
        /// </summary>
        public string CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                _currentCategory = value;
                OnPropertyChanged();
            }
        }

        public CustomKanbanModel OriginalCardModel
        {
            get;
            set;
        }

        #endregion Properties


        #region Functions

        public void AddTagToCollection(string tag)
        {
            TagsCollection.Add(tag);
        }

        /// <summary>
        /// Initializes properties to show information in the edit task pane
        /// </summary>
        /// <param name="selectedModel">Assigned to CardModel to set its properties</param>
        /// <param name="colorKeys"></param>
        /// <param name="tags"></param>
        public void EditTaskHelper(CustomKanbanModel selectedModel, List<string> colorKeys, ObservableCollection<string> tags)
        {
            OriginalCardModel = selectedModel;
            IsEditingTask = true;
            CardModel = selectedModel;
            ColorKeys = colorKeys;
            TagsCollection = tags;
            PaneTitle = "Edit Task";
        }

        /// <summary>
        /// Initializes properties to show information in the new task pane.
        /// CardModel is null for new task.
        /// </summary>
        /// <param name="currentCategory"></param>
        /// <param name="colorKeys"></param>
        public void NewTaskHelper(string currentCategory, List<string> colorKeys)
        {
            CardModel = null; // Null card for new task
            IsEditingTask = false;
            CurrentCategory = currentCategory;
            ColorKeys = colorKeys;
            PaneTitle = "New Task";
        }

        /// <summary>
        /// Updates task data and then updates database entry
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="selectedCategory"></param>
        /// <param name="selectedColorKey"></param>
        /// <param name="selectedCard"></param>
        /// <returns>If updating was successful</returns>
        public bool SaveTask(string tags, object selectedCategory, object selectedColorKey, CustomKanbanModel selectedCard)
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
            selectedModel.ColumnIndex = "";
            selectedModel.Title = Title;
            selectedModel.Description = Description;
            selectedModel.Category = selectedCategory;
            selectedModel.ColorKey = selectedColorKey;
            selectedModel.Tags = tagsArray;

            return DataProvider.UpdateTask(ID, Title,
                Description, selectedCategory.ToString(),
                selectedColorKey.ToString(), tags);
        }

        /// <summary>
        /// Removes task from collection and from the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns>If deletion was successful</returns>
        public bool DeleteTask(CustomKanbanModel model)
        {
            Tasks.Remove(model);
            var deleteSuccess = DataProvider.DeleteTask(model.ID);
            CardModel = null;

            return deleteSuccess;
        }

        /// <summary>
        /// Creates model and adds it to the database. 
        /// Returns the success flag and the new tasks ID
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="selectedCategory"></param>
        /// <param name="selectedColorKey"></param>
        /// <returns>Tuple of values; one for success, other for the new tasks id</returns>
        public (bool, int) AddTask(string tags, object selectedCategory, object selectedColorKey)
        {
            // Tags are stored as as string[] in CustomKanbanModel
            // Strip string into a sting[]
            string[] tagsArray = new string[] { };
            if (tags != null)
                tagsArray = tags.Split(',');
            else
                tags = ""; // No tags

            var boardId = this.BoardId.ToString();

            var currentDateTime = DateTimeOffset.Now.ToString();

            // Create model, set it's ID after made in database
            var model = new CustomKanbanModel
            {
                BoardId = boardId,
                DateCreated = currentDateTime,
                Title = Title,
                Description = Description,
                Category = selectedCategory,
                ColorKey = selectedColorKey,
                Tags = tagsArray
            };

            // Returns a tuple (bool addSuccess, int id) for success flag and 
            // the new tasks ID for the model
            var returnedTuple = DataProvider.AddTask(boardId, currentDateTime, Title,
                Description, selectedCategory.ToString(),
                selectedColorKey.ToString(), tags);
            int newTaskID = returnedTuple.Item1;
            var success = returnedTuple.Item2;

            model.ID = newTaskID.ToString();
            Tasks.Add(model);
            return (success, newTaskID);
        }

        /// <summary>
        /// Removes tag from collection
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>If deletion was successful</returns>
        public bool DeleteTag(string tagName)
        {
            var originalCount = TagsCollection.Count;
            TagsCollection.Remove(tagName);
            return (TagsCollection.Count == (originalCount - 1)) ? true : false;
        }

        /// <summary>
        /// Updates the selected card category and column index after dragging it to
        /// a new column
        /// </summary>
        /// <param name="targetCategory"></param>
        /// <param name="selectedCardModel"></param>
        /// <param name="targetIndex"></param>
        public void UpdateCardColumn(string targetCategory, CustomKanbanModel selectedCardModel, string targetIndex)
        {
            DataProvider.UpdateColumnData(selectedCardModel, targetCategory, targetIndex);
        }

        /// <summary>
        /// Updates a specific card index in the database when reordering after dragging a card
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="currentCardIndex"></param>
        internal void UpdateCardIndex(string iD, int currentCardIndex)
        {
            DataProvider.UpdateCardIndex(iD, currentCardIndex);
        }
        #endregion
    }
}
