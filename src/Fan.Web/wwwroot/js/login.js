/**
 * v1.1 @see Login.cshtml
 */
new Vue({
    el: '#app',
    data: {
        valid: false,
        userName: '',
        nameRules: [
            v => !!v.trim() || 'Email or username is required',
        ],
        password: '',
        passwordRules: [
            v => !!v.trim() || 'Password is required',
        ],
        rememberMe: false,
        errMsg: '',
    },
    computed: {
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        payload: function () {
            return {
                userName: this.userName,
                password: this.password,
                rememberMe: this.rememberMe,
            }
        },
    },
    methods: {
        login() {
            axios.post('/api/auth/login', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    window.location.replace(this.getQueryParam('ReturnUrl') || '/admin');
                })
                .catch(err => {
                    console.log(err);
                    this.errMsg = 'Login failed, please try again!';
                });
        },
        // https://stackoverflow.com/a/901144/32240
        getQueryParam(name) {
            let url = window.location.href;
            name = name.replace(/[\[\]]/g, "\\$&");
            let regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }
    }
})