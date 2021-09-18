using KanbanTasker.Base;
using KanbanTasker.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KanbanTasker.Models
{
    /// <summary>
    ///  A presentation model of a ColumnDTO, which
    ///  represents a column within a board.
    /// </summary>
    public class PresentationBoardColumn : Observable
    {
        private int _id;
        private int _boardId;
        private string _columName;
        private int _position;
        private int _maxTaskLimit;

        public PresentationBoardColumn(ColumnDTO columnDto)
        {
            Id = columnDto.Id;
            BoardId = columnDto.BoardId;
            ColumnName = columnDto.ColumnName;
            Position = columnDto.Position;
            MaxTaskLimit = columnDto.MaxTaskLimit;
        }

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int BoardId
        {
            get => _boardId;
            set => SetProperty(ref _boardId, value);
        }

        public string ColumnName
        {
            get => _columName;
            set => SetProperty(ref _columName, value);
        }

        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public int MaxTaskLimit
        {
            get => _maxTaskLimit;
            set => SetProperty(ref _maxTaskLimit, value);
        }

        public ColumnDTO To_ColumnDTO()
        {
            return new ColumnDTO
            {
                Id = Id,
                BoardId = BoardId,
                ColumnName = ColumnName,
                Position = Position,
                MaxTaskLimit = MaxTaskLimit
            };
        }
    }
}
