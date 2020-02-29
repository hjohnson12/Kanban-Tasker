using KanbanTasker.Model;
using KanbanTasker.Services.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Services.MySQL
{
    public class TagServices : BaseService, ITagServices
    {
        public TagServices (Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }
        public RowOpResult DeleteTag(int id)
        {
            throw new NotImplementedException();
        }

        public List<Tag> GetTags()
        {
            throw new NotImplementedException();
        }

        public RowOpResult<Tag> SaveTag(Tag tag)
        {
            throw new NotImplementedException();
        }

        public RowOpResult<Tag> ValidateTag(RowOpResult<Tag> result)
        {
            throw new NotImplementedException();
        }
    }
}
