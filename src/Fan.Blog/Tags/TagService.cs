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
    public class TagService : ITagService, 
                              INotificationHandler<BlogPostBeforeCreate>,
                              INotificationHandler<BlogPostBeforeUpdate>
    {
        public const int TAG_TITLE_MAXLEN = 24;

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

        /// <summary>
        /// Returns tag by id, throws <see cref="FanException"/> if tag with id is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Tag> GetTagAsync(int id)
        {
            var tags = await GetTagsAsync();
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
        public async Task<Tag> GetTagBySlugAsync(string slug)
        {
            if (slug.IsNullOrEmpty()) throw new FanException("Tag does not exist.");

            var tags = await GetTagsAsync();
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
        public async Task<Tag> GetTagByTitleAsync(string title)
        {
            if (title.IsNullOrEmpty()) throw new FanException("Tag does not exist.");

            var tags = await GetTagsAsync();
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
        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_ALL_TAGS, BlogCache.Time_AllTags, async () => {
                return await _tagRepo.GetListAsync();
            });
        }

        /// <summary>
        /// Creates a <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">The tag with data to be created.</param>
        /// <exception cref="FanException">If title fails validation.</exception>
        /// <remarks>
        /// It validates tag title, generates unique slug, cleans description, assigns count
        /// and finally invalidates cache for all tags and index posts.
        /// </remarks>
        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            if (tag == null || tag.Title.IsNullOrEmpty()) return tag;

            // prep title
            tag.Title = PrepareTitle(tag.Title);

            // make sure it is unique
            var allTags = await GetTagsAsync();
            if (allTags.Any(t => t.Title.Equals(tag.Title, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FanException($"'{tag.Title}' already exists.");
            }

            // prep slug, description and count
            tag.Slug = BlogUtil.FormatTaxonomySlug(tag.Title, allTags.Select(c => c.Slug)); // slug is based on title
            tag.Description = Util.CleanHtml(tag.Description);
            tag.Count = tag.Count;

            // before create 
            await _mediator.Publish(new TagBeforeCreate());

            // create
            tag = await _tagRepo.CreateAsync(tag);

            // remove cache
            await _cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            // after create
            await _mediator.Publish(new TagCreated());

            _logger.LogDebug("Created {@Tag}", tag);
            return tag;
        }

        /// <summary>
        /// Updates an existing <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">The tag with data to be updated.</param>
        /// <exception cref="FanException">If title fails validation.</exception>
        /// <remarks>
        /// It validates tag title, generates unique slug, cleans description, assigns count
        /// and finally invalidates cache for all tags and index posts.
        /// </remarks>
        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            if (tag == null || tag.Id <= 0 || tag.Title.IsNullOrEmpty()) return tag;

            // prep title
            tag.Title = PrepareTitle(tag.Title);

            // make sure it is unique
            var allTags = await GetTagsAsync();
            allTags.RemoveAll(t => t.Id == tag.Id); // remove selft
            if (allTags.Any(t => t.Title.Equals(tag.Title, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FanException($"'{tag.Title}' already exists.");
            }

            // prep slug, description and count
            var tagEntity = await _tagRepo.GetAsync(tag.Id);
            tagEntity.Title = tag.Title; // assign new title
            tagEntity.Slug = BlogUtil.FormatTaxonomySlug(tag.Title, allTags.Select(c => c.Slug)); // slug is based on title
            tagEntity.Description = Util.CleanHtml(tag.Description);
            tagEntity.Count = tag.Count;

            // before update 
            await _mediator.Publish(new TagBeforeUpdate());

            // update 
            await _tagRepo.UpdateAsync(tagEntity);

            // remove cache
            await _cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            // after update
            await _mediator.Publish(new TagUpdated());

            _logger.LogDebug("Updated {@Tag}", tagEntity);
            return tag;
        }

        /// <summary>
        /// Deletes a <see cref="Tag"/> by id and invalidates cache for all tags.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteTagAsync(int id)
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
            var allTags = await GetTagsAsync(); 

            // create any new tags
            foreach (var title in distinctTitles)
            {
                // make sure the incoming title does not already exist
                var tag = allTags.FirstOrDefault(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                if (tag == null)
                {
                    tag = await CreateTagAsync(new Tag { Title = title });
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
            var allTags = await GetTagsAsync();

            // create any new tags
            foreach (var title in distinctTitles)
            {
                // make sure the incoming title does not already exist
                var tag = allTags.FirstOrDefault(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                if (tag == null)
                {
                    tag = await CreateTagAsync(new Tag { Title = title });
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
            title = title.Length > TAG_TITLE_MAXLEN ? title.Substring(0, TAG_TITLE_MAXLEN) : title;
            return title;
        }
    }
}
