let app = new Vue({
    el: '#app',
    store,
    mixins: [composeMixin, editorMdMixin], // from plugin
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
            mode: '',
            timeout: 10000,
        },
        composerUploadProgress: false,
    }),
    computed: {
        disablePubButton() {
            return this.page.title.trim().length <= 0 || this.pubClicked;
        },
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        headers() {
            return { headers: { 'XSRF-TOKEN': this.tok } };
        },
        payload: function () {
            return {
                id: this.page.id,
                parentId: this.page.parentId,
                postDate: this.page.postDate,
                title: this.page.title,
                excerpt: this.page.excerpt,
                pageLayout: this.page.pageLayout,
                //bodyMark: this.editor.getMarkdown(),
            }
        },
        selectedImages() { // from store
            return this.$store.state.selectedImages;
        },
    },
    mounted() {
        this.pubText = this.page.published ? 'Update' : 'Publish';
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
            axios.post('/admin/media?handler=image', formData, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    if (resp.data.images.length > 0) {
                        this.insertImagesToEditor(resp.data.images);
                        this.toast('Image uploaded.');
                    }
                    else {
                        this.toastError('Some files were not uploaded due to file type or size not supported.');
                    }
                    this.composerUploadProgress = false; // close progress
                    this.$store.dispatch('emptyErrMsg');
                })
                .catch(err => {
                    this.composerUploadProgress = false; // close progress
                    this.toastError('Image upload failed.');
                    console.log(err);
                });
        },
        onFieldsChange() {
            this.fieldChanged = true;
            if (this.page.published) return;
            this.saveVisible = true;
            this.saveDisabled = false;
            this.saveText = 'Save';
        },
        saveDraft() {
            console.log('save draft');
            this.saveVisible = true;
            this.saveDisabled = true;
            this.saveText = 'Saving...';

            this.payload.body = this.editor.getPreviewedHTML();
            this.payload.bodyMark = this.editor.getMarkdown();
            console.log('payload: ', this.payload);
            axios.post('/admin/compose/page?handler=save', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.page.id = resp.data.id;
                    this.page.draftDate = resp.data.draftDate;
                    this.page.isDraft = true;

                    let href = window.location.href;
                    if (href.includes('/page?') || href.endsWith('/page')) {
                        let pos = href.indexOf('?');
                        if (pos == -1) {
                            history.replaceState({}, null, href + `/${this.page.id}`);
                        }
                        else {
                            history.replaceState({}, null, href.substring(0, pos) + `/${this.page.id}` + href.substring(pos));
                        }
                    }
                })
                .catch(err => {
                    this.saveVisible = false;
                    this.toastError(err.response.data);
                });

            this.fieldChanged = false;
            this.saveText = 'Saved';
        },
        preview() {
            this.previewDialogVisible = true;
            //this.payload.body = this.editor.getHTML();
            this.payload.body = this.editor.getPreviewedHTML();
            this.payload.bodyMark = this.editor.getMarkdown();

            axios.post('/admin/compose/page?handler=preview', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.previewUrl = resp.data;
                    this.postUrl = this.previewUrl.replace('preview/page/', '');
                })
                .catch(err => { console.log(err); });
        },
        closePreview() {
            this.previewDialogVisible = false;
            this.previewUrl = null;
            this.postUrl = null;
        },
        revert() {
            this.page.published = false;
            this.pubText = this.page.published ? 'Update' : 'Publish';
            this.saveDraft();
        },
        pub() {
            this.closing = false;
            this.pubClicked = true;
            this.pubText = this.page.published ? 'Updating...' : 'Publishing...';

            const url = this.page.published ? '/admin/compose/page?handler=update' : '/admin/compose/page?handler=publish';
            this.payload.body = this.editor.getPreviewedHTML();
            this.payload.bodyMark = this.editor.getMarkdown();
            axios.post(url, this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    if (this.page.isParentDraft)
                        this.close();
                    else
                        window.location.replace(resp.data);
                })
                .catch(err => {
                    this.saveVisible = false;
                    this.pubText = this.page.published ? 'Update' : 'Publish';
                    this.pubClicked = false;
                    this.toastError(err.response.data);
                });
        },
        close() {
            if (this.page.parentId && this.page.parentId > 0)
                window.location.replace(`/admin/pages/${this.page.parentId}`);
            else
                window.location.replace('/admin/pages');
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
         * Inserts images to editor. Note: page uses original img.
         */
        insertImagesToEditor(images) {
            let imgsHtml = '';
            images.forEach(img => {
                imgsHtml += `![${img.alt}](${img.urlOriginal} "${img.title}")\n`;
                img.selected = false; // remove the checkmark
            });
            this.editor.insertValue(imgsHtml);
        },
        closeMediaDialog() {
            this.mediaDialogVisible = false;
            this.selectedImages.forEach(img => img.selected = false); // remove checkmarks
            this.$store.dispatch('emptySelectedImages'); // clear selectedImages so gallery buttons could hide
            this.$store.dispatch('emptyErrMsg');
        },
        titleEnter() {
            this.page.title = this.page.title.replace(/\n/g, ' ');
        },
        toast(text, color = 'silver') {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = color;
            this.snackbar.mode = '';
            this.snackbar.timeout = 3000;
        },
        toastError(text) {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = 'red';
            this.snackbar.timeout = 10000;
            this.snackbar.mode = 'multi-line';
        },
    },
});
