using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    /// <summary>
    /// A manifest, or list, of services availble to Kanban to allow convenient
    /// injection of multiple services and to call one service from within another.
    /// </summary>
    public interface IServiceManifest
    {
        ITaskServices TaskServices { get; }
        IBoardServices BoardServices { get; }
        IDatabaseServices DatabaseServices { get; }
    }
}
