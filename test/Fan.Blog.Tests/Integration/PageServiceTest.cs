using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Tests.Helpers;
using Fan.Exceptions;
using System;
using Xunit;

namespace Fan.Blog.Tests.Integration
{
    /// <summary>
    /// Integration tests for <see cref="Blog.Services.PageService"/> testing the different 
    /// scenarios a user authors pages.
    /// </summary>
    public class PageServiceTest : BlogServiceIntegrationTestBase
    {
        /// <summary>
        /// When you publish a page, it's body, slug and id will be gotten.
        /// </summary>
        [Fact]
        public async void Admin_can_publish_a_page()
        {
            // Given a user
            var userId = Seed_1User();

            // When he publishes a page
            var page = await _pageService.CreateAsync(new Page
            {
                UserId = userId,
                Title = "Test Page",
                Body = "<h1>Test Page</h1>\n", // html comes directly from editor
                BodyMark = "# Test Page",
                CreatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                Status = EPostStatus.Published,
            });

            // Then the page is created with id, slug and body
            Assert.Equal(1, page.Id); // first post got id 1
            Assert.Equal("test-page", page.Slug); // slug is formatted based on its title
            Assert.Equal("<h1>Test Page</h1>\n", page.Body); // body is converted from md
        }

        [Fact]
        public async void Admin_can_publish_a_child_page()
        {
            // Given a published parent page
            var pageId = Seed_1Page();

            // When a child page is created
            var child = await _pageService.CreateAsync(new Page
            {
                ParentId = pageId,
                BodyMark = "# Child Page",
                UserId = Actor.ADMIN_ID,
                CreatedOn = new DateTimeOffset(new DateTime(2019, 07, 30), new TimeSpan(-7, 0, 0)),
                Title = "Test Page",
                Status = EPostStatus.Published,
            });

            // Then GetParentsAsync can return the parent and its child
            var parents = await _pageService.GetParentsAsync(true);
            Assert.Equal(1, parents.Count);
            Assert.Equal(1, parents[0].Children.Count);
            Assert.Equal(child.Id, parents[0].Children[0].Id);
        }

        /// <summary>
        /// Pages are hierarchical, when you publish a child page to a parent page, the returned parent
        /// contains the child.
        /// </summary>
        [Fact]
        public async void Pages_are_hierarchical_parents_contain_children()
        {
            // Given 2 parent pages each with a child page
            Seed_2_Parents_With_1_Child_Each();

            // The returned list is in a hierarchy, the parent contains the child
            var parents = await _pageService.GetParentsAsync(true);
            Assert.Equal(2, parents.Count);
            Assert.Equal(1, parents[0].Children.Count);
        }

        /// <summary>
        /// A parent page slug cannot be one of the <see cref="PageService.Reserved_Slugs"/> values
        /// sincle its slug is used right after the web root.
        /// </summary>
        [Fact]
        public async void Parent_slug_cannot_conflict_with_Reserved_Slugs()
        {
            // Given a user
            var userId = Seed_1User();

            // When he publishes a page with a title "Login" which conflicts with Reserved Slug
            var ex = await Assert.ThrowsAsync<FanException>(() => _pageService.CreateAsync(new Page
            {
                UserId = userId,
                Title = "Login",
                Status = EPostStatus.Published,
            }));

            // Then FanException is thrown with the following msg
            Assert.Equal(string.Format(PageService.RESERVED_SLUG_MSG, "login"), ex.Message);
        }

        /// <summary>
        /// A child page slug can be one of the <see cref="PageService.Reserved_Slugs"/> values 
        /// since its slug is not used right after the web root.
        /// </summary>
        [Fact]
        public async void Child_slug_is_OK_to_use_Reserved_Slugs()
        {
            // Given 2 parent pages each with a child page
            Seed_2_Parents_With_1_Child_Each();

            // When he publishes a child page with a title "Login", it is OK
            await _pageService.CreateAsync(new Page
            {
                UserId = Actor.ADMIN_ID,
                ParentId = 1,
                Title = "Login",
                Status = EPostStatus.Published,
            });
        }

        /// <summary>
        /// A page, whether its parent or child, cannot have duplicate title from its siblings.
        /// </summary>
        [Fact]
        public async void Page_title_cannot_have_duplicate_from_its_siblings()
        {
            // Given 2 parent pages each with a child page
            Seed_2_Parents_With_1_Child_Each();

            // When he publishes a page with a title "Page1" which is a duplicate of a seeded page
            // Then FanException is thrown with the following msg
            var ex = await Assert.ThrowsAsync<FanException>(() => _pageService.CreateAsync(new Page
            {
                UserId = Actor.ADMIN_ID,
                Title = "Page1",
                Status = EPostStatus.Published,
            }));

            Assert.Equal(PageService.DUPLICATE_TITLE_MSG, ex.Message);
        }

        /// <summary>
        /// Drafts are not visible to public.
        /// </summary>
        [Fact]
        public async void Get_draft_page_from_public_throws_FanException()
        {
            // Given a published parent page
            var pageId = Seed_1Page();

            // When update it to draft
            var page = await _pageService.GetAsync(pageId);
            page.Status = EPostStatus.Draft;
            await _pageService.UpdateAsync(page);

            // Then you get exception
            await Assert.ThrowsAsync<FanException>(() => _pageService.GetAsync(isPreview: false, page.Slug));
        }

        /// <summary>
        /// Drafts are not visible to public but they are visible for previewing.
        /// </summary>
        [Fact]
        public async void Get_draft_page_from_preview_does_not_throw_FanException()
        {
            // Given a published parent page
            var pageId = Seed_1Page();

            // When update it to draft
            var page = await _pageService.GetAsync(pageId);
            page.Status = EPostStatus.Draft;
            await _pageService.UpdateAsync(page);

            // Then you get no exception
            await _pageService.GetAsync(isPreview: true, page.Slug);
        }

        [Fact]
        public async void Deleting_a_root_page_also_deletes_its_children()
        {
            // Given 2 parent pages each with a child page
            Seed_2_Parents_With_1_Child_Each();

            var parents = await _pageService.GetParentsAsync(true);
            var parentId = parents[0].Id;
            var childId = parents[0].Children[0].Id;

            // When you delete the parent
            await _pageService.DeleteAsync(parentId);

            // Then the child is gone too
            await Assert.ThrowsAsync<FanException>(() => _pageService.GetAsync(childId));
        }

        /// <summary>
        /// When there are no pages, GetParentsAsync returns an empty list.
        /// </summary>
        [Fact]
        public async void When_there_are_no_pages_GetParentsAsync_returns_empty_list()
        {
            Assert.Empty(await _pageService.GetParentsAsync());
            Assert.Empty(await _pageService.GetParentsAsync(withChildren: true));
        }
    }
}
