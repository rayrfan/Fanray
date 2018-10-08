var app = new Vue({
    el: '#app',
    store,
    mixins: [composeMixin],
    data: () => ({
        pubClicked: false,
        pubText: '',
        saveVisible: false,
        saveDisabled: false,
        saveText: 'Save',
        closing: true,
        fieldChanged: false,
        drawer: null,
        panel: [true, true, true],
        menuDate: false,
        mediaDialogVisible: false,
        previewUrl: null,
        postUrl: null,
        previewDialogVisible: false,
        progressVisible: false,
        editor: null,
        snackbar: {
            show: false,
            text: '',
            color: '',
        },
    }),
    computed: {
        disablePubButton() {
            return this.post.title.trim().length <= 0 || this.pubClicked;
        },
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        headers() {
            return { headers: { 'XSRF-TOKEN': this.tok } };
        },
        payload: function () {
            return {
                id: this.post.id,
                postDate: this.post.postDate,
                categoryId: this.post.categoryId,
                tags: this.post.tags,
                slug: this.post.slug,
                excerpt: this.post.excerpt,
                title: this.post.title,
                body: this.editor.getData(),
            }
        },
        selectedImages() { // from store
            return this.$store.state.selectedImages;
        },
    },
    mounted() {
        this.pubText = this.post.published ? 'Update' : 'Publish';
        this.initEditor();
    },
    methods: {
        initEditor() {
            let typingTimer;
            let self = this;
            ClassicEditor.create(document.querySelector('#editor'), {
                autosave: {
                    save(editor) {
                        clearTimeout(typingTimer);
                        if (!self.post.published) {
                            self.saveVisible = true;
                            self.saveDisabled = false;
                            self.saveText = 'Save';
                            typingTimer = setTimeout(self.saveDraft, self.autosaveInterval);
                        }
                    }
                }
            })
                .then(editor => {
                    self.editor = editor;
                    console.log('editor initialized: ', self.editor);
                })
                .catch(error => {
                    console.error(error);
                });
        },
        onFieldsChange() {
            this.fieldChanged = true;
            if (this.post.published) return;
            this.saveVisible = true;
            this.saveDisabled = false;
            this.saveText = 'Save';
        },
        saveDraft() {
            console.log('save draft');
            this.saveVisible = true;
            this.saveDisabled = true;
            this.saveText = 'Saving...';

            console.log('payload: ', this.payload);
            axios.post('/admin/compose?handler=save', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.post.id = resp.data.id;
                    this.post.slug = resp.data.slug;
                    this.post.draftDate = resp.data.draftDate;
                    this.post.isDraft = true;
                    if (window.location.href.endsWith('/compose'))
                        history.replaceState({}, null, window.location.href + `/${this.post.id}`);
                })
                .catch(err => { console.log(err); });

            this.fieldChanged = false;
            this.saveText = 'Saved';
        },
        preview() {
            this.previewDialogVisible = true;
            this.payload.body = this.editor.getData();
            axios.post('/admin/compose?handler=preview', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.previewUrl = resp.data;
                    this.postUrl = this.previewUrl.replace('preview/', '');
                })
                .catch(err => { console.log(err); });
        },
        closePreview() {
            this.previewDialogVisible = false;
            this.previewUrl = null;
            this.postUrl = null;
        },
        revert() {
            this.post.published = false;
            this.pubText = this.post.published ? 'Update' : 'Publish';
            this.saveDraft();
        },
        pub() {
            this.closing = false;
            this.pubClicked = true;
            this.pubText = this.post.published ? 'Updating...' : 'Publishing...';

            const url = this.post.published ? '/admin/compose?handler=update' : '/admin/compose?handler=publish';
            axios.post(url, this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    window.location.replace(resp.data);
                })
                .catch(err => { console.log(err); });
        },
        /**
         * Inserts selected images to editor.
         * https://docs.ckeditor.com/ckeditor5/latest/builds/guides/faq.html#where-are-the-editorinserthtml-and-editorinserttext-methods-how-to-insert-some-content
         * https://docs.ckeditor.com/ckeditor5/latest/features/image.html#image-captions
         */
        insertImages() {
            let imgsHtml = '';
            this.selectedImages.forEach(img => {
                imgsHtml += `<figure class="image"><img src="${img.urlMedium}" alt="${img.alt}" title="${img.title}"><figcaption>${img.caption}</figcaption></figure>`;
                img.selected = false; // remove the checkmark
            });
            this.$store.dispatch('emptySelectedImages'); // clear selectedImages so gallery buttons could hide
            const viewFragment = this.editor.data.processor.toView(imgsHtml);
            const modelFragment = this.editor.data.toModel(viewFragment);
            this.editor.model.insertContent(modelFragment, this.editor.model.document.selection);
            this.mediaDialogVisible = false;
        },
        closeMediaDialog() {
            this.mediaDialogVisible = false;
            this.selectedImages.forEach(img => img.selected = false ); // remove checkmarks
            this.$store.dispatch('emptySelectedImages'); // clear selectedImages so gallery buttons could hide
        },
        titleEnter() {
            this.post.title = this.post.title.replace(/\n/g, ' ');
        },
        toast(text, color = 'silver') {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = color;
        },
        toastError(text) {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = 'red';
        },
    },
});