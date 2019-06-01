new Vue({
    el: '#app',
    mixins: [adminMixin],
    data: () => ({
        drawer: null,
        snackbar: {
            show: false,
            text: '',
            color: '',
            timeout: 0,
        },
    }),
    computed: {
        tok() {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        headers() {
            return { headers: { 'XSRF-TOKEN': this.tok } };
        },
    },
    mounted() {
        this.initActiveNav();
        this.initWindowEventHandlers();
    },
    methods: {
        /**
         * Make the current admin side nav active.
         */
        initActiveNav() {
            let url = window.location.pathname;
            this.adminNavs.forEach(nav => nav.active = url.startsWith(nav.url));
        },
        initWindowEventHandlers() {
            window.document.addEventListener('ExtSettingsUpdateErr', e => {
                console.error('ExtSettingsUpdateErr: ', e);
                this.toastError(e.detail.response.data);
            });
            window.document.addEventListener('ExtSettingsIfrmHeightUpdate', e => {
                console.log('ExtSettingsIfrmHeightUpdate: ', e.detail);
                this.updateIframeHeight(e.detail);
            });
        },
        logout() {
            console.log('logout');
            axios.post('/home/logout', null, this.headers)
                .then(() => window.location = '/')
                .catch((err) => console.log(err));
        },
        /**
         * 
         * @param {any} text
         * @param {any} timeout  Use 0 to keep open indefinitely.
         * @param {any} color
         */
        toast(text, timeout = 3000, color = 'silver') {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = color;
            this.snackbar.timeout = timeout;
        },
        toastError(text, timeout = 3000) {
            this.snackbar.show = true;
            this.snackbar.text = text;
            this.snackbar.color = 'red';
            this.snackbar.timeout = timeout;
        },
        /**
        * A hack to make iframe height same as its content.
        * https://stackoverflow.com/a/9976309/32240
        */
        initIframeHeight(ifrm) {
            ifrm.style.height = 0; // first reset ifrm to start fresh
            ifrm.style.height = ifrm.contentWindow.document.body.scrollHeight + 'px';
        },
        /**
         * Adjust extensions (plugins, widgets) component's settings dialog height.
         * @param h
         */
        updateIframeHeight(h) {
            let ifrm = this.$refs.exts.$refs.settingsIframe;
            ifrm.style.height = h + 'px';
        },
    }
});
