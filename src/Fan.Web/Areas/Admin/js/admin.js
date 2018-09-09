var app = new Vue({
    el: '#app',
    mixins: [adminMixin],
    data: () => ({
        drawer: null,
        snackbar: {
            show: false,
            text: '',
            color: '',
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
    }
});