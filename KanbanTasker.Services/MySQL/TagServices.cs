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
        
        public override RowOpResult DeleteTag(int id) => base.DeleteTag(id);

        public override List<Tag> GetTags() => base.GetTags();

        public override RowOpResult<Tag> SaveTag(Tag tag) => base.SaveTag(tag);
    }
}
