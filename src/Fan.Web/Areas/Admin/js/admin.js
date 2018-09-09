var app = new Vue({
    el: '#app',
    mixins: [adminMixin],
    data: () => ({
        drawer: null,
        active: true,
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
        var url = window.location.pathname;
        this.adminNavs.forEach(function (nav) {
            nav.active = nav.url === url;
        });
    },
    methods: {
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