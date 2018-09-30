using Fan.Blog.IntegrationTests.Base;
using Fan.Blog.IntegrationTests.Helpers;
using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fan.Blog.IntegrationTests
{
    /// <summary>
    /// Tests for <see cref="SqlPostRepository"/> class.
    /// </summary>
    public class SqlPostRepositoryTest : BlogIntegrationTestBase
    {
        SqlPostRepository _postRepo;

        public SqlPostRepositoryTest()
        {
            _postRepo = new SqlPostRepository(_db);
        }

        // -------------------------------------------------------------------- GetPost

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetAsync(int, EPostType)"/> will return category
        /// and tags with the returned post if the requested post is BlogPost.
        /// </summary>
        [Fact]
        public async void GetPost_By_Id_Will_Return_Category_And_Tags_For_BlogPost()
        {
            // Arrange: 1 post with 1 cat and 2 tags
            SeedTestPost();

            // Act: get
            var post = await _postRepo.GetAsync(1, EPostType.BlogPost);

            // Assert: cat and tags are there
            Assert.NotNull(post.Category);
            Assert.True(post.PostTags.Count() == 2);
            Assert.True(post.PostTags.ToList()[0].Tag.Id == 1);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetAsync(int, EPostType)"/> will return null
        /// if the specified id is not found.
        /// </summary>
        [Fact]
        public async void GetPost_By_Id_Will_Return_Null_If_Not_Found()
        {
            // Act: getting a post that is not there
            var blogPost = await _postRepo.GetAsync(1, EPostType.BlogPost);

            // Assert
            Assert.Null(blogPost);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetAsync(string, EPostType)"/> will return category
        /// and tags with the returned post if the requested post is BlogPost.
        /// </summary>
        [Fact]
        public async void GetPost_By_Slug_Will_Return_Category_And_Tags_For_BlogPost()
        {
            // Arrange: 1 blog post with 1 cat and 2 tags
            SeedTestPost();

            // Act: get
            var post = await _postRepo.GetAsync(1, EPostType.BlogPost);

            // Assert: cat and tags are there
            Assert.NotNull(post.Category);
            Assert.True(post.PostTags.Count() == 2);
            Assert.True(post.PostTags.ToList()[0].Tag.Id == 1);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetAsync(string, EPostType)"/> will return null
        /// if the specified slug is not found.
        /// </summary>
        [Fact]
        public async void GetPost_By_Slug_Will_Return_Null_If_Not_Found()
        {
            // Act: getting a post that is not there
            var blogPost = await _postRepo.GetAsync("not-found", EPostType.BlogPost);

            // Assert
            Assert.Null(blogPost);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetAsync(string, int, int, int)"/> will return null
        /// if the specified slug and year, month, day combo is not found.
        /// </summary>
        [Fact]
        public async void GetPost_By_Slug_And_Date_Will_Return_Null_If_Not_Found()
        {
            // Arrange
            var blogPost = await _postRepo.GetAsync(POST_SLUG, 2016, 12, 31);

            // Assert
            Assert.Null(blogPost);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetAsync(string, int, int, int)"/> will return null
        /// if the specified slug and year, month, day combo is not found.
        /// </summary>
        [Fact]
        public async void GetPost_By_Slug_And_Date_Will_Return_BlogPost_If_Found()
        {
            // Arrange
            SeedTestPost();

            // Act
            var blogPost = await _postRepo.GetAsync(POST_SLUG, 2017, 1, 1);

            // Assert
            Assert.Equal(EPostType.BlogPost, blogPost.Type);
        }

        // -------------------------------------------------------------------- GetPostList

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetListAsync(PostListQuery)"/> querying by 
        /// <see cref="EPostListQueryType.BlogPosts"/> returns only published posts.
        /// </summary>
        [Fact]
        public async void GetPostList_By_BlogPosts_Returns_Only_Published_Posts()
        {
            // Arrange: 5 drafts, 6 published
            SeedTestPosts(11);

            var query = new PostListQuery(EPostListQueryType.BlogPosts)
            {
                PageIndex = 1,
                PageSize = 10,
            };

            // Act
            var list = await _postRepo.GetListAsync(query);

            // Assert
            Assert.Equal(6, list.posts.Count); // only published posts are returned
            Assert.Equal(6, list.totalCount);
            var tags = list.posts[0].PostTags;
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetListAsync(PostListQuery)"/> querying by 
        /// <see cref="EPostListQueryType.BlogDrafts"/> returns only draft posts.
        /// </summary>
        [Fact]
        public async void GetPostList_By_Drafts_Returns_All_Drafts()
        {
            // Arrange: 11 drafts
            SeedTestPosts(23);

            var query = new PostListQuery(EPostListQueryType.BlogDrafts); // draft returns all, so no need for page indx and size

            // Act
            var list = await _postRepo.GetListAsync(query);

            // Assert
            Assert.Equal(11, list.posts.Count); // only published posts are returned
            Assert.Equal(11, list.totalCount);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetListAsync(PostListQuery)"/> querying by 
        /// <see cref="EPostListQueryType.BlogPostsByCategory"/> returns posts for that category.
        /// </summary>
        [Fact]
        public async void GetPostList_By_Category_Returns_Posts_For_Category()
        {
            // Arrange
            SeedTestPosts(11);

            var query = new PostListQuery(EPostListQueryType.BlogPostsByCategory)
            {
                CategorySlug = CAT_SLUG,
                PageIndex = 1,
                PageSize = 10,
            };

            // Act
            var list = await _postRepo.GetListAsync(query);

            // Assert
            Assert.Equal(6, list.posts.Count);
            Assert.Equal(6, list.totalCount);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetListAsync(PostListQuery)"/> querying by 
        /// <see cref="EPostListQueryType.BlogPostsByTag"/> returns posts for that tag.
        /// </summary>
        [Theory]
        [InlineData(TAG1_SLUG, 6)]
        [InlineData(TAG2_SLUG, 0)]
        public async void GetPostList_By_Tag_Returns_Posts_For_Tag(string slug, int expectedPostCount)
        {
            // Arrange: given 11 posts
            SeedTestPosts(11);

            var query = new PostListQuery(EPostListQueryType.BlogPostsByTag)
            {
                TagSlug = slug,
                PageIndex = 1,
                PageSize = 10,
            };

            // Act: GetList
            var list = await _postRepo.GetListAsync(query);

            // Assert
            Assert.Equal(expectedPostCount, list.posts.Count);
            Assert.Equal(expectedPostCount, list.totalCount);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.GetListAsync(PostListQuery)"/> querying for
        /// <see cref="EPostListQueryType.BlogPostsByNumber"/> returns all posts regardless 
        /// their status.
        /// </summary>
        [Fact]
        public async void GetPostList_By_Number_Returns_All_Posts_Regardless_Status()
        {
            // Arrange: given 11 drafts, 12 published
            SeedTestPosts(23);

            // Act: when query Max number of post by MetaWeblog
            var query = new PostListQuery(EPostListQueryType.BlogPostsByNumber) { PageSize = int.MaxValue };
            var list = await _postRepo.GetListAsync(query);

            // Assert: then all posts are returned
            Assert.Equal(23, list.posts.Count); 
            Assert.Equal(23, list.totalCount);
        }

        // -------------------------------------------------------------------- CreatePost

        /// <summary>
        /// Test for <see cref="SqlPostRepository.CreateAsync(Post)"/>, when create a post with 
        /// new category and new tags, you don't have to create the category and tags first, you 
        /// can just create the post and be done with it.
        /// </summary>
        [Fact]
        public async void CreatePost_Will_Create_Its_Category_And_Tags_Automatically()
        {
            // Arrange: given brand new 1 post, 1 cat and 2 tags
            SeedUser();
            var cat = new Category { Slug = "tech", Title = "Technology" };
            var tag1 = new Tag { Slug = "aspnet", Title = "ASP.NET" };
            var tag2 = new Tag { Slug = "cs", Title = "C#" };
            var post = new Post
            {
                Category = cat,
                Body = "A post body.",
                UserId = Actor.AUTHOR_ID,
                UpdatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                RootId = null,
                Title = "Hello World",
                Slug = "hello-world",
                Type = EPostType.BlogPost,
                Status = EPostStatus.Published,
            };
            post.PostTags = new List<PostTag> {
                    new PostTag { Post = post, Tag = tag1 },
                    new PostTag { Post = post, Tag = tag2 },
                };

            // Act: when creating the post
            await _postRepo.CreateAsync(post);

            // Assert: then the category and tags were created as well
            var postAgain = _db.Set<Post>().Include(p => p.PostTags).Single(p => p.Id == post.Id);
            Assert.Equal(cat.Id, postAgain.CategoryId);
            Assert.Equal(tag1.Id, postAgain.PostTags.ToList()[0].TagId);
            Assert.Equal(tag2.Id, postAgain.PostTags.ToList()[1].TagId);
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.CreateAsync(Post)"/>, when create a post with 
        /// 2 existing tags scenario.
        /// </summary>
        [Fact]
        public async void CreatePost_With_Existing_Tags()
        {
            // Arrange: given 1 post, 1 cat and 2 tags
            SeedTestPost();
            // a new post
            var post = new Post
            {
                Body = "A post body.",
                UserId = Actor.AUTHOR_ID,
                UpdatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                RootId = null,
                Title = "Hello World",
                Slug = "hello-world",
                Type = EPostType.BlogPost,
                Status = EPostStatus.Published,
            };
            // the 2 existing tags
            var tag1 = _db.Set<Tag>().Single(t => t.Slug == TAG1_SLUG);
            var tag2 = _db.Set<Tag>().Single(t => t.Slug == TAG2_SLUG);
            // associate them together
            post.PostTags = new List<PostTag> {
                    new PostTag { Post = post, Tag = tag1 },
                    new PostTag { Post = post, Tag = tag2 },
                };

            // Act: when creating the post
            await _postRepo.CreateAsync(post);

            // Assert: tags are with the post
            var postAgain = _db.Set<Post>().Include(p => p.PostTags).Single(p => p.Id == post.Id);
            Assert.Equal(tag1.Id, postAgain.PostTags.ToList()[0].TagId);
            Assert.Equal(tag2.Id, postAgain.PostTags.ToList()[1].TagId);
        }

        // -------------------------------------------------------------------- UpdatePost

        /// <summary>
        /// Test for <see cref="SqlPostRepository.UpdateAsync(Post)"/> when user updates post with
        /// 2 tags by removing one of the two and add another new tag.
        /// </summary>
        [Fact]
        public async void UpdatePost_With_Tags_Updated()
        {
            // Arrange: given 1 post with 2 tags
            SeedTestPost();
            // and another new tag
            var tagRepo = new SqlTagRepository(_db);
            var tagJava = await tagRepo.CreateAsync(new Tag { Title = "Java", Slug = "java" });
            // prep a list of tags
            List<string> tagTitles = new List<string> { TAG2_TITLE, "Java" };
            // get the post
            var post = await _postRepo.GetAsync(1, EPostType.BlogPost);

            // Act: when user updated the post by removing AspNet tag and adding Java tag

            // this won't work, the PostTag is still being tracked 
            //post.PostTags.Clear();
            //post.PostTags.Add(new PostTag { Post = post, Tag = tagJava });
            //post.PostTags.Add(new PostTag { Post = post, Tag = tagCs });

            // instead we have to
            List<string> tagTitlesCurrent = post.PostTags.Select(pt => pt.Tag.Title).ToList();
            var tagsToRemove = tagTitlesCurrent.Except(tagTitles);
            foreach (var t in tagsToRemove)
            {
                post.PostTags.Remove(post.PostTags.Single(pt => pt.Tag.Title == t));
            }
            post.PostTags.Add(new PostTag { Post = post, Tag = tagJava });

            await _postRepo.UpdateAsync(post);

            // Assert: then the post's tags are updated
            var postAgain = await _postRepo.GetAsync(1, EPostType.BlogPost);
            Assert.Equal(2, postAgain.PostTags.Count());
            Assert.True(post.PostTags.ToList()[0].Tag.Slug == "cs");
            Assert.True(post.PostTags.ToList()[1].Tag.Slug == "java");
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.UpdateAsync(Post)"/>, get list returns 
        /// non tracked item, but post can add it.
        /// </summary>
        [Fact]
        public async void UpdatePost_Can_Add_None_Tracked_Tag()
        {
            // Arrange: given 1 post with 2 tags
            SeedTestPost();
            // and a non-tracked tag
            var tagRepo = new SqlTagRepository(_db);
            await tagRepo.CreateAsync(new Tag { Title = "Java", Slug = "java" });
            var tagNonTracked = (await tagRepo.GetListAsync()).Single(t => t.Title == "Java");

            // Act: when user updates post by adding the non-tracked tag
            var post = _db.Set<Post>().Include(p => p.PostTags).Single(p => p.Slug == POST_SLUG);
            post.PostTags.Add(new PostTag { Post = post, Tag = tagNonTracked });
            await _postRepo.UpdateAsync(post);

            // Assert: now post has 3 tags
            var postAgain = _db.Set<Post>().Include(p => p.PostTags).Single(p => p.Id == post.Id);
            Assert.Equal(3, postAgain.PostTags.Count());
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.UpdateAsync(Post)"/>, when you update a post, 
        /// assigning it a new category, there are multiple ways to do it, and there are ways 
        /// that won't work.
        /// </summary>
        [Fact]
        public async void UpdatePost_With_A_New_Category()
        {
            // Arrange a post with a category
            SeedTestPost();
            var post = await _db.Set<Post>().SingleAsync(p => p.Slug == POST_SLUG);
            Assert.Equal(1, post.Category.Id);
            Assert.Equal(1, post.CategoryId);

            // Act
            // either one of 3 three ways will work
            Category newCat = null;

            //// 1. gets by create new
            //newCat = await _catRepo.CreateAsync(new Category { Title = "Fashion", Slug = "fashion" });
            //// 2. gets from db
            //newCat = _db.Categories.Single(c => c.Slug == "fashion");
            // 3. create a new object, note with no id
            newCat = new Category { Title = "Fashion", Slug = "fashion" };

            // either set the obj
            post.Category = newCat;
            // or set the id, in this case you have to have way 1 and 2 happen first
            //post.CategoryId = newCat.Id;

            // these don't work
            //var newCat = (await _catRepo.GetCategoriesAsync()).Single(c => c.Slug == "fashion"); // un-tracked
            //var newCat = new Category { Id = 2, Slug = "fashion", Title = "Fashion" }; // assuming this cat exists, you have to get it from db

            _db.SaveChanges();

            // Assert
            var postAgain = _db.Set<Post>().Include(p => p.PostTags).Single(p => p.Id == post.Id);
            Assert.Equal(newCat.Id, postAgain.CategoryId);
            Assert.Equal(2, postAgain.Category.Id);
            Assert.Equal("fashion", postAgain.Category.Slug);
        }

        // -------------------------------------------------------------------- DeletePost

        /// <summary>
        /// Test for <see cref="SqlPostRepository.DeleteAsync(int)"/>.
        /// </summary>
        [Fact]
        public async void DeletePost_Removes_Post_From_Db()
        {
            // Arrange
            SeedTestPost();

            // Act
            await _postRepo.DeleteAsync(1);

            // Assert
            Assert.Equal(0, _db.Set<Post>().Count());
        }

        /// <summary>
        /// Test for <see cref="SqlPostRepository.DeleteAsync(int)"/> when delete an id not found,
        /// it throws InvalidOperationException exception.
        /// </summary>
        [Fact]
        public async void DeletePost_Throws_Exception_If_Id_Not_Found()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _postRepo.DeleteAsync(1));
        }
    }

}
