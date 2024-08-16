namespace VN_API.Models
{
    public class VisualNovelListType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMutuallyExclusive { get; set; }
    }

    public class VisualNovelList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsCustom { get; set; }
        public bool IsPrivate { get; set; }
        public string UserId { get; set; }
        public VisualNovelListType? ListType { get; set; }
        public virtual ICollection<VisualNovelListEntry> VisualNovelListEntries { get; set; }
    }

    public class VisualNovelListEntry
    {
        public int Id { get; set; }
        public VisualNovelList VisualNovelList { get; set; }
        public VisualNovel VisualNovel { get; set; }
        public DateTime AddingTime { get; set; }
    }

    //public class UserLabels
    //{
    //    public int Id { get; set; }
    //    public int UserId { get; set; }
    //    public string Label { get; set; }
    //    public bool IsPrivate { get; set; }
    //}

    //public class UserList
    //{
    //    public UserList()
    //    {
    //        Labels = new List<UserLabels>
    //        {
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Читаю", IsPrivate = false,
    //            },
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Перечитываю", IsPrivate = false,
    //            },
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Прочитано", IsPrivate = false,
    //            },
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Брошено", IsPrivate = false,
    //            },
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Желаемое ", IsPrivate = false,
    //            },
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Любимое", IsPrivate = false,
    //            },
    //            new UserLabels
    //            {
    //                Id = Guid.NewGuid(), UserId = UserId, Label = "Чёрный список", IsPrivate = false,
    //            },
    //        };
    //    }

    //    public Guid Id { get; set; }
    //    public int UserId { get; set; }
    //    public int VisualNovelId { get; set; }
    //    public DateTime AddedOn { get; set; }
    //    public DateTime LastModified { get; set; }
    //    public virtual List<UserLabels> Labels { get; set; }
    //}
}
