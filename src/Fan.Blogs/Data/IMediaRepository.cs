using Fan.Blogs.Models;
using System;
using System.Threading.Tasks;

namespace Fan.Blogs.Data
{
    public interface IMediaRepository
    {
        Task<Media> CreateAsync(Media media);
        Task<Media> GetAsync(int mediaId);
        Task<Media> GetAsync(string fileName, DateTimeOffset uploadedOn);
        Task<Media> UpdateAsync(Media media);
    }
}
