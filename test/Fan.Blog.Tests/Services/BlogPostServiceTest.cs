using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Events;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Blog.Tests.Helpers;
using Fan.Exceptions;
using Fan.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.Tests.Services
{
    public class BlogPostServiceTest
    {
        private readonly BlogPostService blogPostService;
        private readonly Mock<IPostRepository> postRepoMock = new Mock<IPostRepository>();
        private readonly Mock<IMediator> mediatorMock = new Mock<IMediator>();
        private readonly CancellationToken cancellationToken = new CancellationToken();

        public BlogPostServiceTest()
        {
            // cache, logger, mapper
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var cache = new MemoryDistributedCache(serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>());
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<BlogPostService>();
            var mapper = BlogUtil.Mapper;

            // settings
            var settingSvcMock = new Mock<ISettingService>();
            settingSvcMock.Setup(svc => svc.GetSettingsAsync<CoreSettings>()).Returns(Task.FromResult(new CoreSettings()));
            settingSvcMock.Setup(svc => svc.GetSettingsAsync<BlogSettings>()).Returns(Task.FromResult(new BlogSettings()));

            // image service
            var imgSvcMock = new Mock<IImageService>();

            // service
            blogPostService = new BlogPostService(settingSvcMock.Object, 
                imgSvcMock.Object, 
                postRepoMock.Object, 
                cache, logger, mapper, mediatorMock.Object);
        }


        /// <summary>
        /// When an author publishes a blog post from OLW.
        /// </summary>
        [Fact]
        public async void CreateAsync_BlogPost_from_OLW()
        {
            // Arrange: 
            // since the final step of CreateAsync is to call repo.GetAsync,
            // arrange a post to return
            postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), EPostType.BlogPost))
                .Returns(Task.FromResult(new Post { CreatedOn = DateTimeOffset.Now }));

            // Act: 
            // user creates a new blog post from OLW, this post omits values that are
            // typically missed when posting from OLW
            var blogPostCreated = await blogPostService.CreateAsync(new BlogPost 
            {
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = null,                        // user didn't input
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryTitle = null,               // coming from olw CatTitle is used instead of CatId
                TagTitles = null,                   // user didn't input
                CreatedOn = new DateTimeOffset(),   // user didn't input, it's MinValue
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Assert:
            // mediatr publishes null cat title
            mediatorMock.Verify(m => m.Publish(
                It.Is<BlogPostBeforeCreate>(e => e.CategoryTitle == null
                                              && e.TagTitles == null
                ), cancellationToken), Times.Once);

            // repo.CreateAsync is called once with the following conditions
            postRepoMock.Verify(repo => repo.CreateAsync(
                It.Is<Post>(p => p.Slug == "hello-world" // Slug is generated
                              && p.CreatedOn != new DateTimeOffset() // CreatedOn is set to now instead of min value
                              && p.Excerpt == null // Excerpt is not generated on user's behalf
                           ), null, null), Times.Once);

            // the date displays human friend string
            var coreSettings = new CoreSettings();
            Assert.Equal("now", blogPostCreated.CreatedOn.ToDisplayString(coreSettings.TimeZoneId));
        }

        /// <summary>
        /// When an author publishes a blog post from browser.
        /// Admin_publishes_BlogPost_from_browser
        /// </summary>
        [Fact]
        public async void CreateAsync_BlogPost_from_browser()
        {
            // Arrange
            // since the final step of CreateAsync is to call repo.GetAsync,
            // make it return a non-null post
            postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), EPostType.BlogPost))
                .Returns(Task.FromResult(new Post()));

            // Act
            // user creates a new blog post from browser
            // notice CategoryId is typically set when posting from browser
            var tagTitles = new List<string> { "test", "c#" };
            await blogPostService.CreateAsync(new BlogPost // from browser
            {
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = "hello-world-from-browser",  // user can input this value
                Body = "This is my first post",
                Excerpt = null,                     // user didn't input
                CategoryId = 1,                     // coming from browser CatId is filled
                TagTitles = tagTitles,
                CreatedOn = DateTimeOffset.Now,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Assert
            // mediatr publishes null cat title
            mediatorMock.Verify(m => m.Publish(
                It.Is<BlogPostBeforeCreate>(e => e.CategoryTitle == null
                                              && e.TagTitles == tagTitles
                ), cancellationToken), Times.Once);

            // repo.CreateAsync is called once with the following conditions
            postRepoMock.Verify(repo => repo.CreateAsync(
                It.Is<Post>(p => p.Slug == "hello-world-from-browser" 
                              && p.CreatedOn != new DateTimeOffset() 
                              && p.Excerpt == null 
                              && p.Category == null),  
                null, tagTitles), Times.Once);
        }

        /// <summary>
        /// When author saves a draft with empty title, it's OK.
        /// Admin_Can_Save_Draft_With_Empty_Title_And_Slug
        /// </summary>
        [Fact]
        public async void CreateAsync_draft_BlogPost_with_empty_title_and_slug_is_OK()
        {
            // Arrange
            // since the final step of CreateAsync is to call repo.GetAsync,
            // make it return a non-null post
            postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), EPostType.BlogPost))
                .Returns(Task.FromResult(new Post()));

            // Act
            // user creates a new blog post from browser
            // notice CategoryId is typically set when posting from browser
            var tagTitles = new List<string> { "test", "c#" };
            await blogPostService.CreateAsync(new BlogPost 
            {
                UserId = Actor.ADMIN_ID,
                Title = null,
                Slug = null,  
                Body = "This is my first post",
                Excerpt = null,                     
                CategoryId = 1,                     
                TagTitles = tagTitles,
                CreatedOn = DateTimeOffset.Now,
                Status = EPostStatus.Draft, // draft
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Assert
            // mediatr publishes null cat title
            mediatorMock.Verify(m => m.Publish(
                It.Is<BlogPostBeforeCreate>(e => e.CategoryTitle == null
                                              && e.TagTitles == tagTitles
                ), cancellationToken), Times.Once);

            // repo.CreateAsync is called once with the following conditions
            postRepoMock.Verify(repo => repo.CreateAsync(
                It.Is<Post>(p => p.Slug == null
                              && p.Title == null),
                null, tagTitles), Times.Once);
        }

        /// <summary>
        /// When an author updates a blog post from OLW.
        /// Admin_updates_BlogPost_from_OLW
        /// </summary>
        [Fact]
        public async void UpdateAsync_BlogPost()
        {
            // Arrange
            // since the final step of UpdateAsync is to call repo.GetAsync,
            // make it return a non-null post
            postRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), EPostType.BlogPost))
                .Returns(Task.FromResult(new Post()));

            // Act
            var tagTitles = new List<string> { "test", "c#" };
            await blogPostService.UpdateAsync(new BlogPost 
            {
                Id = 1,
                UserId = Actor.ADMIN_ID,
                Title = "Hello World!",
                Slug = "hello-world-from-browser",  
                Body = "This is my first post",
                Excerpt = null,                     
                CategoryId = 1,                     
                TagTitles = tagTitles,
                CreatedOn = DateTimeOffset.Now,
                Status = EPostStatus.Published,
                CommentStatus = ECommentStatus.AllowComments,
            });

            // Assert
            mediatorMock.Verify(m => m.Publish(
                It.Is<BlogPostBeforeUpdate>(e => e.CategoryTitle == null
                                              && e.TagTitles == tagTitles
                                              && e.PostTags.Count() == 0), cancellationToken), Times.Once);

            postRepoMock.Verify(repo => repo.UpdateAsync(
                It.Is<Post>(p => p.Slug == "hello-world-from-browser"
                              && p.Excerpt == null
                              && p.Category == null),
                null, tagTitles), Times.Once);
        }

        /// <summary>
        /// This test is to make sure the following, <see cref="https://github.com/FanrayMedia/Fanray/issues/88"/>
        /// 
        /// 1. User publishes a new post(at this point, a slug has been created, search engine could have scrawled it)
        /// 2. User goes back to change the post title
        /// 3. Publish the post again will not alter the slug, thus will not break SEO
        /// </summary>
        [Fact]
        public async void Update_post_title_will_not_alter_slug()
        {
            // 1. user writes a post with title
            var title = "A blog post title";
            var dt = DateTimeOffset.Now;
            var postId = 1;
            // Very important to setup return null for Post or it'll go into infinite loop
            postRepoMock.Setup(r => r.GetAsync(It.IsAny<string>(), dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult((Post)null));

            // 2. user publishes the post
            var slug = await blogPostService.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);

            // 3. user goes back to update post title
            // NOTE: at the point the existing slug is being passed in 
            // See BlogService.PrepPostAsync()
            var theSlug = await blogPostService.GetBlogPostSlugAsync(slug, dt, ECreateOrUpdate.Update, postId);

            Assert.Equal(theSlug, slug);
        }

        /// <summary>
        /// This test is to make sure the following, <see cref="https://github.com/FanrayMedia/Fanray/issues/88"/>
        /// 
        /// 1. User publishes a new post(at this point, a slug has been created, search engine could have scrawled it)
        /// 2. User goes back and change the post slug
        /// 3. Publish the post again will be able to update slug
        /// </summary>
        [Fact]
        public async void Update_post_slug_will_alter_slug()
        {
            // 1. user writes a post with title
            var title = "A blog post title";
            var dt = DateTimeOffset.Now;
            var postId = 1;
            // Very important to setup return null for Post or it'll go into infinite loop
            postRepoMock.Setup(r => r.GetAsync(It.IsAny<string>(), dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult((Post)null));

            // 2. user publishes the post
            var slug = await blogPostService.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);

            // Now the user update the post slug
            slug = "i-want-a-different-slug-for-this-post";

            // 3. user goes back to update post title
            // NOTE: at the point the existing slug is being passed in 
            // See BlogService.PrepPostAsync()
            var theSlug = await blogPostService.GetBlogPostSlugAsync(slug, dt, ECreateOrUpdate.Update, postId);

            Assert.Equal(theSlug, slug);
        }

        /// <summary>
        /// When create a post the slug is guaranteed to be unique.
        /// </summary>
        [Theory]
        [InlineData("A blog post title", "a-blog-post-title", "a-blog-post-title-2")]
        [InlineData("A blog post title 2", "a-blog-post-title-2", "a-blog-post-title-3")]
        public async void Create_post_will_always_produce_unique_slug(string title, string slug, string expected)
        {
            // Given an existing post with slug 
            var dt = DateTimeOffset.Now;
            postRepoMock.Setup(r => r.GetAsync(slug, dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult(new Post { Id = 10000, Slug = slug }));

            // When user publishes the post that will conflict with existing slug
            var postId = 1;
            var slugUnique = await blogPostService.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);

            // Then a unique slug is produced
            Assert.Equal(expected, slugUnique);
        }

        /// <summary>
        /// If user manually updates an existing post's slug and it conflicts with an existing post,
        /// my blog will resolve the conflict by producing an unique slug automatically.
        /// </summary>
        [Fact]
        public async void Update_post_will_produce_unique_slug_if_user_updates_slug_to_run_into_conflict()
        {
            // Given an existing post
            var slug = "i-want-a-different-slug-for-this-post";
            var dt = DateTimeOffset.Now;
            postRepoMock.Setup(r => r.GetAsync(slug, dt.Year, dt.Month, dt.Day))
                .Returns(Task.FromResult(new Post { Id = 10000, Slug = slug }));

            // When user publishes the post and update the slug
            var postId = 1;
            var title = "A blog post title";
            var slugCreated = await blogPostService.GetBlogPostSlugAsync(title, dt, ECreateOrUpdate.Create, postId);
            Assert.Equal("a-blog-post-title", slugCreated);

            // Now the user update the post slug
            slugCreated = "i-want-a-different-slug-for-this-post";

            // Then
            var slugUpdated = await blogPostService.GetBlogPostSlugAsync(slug, dt, ECreateOrUpdate.Update, postId);

            Assert.Equal("i-want-a-different-slug-for-this-post-2", slugUpdated);
        }

        // -------------------------------------------------------------------- Post Validation

        /// <summary>
        /// If your post is a draft, then the title can be empty.
        /// </summary>
        [Fact]
        public async void BlogPost_draft_can_have_empty_title()
        {
            // When you have a draft with empty title
            var blogPost = new BlogPost { Title = "", Status = EPostStatus.Draft };

            // Then its validation will not fail
            await blogPost.ValidateTitleAsync();
        }

        /// <summary>
        /// When you publish a blog post the title cannot be empty.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="numberOfErrors"></param>
        /// <param name="expectedMessages"></param>
        [Theory]
        [InlineData(null, EPostStatus.Published, 1, new string[] { "'Title' must not be empty." })]
        [InlineData("", EPostStatus.Published, 1, new string[] { "'Title' must not be empty." })]
        public async void Publish_BlogPost_does_not_allow_empty_title(string title, EPostStatus status, int numberOfErrors, string[] expectedMessages)
        {
            // Given a blog post to publish
            var blogPost = new BlogPost { Title = title, Status = status };

            // When validate it throws FanException
            var ex = await Assert.ThrowsAsync<FanException>(() => blogPost.ValidateTitleAsync());
            Assert.Equal(numberOfErrors, ex.ValidationErrors.Count);
            Assert.Equal(expectedMessages[0], ex.ValidationErrors[0].ErrorMessage);
        }

        /// <summary>
        /// A blog post title cannot exceed 250 characters regardless whether it's published or draft.
        /// </summary>
        [Theory]
        [InlineData(EPostStatus.Draft, new string[] { "The length of 'Title' must be 250 characters or fewer. You entered 251 characters." })]
        [InlineData(EPostStatus.Published, new string[] { "The length of 'Title' must be 250 characters or fewer. You entered 251 characters." })]
        public async void BlogPost_title_cannot_exceed_250_chars_regardless_status(EPostStatus status, string[] expectedMessages)
        {
            // Arrange: a blog post with a title of 251 chars
            var title = string.Join("", Enumerable.Repeat<char>('a', 251));
            var blogPost = new BlogPost { Title = title, Status = status };

            // Act: validate
            var ex = await Assert.ThrowsAsync<FanException>(() => blogPost.ValidateTitleAsync());
            // Assert: 1 error
            Assert.Equal(1, ex.ValidationErrors.Count);
            Assert.Equal(expectedMessages[0], ex.ValidationErrors[0].ErrorMessage);
        }

        /// <summary>
        /// When you pass <see cref="BlogPostService.CreateAsync(BlogPost)"/> a null param
        /// you get <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public async void CreateAsync_throws_ArgumentNullException_if_param_passed_in_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => blogPostService.CreateAsync(null));
        }

        /// <summary>
        /// When you pass <see cref="BlogPostService.UpdateAsync(BlogPost)"/> a null param
        /// you get <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public async void UpdateAsync_throws_ArgumentException_if_param_passed_in_is_null()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => blogPostService.UpdateAsync(null));
        }
    }
}
