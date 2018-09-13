/**
 * The js code for Compose.cshtml.
 */
var app = new Vue({
    el: '#app',
    data: () => ({
        id: 0,
        published: false,
        isDraft: false,
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
        date: '',
        draftDate: '',
        selectedCatId: 1,
        cats: [],
        selectedTags: [],
        tags: [],
        slug: '',
        excerpt: '',
        title: '',
        mediaDialogVisible: false,
        progressVisible: false,
        editor: null,
        content: '',
        snackbar: {
            show: false,
            text: '',
            color: '',
        },
    }),
    computed: {
        disablePubButton() {
            return this.title.trim().length <= 0 || this.pubClicked;
        },
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        headers() {
            return { headers: { 'XSRF-TOKEN': this.tok } };
        },
        payload: function () {
            return {
                id: this.id,
                postDate: this.date,
                categoryId: this.selectedCatId,
                tags: this.selectedTags,
                slug: this.slug,
                excerpt: this.excerpt,
                title: this.title,
                body: this.editor.getData(),
            }
        },
    },
    mounted() {
        this.initEditor();
    },
    methods: {
        initEditor() {
            this.id = window.location.href.substring(window.location.href.lastIndexOf("/") + 1) | 0;
            let self = this;
            axios.get(`/admin/compose?handler=post&postId=${this.id}`).then(resp => {
                this.date = resp.data.post.postDate;
                this.selectedCatId = resp.data.post.categoryId;
                this.selectedTags = resp.data.post.tags;
                this.slug = resp.data.post.slug;
                this.excerpt = resp.data.post.excerpt;
                this.title = resp.data.post.title;
                this.content = resp.data.post.body;
                this.published = resp.data.post.published;
                this.isDraft = resp.data.post.isDraft;
                this.draftDate = resp.data.post.draftDate;
                this.cats = resp.data.allCats;
                this.tags = resp.data.allTags;
                this.pubText = this.published ? 'Update' : 'Publish';

                let typingTimer;
                let doneTypingInterval = 5000;

                ClassicEditor.create(document.querySelector('#editor'), {
                    autosave: {
                        save(editor) {
                            clearTimeout(typingTimer);
                            if (!self.published) {
                                self.saveVisible = true;
                                self.saveDisabled = false;
                                self.saveText = 'Save';
                                typingTimer = setTimeout(self.saveDraft, doneTypingInterval);
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
            });
        },
        onFieldsChange() {
            this.fieldChanged = true;
            if (this.published) return;
            this.saveVisible = true;
            this.saveDisabled = false;
            this.saveText = 'Save';
        },
        saveDraft() {
            console.log('save draft');
            this.saveVisible = true;
            this.saveDisabled = true;
            this.saveText = 'Saving...';

            console.log(this.payload);
            axios.post('/admin/compose?handler=save', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.id = resp.data.id;
                    this.slug = resp.data.slug;
                    this.draftDate = resp.data.draftDate;
                    this.isDraft = true;
                    if (window.location.href.endsWith('/compose'))
                        history.replaceState({}, null, window.location.href + `/${this.id}`);
                })
                .catch(err => { console.log(err); });

            this.fieldChanged = false;
            this.saveText = 'Saved';
        },
        revert() {
            this.published = false;
            this.pubText = this.published ? 'Update' : 'Publish';
            this.saveDraft();
        },
        pub() {
            this.closing = false;
            this.pubClicked = true;
            this.pubText = this.published ? 'Updating...' : 'Publishing...';

            const url = this.published ? '/admin/compose?handler=update' : '/admin/compose?handler=publish';
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
        insertImages(images) {
            console.log("selected images to insert: ", images);
            let imgsHtml = '';
            for (var i = 0; i < images.length; i++) {
                imgsHtml += `<figure class="image"><img src="${images[i].urlMedium}" alt="${images[i].alt}" title="${images[i].title}"><figcaption>${images[i].caption}</figcaption></figure>`;
            }
            const viewFragment = this.editor.data.processor.toView(imgsHtml);
            const modelFragment = this.editor.data.toModel(viewFragment);
            this.editor.model.insertContent(modelFragment, this.editor.model.document.selection);
            this.mediaDialogVisible = false;
        },
        titleEnter() {
            this.title = this.title.replace(/\n/g, ' ');
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
