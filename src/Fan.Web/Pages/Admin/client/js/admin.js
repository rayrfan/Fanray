var app = new Vue({
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
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        headers() {
            return { headers: { 'XSRF-TOKEN': this.tok } };
        },
    },
    mounted() {
        this.initActiveNav();
    },
    methods: {
        /**
         * Make the current admin side nav active.
         */
        initActiveNav() {
            var url = window.location.pathname;
            this.adminNavs.forEach(function (nav) {
                nav.active = url.startsWith(nav.url);
            });
        },
        logout: function () {
            console.log('logout');
            axios.post('/account/logout', null, this.headers)
                .then(function (response) {
                    window.location = '/';
                })
                .catch(function (error) {
                    console.log(error);
                });
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
    }
});