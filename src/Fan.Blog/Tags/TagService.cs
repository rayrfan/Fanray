using Fan.Blog.Helpers;
using Fan.Blog.Posts;
using Fan.Exceptions;
using Fan.Helpers;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fan.Blog.Tags
{
    /// <summary>
    /// The blog tag service.
    /// </summary>
    public class TagService : ITagService,
                              INotificationHandler<BlogPostBeforeCreate>,
                              INotificationHandler<BlogPostBeforeUpdate>
    {
        private readonly ITagRepository _tagRepo;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TagService> _logger;

        public TagService(ITagRepository tagRepo,
            IMediator mediator,
            IDistributedCache cache,
            ILogger<TagService> logger)
        {
            _tagRepo = tagRepo;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        // -------------------------------------------------------------------- const

        /// <summary>
        /// The max allowed length of a tag title is 24 chars.
        /// </summary>
        public const int TITLE_MAXLEN = 24;

        /// <summary>
        /// The max allowed length of a tag slug is 24 chars.
        /// </summary>
        public const int SLUG_MAXLEN = 24;

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Returns tag by id, throws <see cref="FanException"/> if tag with id is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Tag> GetAsync(int id)
        {
            var tags = await GetAllAsync();
            var tag = tags.SingleOrDefault(c => c.Id == id);
            if (tag == null)
            {
                throw new FanException($"Tag with id {id} is not found.");
            }

            return tag;
        }

        /// <summary>
        /// Returns tag by slug, throws <see cref="FanException"/> if tag with slug is not found.
        /// </summary>
        /// <param name="slug">Tag slug.</param>
        /// <returns></returns>
        public async Task<Tag> GetBySlugAsync(string slug)
        {
            if (slug.IsNullOrEmpty()) throw new FanException("Tag does not exist.");

            var tags = await GetAllAsync();
            var tag = tags.SingleOrDefault(c => c.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            if (tag == null)
            {
                throw new FanException($"Tag with slug '{slug}' does not exist.");
            }

            return tag;
        }

        /// <summary>
        /// Returns tag by title, throws <see cref="FanException"/> if tag with title is not found.
        /// </summary>
        /// <param name="title">Tag title.</param>
        /// <returns></returns>
        public async Task<Tag> GetByTitleAsync(string title)
        {
            if (title.IsNullOrEmpty()) throw new FanException("Tag does not exist.");

            var tags = await GetAllAsync();
            var tag = tags.SingleOrDefault(c => c.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
            if (tag == null)
            {
                throw new FanException($"Tag with title '{title}' does not exist.");
            }

            return tag;
        }

        /// <summary>
        /// Returns all tags, cached after calls to DAL.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method must return all tags as <see cref="PrepPostAsync(BlogPost, ECreateOrUpdate)"/>
        /// depends on entire tags. If any filtering needs to be done for presentation purpose, then
        /// it must be done in presentation layer.
        /// 
        /// TODO: currently create and update post depend on all tags instead for querying each tag 
        /// individually to db each time, this saves some db round trip. however there is fine line
        /// for how large the number of tags grow, in which case we need a better strategy.
        /// </remarks>
        public async Task<List<Tag>> GetAllAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_ALL_TAGS, BlogCache.Time_AllTags, async () => {
                return await _tagRepo.GetListAsync();
            });
        }

        /// <summary>
        /// Creates a new <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">The tag with data to be created.</param>
        /// <exception cref="FanException">If tag is empty or title exists already.</exception>
        /// <returns>Created tag.</returns>
        public async Task<Tag> CreateAsync(Tag tag)
        {
            if (tag == null || tag.Title.IsNullOrEmpty())
            {
                throw new FanException($"Invalid tag to create.");
            }

            // prep title
            tag.Title = PrepareTitle(tag.Title);

            // make sure it is unique
            var allTags = await GetAllAsync();
            if (allTags.Any(t => t.Title.Equals(tag.Title, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FanException($"'{tag.Title}' already exists.");
            }

            // prep slug, description and count
            tag.Slug = BlogUtil.FormatTaxonomySlug(tag.Title, SLUG_MAXLEN, allTags.Select(c => c.Slug)); // slug is based on title
            tag.Description = Util.CleanHtml(tag.Description);
            tag.Count = tag.Count;

            // create
            tag = await _tagRepo.CreateAsync(tag);

            // remove cache
            await _cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            _logger.LogDebug("Created {@Tag}", tag);
            return tag;
        }

        /// <summary>
        /// Updates an existing <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">The tag with data to be updated.</param>
        /// <exception cref="FanException">If tag is invalid or title exists already.</exception>
        /// <returns>Updated tag.</returns>
        public async Task<Tag> UpdateAsync(Tag tag)
        {
            if (tag == null || tag.Id <= 0 || tag.Title.IsNullOrEmpty())
            {
                throw new FanException($"Invalid tag to update.");
            }

            // prep title
            tag.Title = PrepareTitle(tag.Title);

            // make sure it is unique
            var allTags = await GetAllAsync();
            allTags.RemoveAll(t => t.Id == tag.Id); // remove self
            if (allTags.Any(t => t.Title.Equals(tag.Title, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FanException($"'{tag.Title}' already exists.");
            }

            // prep slug, description and count
            var entity = await _tagRepo.GetAsync(tag.Id);
            entity.Title = tag.Title; // assign new title
            entity.Slug = BlogUtil.FormatTaxonomySlug(tag.Title, SLUG_MAXLEN, allTags.Select(c => c.Slug)); // slug is based on title
            entity.Description = Util.CleanHtml(tag.Description);
            entity.Count = tag.Count;

            // update 
            await _tagRepo.UpdateAsync(entity);

            // remove cache
            await _cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            // return entity
            _logger.LogDebug("Updated {@Tag}", entity);
            return entity;
        }

        /// <summary>
        /// Deletes a <see cref="Tag"/> by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            await _tagRepo.DeleteAsync(id);
            await _cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);
        }

        // -------------------------------------------------------------------- event handlers

        /// <summary>
        /// Handles the <see cref="BlogPostBeforeCreate"/> event by creating any new tags.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Handle(BlogPostBeforeCreate notification, CancellationToken cancellationToken)
        {
            if (notification.TagTitles == null || notification.TagTitles.Count <= 0) return;

            // make sure list has no empty strings and only unique values
            var distinctTitles = notification.TagTitles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
            var allTags = await GetAllAsync();

            // create any new tags
            foreach (var title in distinctTitles)
            {
                // make sure the incoming title does not already exist
                var tag = allTags.FirstOrDefault(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                if (tag == null)
                {
                    tag = await CreateAsync(new Tag { Title = title });
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="BlogPostBeforeUpdate"/> event by creating any new tags.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Handle(BlogPostBeforeUpdate notification, CancellationToken cancellationToken)
        {
            if (notification.TagTitles == null || notification.TagTitles.Count <= 0 || notification.CurrentPost == null) return;

            // get tags that are not among current tags
            var currentTitles = notification.CurrentPost.PostTags.Select(pt => pt.Tag.Title);
            var distinctTitles = notification.TagTitles.Except(currentTitles);
            var allTags = await GetAllAsync();

            // create any new tags
            foreach (var title in distinctTitles)
            {
                // make sure the incoming title does not already exist
                var tag = allTags.FirstOrDefault(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                if (tag == null)
                {
                    tag = await CreateAsync(new Tag { Title = title });
                }
            }
        }

        // -------------------------------------------------------------------- private methods

        /// <summary>
        /// Cleans tag title from any html and shortens it if exceed max allow length.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string PrepareTitle(string title)
        {
            title = Util.CleanHtml(title);
            title = title.Length > TITLE_MAXLEN ? title.Substring(0, TITLE_MAXLEN) : title;
            return title;
        }
    }
}
