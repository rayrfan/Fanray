let app = new Vue({
    el: '#app',
    mixins: [composeMixin, editorMdMixin],
    data: () => ({
        drawer: null,
        panel: [true, true, true],
        editor: null,
        saveDisabled: false,
        saveText: 'Save',
        snackbar: {
            show: false,
            text: '',
            color: '',
            timeout: 10000,
        },
    }),
    computed: {
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        headers() {
            return { headers: { 'XSRF-TOKEN': this.tok } };
        },
    },
    mounted() {
        this.initEditor();
    },
    methods: {
        async save() {
            try {
                this.saveDisabled = true;
                this.saveText = 'Saving...';
                await axios.post('/admin/compose/pagenav?handler=save',
                    { pageId: this.pageId, navMd: this.editor.getMarkdown() }, { headers: { 'XSRF-TOKEN': this.tok } });
                this.toast('Page navigation saved.');
            } catch (err) {
                this.toastError(err.response.data);
            }
            this.saveText = 'Save';
            this.saveDisabled = false;
        },
        insertNav(title) {
            this.editor.insertValue(`- [[${title}]]\n`);
        },
        toast(text, color = 'silver') {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = color;
            this.snackbar.timeout = 3000;
        },
        toastError(text) {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = 'red';
        },
    },
});
