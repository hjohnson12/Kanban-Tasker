using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanTasker.Model
{
    public interface ITagServices
    {
        RowOpResult<Tag> SaveTag(Tag tag);
        RowOpResult DeleteTag(int id);
        List<Tag> GetTags();
        RowOpResult<Tag> ValidateTag(RowOpResult<Tag> result);
    }
}
