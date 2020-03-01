using KanbanTasker.Model;
using KanbanTasker.Services.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanbanTasker.Services.MSSQL
{
    public class TagServices : BaseService, ITagServices
    {
        public TagServices(Db db, IServiceManifest serviceManifest) : base(db, serviceManifest)
        {

        }
        public RowOpResult DeleteTag(int id)
        {
            RowOpResult result = new RowOpResult();
            Tag tag = db.Tags.FirstOrDefault(x => x.ID == id);

            if (tag == null)
            {
                result.ErrorMessage = $"tagID {id} is invalid. Tag may have already been deleted.";
                return result;
            }

            db.Entry(tag).State = EntityState.Deleted;
            db.SaveChanges();
            result.Success = true;
            return result;
        }

        public List<Tag> GetTags() => db.Tags.ToList();

        public RowOpResult<Tag> SaveTag(Tag tag)
        {
            if (tag.TagName == null)
                throw new NotImplementedException(nameof(tag.TagName));

            RowOpResult<Tag> result = new RowOpResult<Tag>(tag);

            ValidateTag(result);

            if (!result.Success)
                return result;

            db.Entry(tag).State = tag.ID == 0 ? EntityState.Added : EntityState.Modified;
            db.SaveChanges();
            result.Success = true;
            return result;
        }
    }
}
