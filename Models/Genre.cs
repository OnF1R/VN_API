﻿namespace VN_API.Models
{
    public class Genre
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual List<VisualNovel> VisualNovels { get; set; } = new();
    }
}
