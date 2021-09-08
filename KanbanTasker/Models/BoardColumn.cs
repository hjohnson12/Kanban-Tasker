using KanbanTasker.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Models
{
    public class BoardColumn : Observable
    {
        private string _columnName;
        private int _maxTaskLimit;

        public string ColumnName
        {
            get => _columnName;
            set
            {
                _columnName = value;
                OnPropertyChanged();
            }
        }

        public int MaxTaskLimit
        {
            get => _maxTaskLimit;
            set => SetProperty(ref _maxTaskLimit, value);
        }
    }
}
