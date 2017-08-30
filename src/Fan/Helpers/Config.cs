using AutoMapper;
using Fan.Models;

namespace Fan.Helpers
{
    public static class Config
    {
        /// <summary>
        /// Returns automapper mapping.
        /// </summary>
        /// <remarks>
        /// https://github.com/AutoMapper/AutoMapper/issues/1441
        /// </remarks>
        public static IMapper Mapper
        {
            get
            {
                return new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Post, BlogPost>();
                    cfg.CreateMap<BlogPost, Post>();
                    cfg.CreateMap<Post, Page>();
                    cfg.CreateMap<Page, Post>();
                    cfg.CreateMap<Post, Media>();
                    cfg.CreateMap<Media, Post>();
                }).CreateMapper();
            }
        }
    }
}
