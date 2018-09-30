/**
 * 
 */
Vue.component('blog-categories', {
    mixins: [categoriesMixin],
    data: () => ({
        dialogVisible: false,
        title: '',
        description: '',
        upsertText: 'Add',
        catEditing: null,
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
        // when user clicks on the plus fab to add a new category
        addCat() {
            this.dialogVisible = true;
            this.title = '';
            this.description = '';
            this.upsertText = 'Add';
        },
        // when user clicks on the Edit from the dropdown
        editCat(cat) {
            this.dialogVisible = true;
            this.catEditing = cat;
            this.upsertText = 'Edit';
            this.title = cat.title;
            this.description = cat.description;
        },
        // when user clicks on the Delete from the dropdown
        deleteCat(cat) {
            console.log('cat being deleted: ', cat);
            if (confirm(`Are you sure you want to permanently delete '${cat.title}'?`)) {
                axios.delete(`/admin/categories?id=${cat.id}`, this.$root.headers)
                    .then(resp => {
                        let idx = this.categories.findIndex(c => c.id === cat.id);
                        let defaultCat = this.categories.find(c => c.id === this.defaultCategoryId);
                        defaultCat.count += cat.count;
                        this.categories.splice(idx, 1);
                        this.$root.toast('Category deleted.');
                    })
                    .catch(err => {
                        this.$root.toastError('Delete category failed.');
                        console.error(err);
                    });
            }
        },
        // when user clicks on the Set as Default from the dropdown
        setDefault(id) {
            axios.post(`/admin/categories?handler=default&id=${id}`, null, this.$root.headers)
                .then(resp => {
                    this.defaultCategoryId = id;
                })
                .catch(err => {
                    this.$root.toast('Set default category failed.', 'red');
                    console.log(err);
                });
        },
        // when user clicks on the View Posts from the dropdown
        viewPosts(url) {
            window.location.href = url; // relative 
        },
        // when user clicks on the Add button on the dialog
        insertCat() {
            axios.post('/admin/categories', this.payload, this.$root.headers)
                .then(resp => {
                    this.closeDialog();
                    this.categories.push(resp.data);
                    this.sortCategories();
                    this.$root.toast('New category added.');
                })
                .catch(err => {
                    this.errMsg = err.response.data[0].errorMessage;
                    this.$root.toast('Add category failed.', 'red');
                });
        },
        // when user clicks on the Update button on the dialog
        updateCat() {
            this.payload.id = this.catEditing.id;
            this.payload.count = this.catEditing.count;
            axios.post('/admin/categories?handler=update', this.payload, this.$root.headers)
                .then(resp => {
                    this.closeDialog();
                    // replace
                    let idx = this.categories.findIndex(c => c.id === resp.data.id);
                    this.categories[idx] = resp.data;
                    // sort
                    this.sortCategories();
                    this.$root.toast('Category updated.');
                })
                .catch(err => {
                    this.errMsg = err.response.data[0].errorMessage;
                    this.$root.toast('Update category failed.', 'red');
                });
        },
        sortCategories() {
            this.categories.sort((a, b) => {
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