/**
 * @see Compose.cshtml
 */
let Delta = Quill.import('delta');
const Clipboard = Quill.import('modules/clipboard');
class PlainClipboard extends Clipboard {
    onPaste(e) {
        e.preventDefault()
        const range = this.quill.getSelection()
        const text = e.clipboardData.getData('text/plain')
        const delta = new Delta()
            .retain(range.index)
            .delete(range.length)
            .insert(text)
        const index = text.length + range.index
        const length = 0
        this.quill.updateContents(delta, 'silent')
        this.quill.setSelection(index, length, 'silent')
        this.quill.scrollIntoView()

        clearTimeout(this.typingTimer);
        if (!app.published) {
            app.showSave = true;
            app.disableSave = false;
            app.saveText = 'Save';

            this.typingTimer = setTimeout(app.saveDraft, app.doneTypingInterval);
        }
    }
}
Quill.register('modules/clipboard', PlainClipboard, true);

var app = new Vue({
    el: '#app',
    data: () => ({
        id: 0,
        published: false,
        pubClicked: false,
        pubText: '',
        showSave: false,
        disableSave: false,
        saveText: 'Save',
        closing: true,
        fieldChanged: false,
        drawer: null,
        menuDate: false,
        date: '',
        selectedCatId: 1,
        cats: [],
        selectedTags: [],
        tags: [],
        slug: '',
        excerpt: '',
        title: '',
        quill: null,
        toolbarOptions: [
            [{ 'header': [false, 1, 2, 3, 4, 5, 6,] }],
            ['bold', 'italic', 'underline', 'strike'],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'list': 'check' }],
            ['blockquote', 'code-block'],
            ['link', 'image', 'video'],
            [{ 'align': '' }, { 'align': 'center' }, { 'align': 'right' }, { 'align': 'justify' }],
            [{ 'indent': '-1' }, { 'indent': '+1' }],
            [{ 'color': [] }, { 'background': [] }],
            ['clean'],
        ],
        mediaDialogVisible: false,
        images: [],
        change: null,
        doneTypingInterval: 5000,
    }),
    computed: {
        showMediaDialog: {
            get: function () {
                if (this.mediaDialogVisible) {
                    this.getImages();
                }
                return this.mediaDialogVisible
            },
            set: function (value) {
                if (!value) {
                    this.mediaDialogVisible = false;
                }
            }
        },
        disablePubButton() {
            return this.title.trim().length <= 0 || this.pubClicked;
        },
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        payload: function () {
            let content = this.quill.root.innerHTML;
            if (content === '<p><br></p>') content = '';

            return {
                id: this.id,
                postDate: this.date,
                categoryId: this.selectedCatId,
                tags: this.selectedTags,
                slug: this.slug,
                excerpt: this.excerpt,
                title: this.title,
                body: content,
            }
        },
    },
    mounted() {
        this.initHighlight();
        this.initEditor();
    },
    methods: {
        initEditor() {
            this.quill = new Quill('#editor', {
                modules: {
                    syntax: true,
                    toolbar: this.toolbarOptions,
                    imageResize: {},
                },
                theme: 'snow'
            });

            this.id = window.location.href.substring(window.location.href.lastIndexOf("/") + 1) | 0;
            axios.get(`/admin/compose?handler=post&postId=${this.id}`).then(resp => {
                this.date = resp.data.post.postDate;
                this.selectedCatId = resp.data.post.categoryId;
                this.selectedTags = resp.data.post.tags;
                this.slug = resp.data.post.slug;
                this.excerpt = resp.data.post.excerpt;
                this.title = resp.data.post.title;
                this.quill.clipboard.dangerouslyPasteHTML(resp.data.post.body);
                this.published = resp.data.published;
                this.cats = resp.data.allCats;
                this.tags = resp.data.allTags;
                this.pubText = this.published ? 'Update' : 'Publish';
                this.change = new Delta();

                // auto save
                let typingTimer;
                this.quill.on('text-change', (delta) => {
                    this.change = this.change.compose(delta);
                    clearTimeout(typingTimer);
                    if (this.change.length() > 0 && !this.published) {
                        this.showSave = true;
                        this.disableSave = false;
                        this.saveText = 'Save';

                        typingTimer = setTimeout(this.saveDraft, this.doneTypingInterval);
                    }
                });
                window.onbeforeunload = () => {
                    if ((this.change.length() > 0 || this.fieldChanged) && this.closing) {
                        return 'There are unsaved changes. Are you sure you want to leave?';
                    }
                }
            });

            // media
            let toolbar = this.quill.getModule('toolbar');
            toolbar.addHandler('image', () => this.mediaDialogVisible = true);
        },
        initHighlight() {
            hljs.configure({
                languages: ['cs', 'css', 'java', 'javascript', 'markdown', 'ruby', 'python', 'scss', 'sql', 'typescript', 'xml']
            });
        },
        onFieldsChange() {
            this.fieldChanged = true;
            if (this.published) return;
            this.showSave = true;
            this.disableSave = false;
            this.saveText = 'Save';
        },
        saveDraft() {
            this.showSave = true;
            this.disableSave = true;
            this.saveText = 'Saving...';

            axios.post('/admin/compose?handler=save', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    this.id = resp.data.id;
                    this.slug = resp.data.slug;
                    if (window.location.href.endsWith('/compose'))
                        history.replaceState({}, null, window.location.href + `/${this.id}`);
                })
                .catch(err => { console.log(err); });

            this.change = new Delta();
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
        uploadImages() {
            const input = document.createElement('input');
            input.setAttribute('type', 'file');
            input.setAttribute('accept', 'image/*');
            input.setAttribute('multiple', null);
            input.click();
            input.onchange = () => {
                const formData = new FormData();
                for (let i = 0; i < input.files.length; i++) {
                    formData.append('images', input.files[i]);
                }

                axios.post('/admin/media?handler=image', formData, { headers: { 'XSRF-TOKEN': this.$root.tok } })
                    .then(resp => {
                        this.images = resp.data.images;
                    })
                    .catch(err => {
                        console.log(err);
                    });
            };
        },
        getImages() {
            let url = `/admin/media?handler=images`;
            axios.get(url).then(resp => {
                this.images = resp.data.images;
            }).catch(err => console.log(err));
        },
        insertImage(url) {
            const range = this.quill.getSelection();
            this.quill.insertEmbed(range.index, 'image', url);
            this.mediaDialogVisible = false;
        },
    },
});
