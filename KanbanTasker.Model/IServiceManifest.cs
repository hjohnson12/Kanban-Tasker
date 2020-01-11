using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    // This is a manifest, or list, of services available to Kanban.  It allows us to conveniently inject multiple services and also to call one service from within another.

    public interface IServiceManifest
    {
        ITaskServices TaskServices { get; }
        IBoardServices BoardServices { get; }
        IDatabaseServices DatabaseServices { get; }
    }
}
