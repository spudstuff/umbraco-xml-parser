namespace RecursiveMethod.UmbracoXmlParser.Umbraco8Core
{
    public struct ContentNodeKit
    {
        public ContentNode Node;
        public int ContentTypeId;
        public ContentData DraftData;
        public ContentData PublishedData;

        public bool IsEmpty
        {
            get { return Node == null; }
        }

        public bool IsNull
        {
            get
            {
                return ContentTypeId < 0;
            }
        }

        public static ContentNodeKit Null
        {
            get
            {
                return new ContentNodeKit { ContentTypeId = -1 };
            }
        }
    }
}
