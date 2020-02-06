This exercise walks you through 3 simple steps to create a front page for your site.

[TOC]

# Create a New Page

Go to [Pages][1] and add a new page.

- **Title**: type in `Home`
- **Body**: copy and paste in the following code

```html
<style>
.hero { text-align: center; margin-bottom: 50px; }
.hero h1 { font-size: 3rem }
.hero p { font-size: 1.6rem; line-height: 1.3; color: #6a8bad; }
</style>
<div class="hero">
  <h1>Fanray</h1>
  <p>A simple and elegant blog.</p>
</div>

<style>
.action { margin: 3rem 0 1rem 0; text-align: center }
</style>
<p class="action">
  <a href="/blog" class="btn btn-primary btn-lg active" role="button" aria-pressed="true">Get Started →</a>
</p>
```

- **Description**: type in `Welcome to Fanray`
- **Layout**: choose `Full` which gives you that full page design

Click on **Publish** to make it public.

# Set "Home Page" as Home

Go to [Navigation][2], you should see the new **Home** page listed under the **Pages** panel, click on the 3 vertical dots and you will see a **Set as Home** button, click on it. This will set the **Home** page as the root of your site.

# Add Link to Site Navigation

Finally, you want to add a Home link on to the navigation. In the [Navigation][2], add a **Custom Link**.

- **URL**: type in `/`
- **Text**: type in `Home`

Click on **Add To Menu** to add it to the Main Menu. Then drag the **Home** item to the top as the first item on the navigation.

<span class="prev">[[Pages]]</span>
<span class="next">[[Docs]]</span>

  [1]: /admin/pages
  [2]: /admin/navigation