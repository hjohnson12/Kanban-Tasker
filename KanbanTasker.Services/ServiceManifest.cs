using System;
using System.Collections.Generic;
using System.Text;
using LeaderAnalytics.AdaptiveClient.Utilities;
using KanbanTasker.Model;
using KanbanTasker.Model.Services;

namespace KanbanTasker.Services
{
    public class ServiceManifest : ServiceManifestFactory, IServiceManifest
    {
        public ITaskServices TaskServices => Create<ITaskServices>();
        public IBoardServices BoardServices => Create<IBoardServices>();
        public IDatabaseServices DatabaseServices => Create<IDatabaseServices>();
    }
}
