Vue.component('blog-posts', {
    mixins: [blogPostsMixin],
    data: () => ({
        //posts: [],
        //totalPosts: 0,
        //statuses: null,
        //publishedCount: 0,
        //draftCount: 0,
        // activeStatus: 'published',
        pagination: {},
        loading: false,
        //rowsPerPageItems: [5, 10],
    }),
    watch: {
        pagination: {
            handler() {
                console.log('this.activeStatus: ', this.activeStatus);
                // vuetify tab caveat
                if (this.activeStatus === 0)
                    this.activeStatus = 'published';
                else if (this.activeStatus === 1)
                    this.activeStatus = 'draft';

                this.getPosts(this.activeStatus);
            },
            deep: true,
        },
    },
    computed: {
        // vuetify tab is by physical order, thus this conversion
        active: {
            get: function () {
                return (this.activeStatus === 'published') ? 0 : 1;
            },
            set: function (value) { },
        },
    },
    methods: {
        getPosts(status) {
            this.activeStatus = status;
            this.loading = true;
            let url = `/admin/posts?handler=posts&status=${this.activeStatus}&pageNumber=${this.pagination.page}&pageSize=${this.pagination.rowsPerPage}`;
            console.log(url);
            axios.get(url).then(resp => {
                this.posts = resp.data.posts;
                this.totalPosts = resp.data.totalPosts;
                this.statuses = resp.data.statuses;
                this.publishedCount = resp.data.publishedCount;
                this.draftCount = resp.data.draftCount;

                if (window.location.href.endsWith('/admin/posts'))
                    history.pushState({}, null, window.location.href + `/${this.activeStatus}`);
                else if (window.location.href.endsWith('/admin/posts/published') ||
                    window.location.href.endsWith('/admin/posts/draft')) {
                    let newUrl = '/admin/posts/' + this.activeStatus;
                    history.pushState({}, null, newUrl);
                }
            }).catch(function (error) {
                console.log(error);
            });
            this.loading = false;
        },
        deletePost(item) {
            const index = this.posts.indexOf(item);
            if (confirm('You are permanently deleting this post, are you sure?')) {
                this.loading = true;
                let url = `/admin/posts?postId=${item.id}&status=${this.activeStatus}&pageNumber=${this.pagination.page}&pageSize=${this.pagination.rowsPerPage}`;
                axios.delete(url, { headers: { 'XSRF-TOKEN': this.$root.tok } })
                    .then(resp => {
                        this.posts.splice(index, 1);
                        this.posts = resp.data.posts;
                        this.totalPosts = resp.data.totalPosts;
                        this.statuses = resp.data.statuses;
                        this.draftCount = resp.data.draftCount;
                        this.publishedCount = resp.data.publishedCount;
                    })
                    .catch(function (err) {
                        console.log(err);
                    });
                this.loading = false;
            }
        },
    },
});
