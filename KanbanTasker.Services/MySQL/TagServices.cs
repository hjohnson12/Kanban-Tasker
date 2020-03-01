using KanbanTasker.Model;
using KanbanTasker.Services.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Services.MySQL
{
    public class TagServices : MSSQL.TagServices, ITagServices
    {
        public TagServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest){}
        
        public RowOpResult DeleteTag(int id) => base.DeleteTag(id);

        public List<Tag> GetTags() => base.GetTags();

        public RowOpResult<Tag> SaveTag(Tag tag) => base.SaveTag(tag);
    }
}
