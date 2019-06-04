Vue.component('blog-tags', {
    mixins: [tagsMixin],
    data: () => ({
        dialogVisible: false,
        title: '',
        description: '',
        upsertText: 'Add',
        tagEditing: null,
        errMsg: '',
    }),
    computed: {
        disableUpsertButton() {
            return this.title.trim().length <= 0;
        },
        addButtonVisible() {
            return this.upsertText === 'Add';
        },
        payload() {
            return {
                title: this.title,
                description: this.description,
            }
        },
    },
    methods: {
        closeDialog() {
            this.dialogVisible = false;
            this.title = '';
            this.description = '';
            this.errMsg = '';
        },
        // when user clicks on the plus fab to add a new tag
        addTag() {
            this.dialogVisible = true;
            this.title = '';
            this.description = '';
            this.upsertText = 'Add';
        },
        // when user clicks on the Edit from the dropdown
        editTag(tag) {
            this.dialogVisible = true;
            this.tagEditing = tag;
            this.upsertText = 'Edit';
            this.title = tag.title;
            this.description = tag.description;
        },
        // when user clicks on the Delete from the dropdown
        deleteTag(tag) {
            console.log('tag being deleted: ', tag);
            if (confirm(`Are you sure you want to permanently delete '${tag.title}'?`)) {
                axios.delete(`/admin/tags?id=${tag.id}`, this.$root.headers)
                    .then(resp => {
                        let idx = this.tags.findIndex(c => c.id === tag.id);
                        this.tags.splice(idx, 1);
                        this.$root.toast('Tag deleted.');
                    })
                    .catch(err => {
                        this.$root.toastError('Delete tag failed.');
                        console.error(err);
                    });
            }
        },
        // when user clicks on the View Posts from the dropdown
        viewPosts(url) {
            window.location.href = url; // relative 
        },
        // when user clicks on the Add button on the dialog
        insertTag() {
            axios.post('/admin/tags', this.payload, this.$root.headers)
                .then(resp => {
                    this.closeDialog();
                    this.tags.push(resp.data);
                    this.sortTags();
                    this.$root.toast('New tag added.');
                })
                .catch(err => {
                    this.errMsg = err.response.data;
                    this.$root.toastError('Add tag failed.');
                });
        },
        // when user clicks on the Update button on the dialog
        updateTag() {
            this.payload.id = this.tagEditing.id;
            this.payload.count = this.tagEditing.count;
            axios.post('/admin/tags?handler=update', this.payload, this.$root.headers)
                .then(resp => {
                    this.closeDialog();
                    // replace
                    let idx = this.tags.findIndex(c => c.id === resp.data.id);
                    this.tags[idx] = resp.data;
                    // sort
                    this.sortTags();
                    this.$root.toast('Tag updated.');
                })
                .catch(err => {
                    this.errMsg = err.response.data;
                    this.$root.toastError('Update tag failed.');
                });
        },
        sortTags() {
            this.tags.sort((a, b) => {
                var titleA = a.title.toUpperCase();
                var titleB = b.title.toUpperCase();
                if (titleA < titleB) {
                    return -1;
                }
                if (titleA > titleB) {
                    return 1;
                }

                return 0; // equal
            });
        }
    }
});