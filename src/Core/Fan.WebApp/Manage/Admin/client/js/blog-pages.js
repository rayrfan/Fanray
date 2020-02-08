Vue.component('pages', {
    mixins: [pagesMixin],
    data: () => ({
        loading: false,
    }),
    methods: {
        deletePage(page) {
            if (confirm('Are you sure to permanently delete this page and its child pages if any?')) {
                axios.delete(`/admin/pages?pageId=${page.id}`, { headers: { 'XSRF-TOKEN': this.$root.tok } })
                    .then(resp => {
                        let idx = this.pages.findIndex(p => p.id === page.id);
                        this.pages.splice(idx, 1);

                        if (this.parentId > 0) {
                            if (page.isChild) {
                                let pidx = this.pages.findIndex(p => p.id === this.parentId);
                                let parent = this.pages[pidx];
                                parent.childCount--;
                            }
                            else {
                                window.location.replace('/admin/pages');
                            }
                        }

                        this.$root.toast('Page deleted.');
                    })
                    .catch(function (err) {
                        this.$root.toastError('Delete page failed.');
                        console.log(err);
                    });
            }
        },
    },
});
