let app = new Vue({
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
        editor: null,
        snackbar: {
            show: false,
            text: '',
            color: '',
        },
        composerUploadProgress: false,
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
        this.initWindowDnd();
        this.initEditor();
    },
    methods: {
        /**
         * Initialize window drag drop events, dragFilesUpload and sendIMages.
         */
        initWindowDnd() {
            console.log('initWindowDnd');
            window.addEventListener("dragenter", function (e) {
                document.querySelector("#dropzone").style.visibility = "";
                document.querySelector("#dropzone").style.opacity = 1;
            });

            window.addEventListener("dragleave", function (e) {
                e.preventDefault();
                document.querySelector("#dropzone").style.visibility = "hidden";
                document.querySelector("#dropzone").style.opacity = 0;
            });

            window.addEventListener("dragover", function (e) {
                e.preventDefault();
                document.querySelector("#dropzone").style.visibility = "";
                document.querySelector("#dropzone").style.opacity = 1;
            });

            let self = this;
            window.addEventListener("drop", function (e) {
                e.preventDefault();
                document.querySelector("#dropzone").style.visibility = "hidden";
                document.querySelector("#dropzone").style.opacity = 0;
                if (!self.mediaDialogVisible) self.dragFilesUpload(e.dataTransfer.files); // prevent gallery call this
            });
        },
        dragFilesUpload(files) {
            if (!files.length) return;
            this.composerUploadProgress = true; // open progress
            const formData = new FormData();
            Array.from(Array(files.length).keys()).map(x => formData.append('images', files[x]));
            this.sendImages(formData);
        },
        sendImages(formData) {
            axios.post('/admin/media?handler=image', formData, this.$root.headers)
                .then(resp => {
                    if (resp.data.images.length > 0) {
                        this.insertImagesToEditor(resp.data.images);
                        this.$root.toast('Image uploaded.');
                    }
                    else {
                        this.$root.toastError('Some files were not uploaded due to file type or size not supported.');
                    }
                    this.composerUploadProgress = false; // close progress
                    this.$store.dispatch('emptyErrMsg');
                })
                .catch(err => {
                    this.composerUploadProgress = false; // close progress
                    this.$root.toastError('Image upload failed.');
                    console.log(err);
                });
        },
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
            axios.post('/admin/compose/post?handler=save', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.post.id = resp.data.id;
                    this.post.slug = resp.data.slug;
                    this.post.draftDate = resp.data.draftDate;
                    this.post.isDraft = true;
                    if (window.location.href.endsWith('/compose/post'))
                        history.replaceState({}, null, window.location.href + `/${this.post.id}`);
                })
                .catch(err => { console.log(err); });

            this.fieldChanged = false;
            this.saveText = 'Saved';
        },
        preview() {
            this.previewDialogVisible = true;
            this.payload.body = this.editor.getData();
            axios.post('/admin/compose/post?handler=preview', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
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

            const url = this.post.published ? '/admin/compose/post?handler=update' : '/admin/compose/post?handler=publish';
            axios.post(url, this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    window.location.replace(resp.data);
                })
                .catch(err => { console.log(err); });
        },
        /**
         * Inserts selected images from gallery to editor.
         */
        insertImages() {
            this.insertImagesToEditor(this.selectedImages);
            this.mediaDialogVisible = false;
            this.$store.dispatch('emptySelectedImages'); // clear selectedImages so gallery buttons could hide
            this.$store.dispatch('emptyErrMsg');
        },
        /**
         * Inserts images to editor.
         * https://docs.ckeditor.com/ckeditor5/latest/builds/guides/faq.html#where-are-the-editorinserthtml-and-editorinserttext-methods-how-to-insert-some-content
         * https://docs.ckeditor.com/ckeditor5/latest/features/image.html#image-captions
         */
        insertImagesToEditor(images) {
            let imgsHtml = '';
            images.forEach(img => {
                imgsHtml += `<figure class="image"><img src="${img.urlMedium}" alt="${img.alt}" title="${img.title}"><figcaption>${img.caption}</figcaption></figure>`;
                img.selected = false; // remove the checkmark
            });
            const viewFragment = this.editor.data.processor.toView(imgsHtml);
            const modelFragment = this.editor.data.toModel(viewFragment);
            this.editor.model.insertContent(modelFragment, this.editor.model.document.selection);
        },
        closeMediaDialog() {
            this.mediaDialogVisible = false;
            this.selectedImages.forEach(img => img.selected = false); // remove checkmarks
            this.$store.dispatch('emptySelectedImages'); // clear selectedImages so gallery buttons could hide
            this.$store.dispatch('emptyErrMsg');
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