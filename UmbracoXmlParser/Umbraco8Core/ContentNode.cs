using System;
using System.Collections.Generic;

namespace RecursiveMethod.UmbracoXmlParser.Umbraco8Core
{
    /// <summary>
    /// Represents a content "node" ie a pair of draft + published versions 
    /// </summary>
    public class ContentNode
    {
        // everything that is common to both draft and published versions
        // keep this as small as possible
        public readonly int Id;
        public readonly Guid Uid;
        public readonly int Level;
        public readonly string Path;
        public readonly int SortOrder;
        public readonly int ParentContentId;
        public List<int> ChildContentIds;
        public readonly DateTime CreateDate;
        public readonly int CreatorId;

        public ContentData DraftData { get; set; }
        public ContentData PublishedData { get; set; }

        // special ctor with no content data - for members
        public ContentNode(int id, Guid uid, int level, string path, int sortOrder, int parentContentId, DateTime createDate, int creatorId)
        {
            Id = id;
            Uid = uid;
            Level = level;
            Path = path;
            SortOrder = sortOrder;
            ParentContentId = parentContentId;
            CreateDate = createDate;
            CreatorId = creatorId;
            ChildContentIds = new List<int>();
        }

        // two-phase ctor, phase 2
        public void SetContentTypeAndData(ContentData draftData, ContentData publishedData)
        {
            if (draftData == null && publishedData == null)
            {
                throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");
            }

            DraftData = draftData;
            PublishedData = publishedData;
        }
    }
}
