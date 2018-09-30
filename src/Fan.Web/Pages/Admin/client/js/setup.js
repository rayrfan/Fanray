/**
 * Setup.cshtml
 */

new Vue({
    el: '#app',
    mixins: [setupMixin],
    data: {
        valid: false,
        title: '',
        titleRules: [
            v => !!v.trim() || 'Required',
        ],
        email: '',
        emailRules: [
            v => !!v || 'Required',
            v => /.+@.+/.test(v) || 'Email must be valid',
        ],
        userName: '',
        userNameRules: [
            v => !!v || 'Required',
            v => v.length >= 2 || 'Min 2 characters',
            v => v.length <= 24 || 'Max 20 characters',
            v => /^[a-zA-Z0-9-_]+$/.test(v) || 'Alphanumeric, dash, underscore only',
        ],
        displayName: '',
        displayNameRules: [
            v => !!v || 'Required',
            v => v.length >= 2 || 'Min 2 characters',
            v => v.length <= 24 || 'Max 24 characters',
        ],
        password: '',
        passwordRules: {
            required: v => !!v.trim() || 'Required',
            min: v => v.length >= 8 || 'Min 8 characters',
        },
        passwordVisible: false,
        errMsg: '',
    },
    computed: {
        tok: function () {
            return document.querySelector('input[name="__RequestVerificationToken"][type="hidden"]').value;
        },
        payload: function () {
            return {
                title: this.title,
                timeZoneId: this.selectedTimeZoneId,
                email: this.email,
                userName: this.userName,
                displayName: this.displayName,
                password: this.password,
            };
        },
    },
    methods: {
        createBlog() {
            this.valid = false;
            axios.post('/setup', this.payload, { headers: { 'XSRF-TOKEN': this.tok } })
                .then(resp => {
                    window.location.replace('/');
                })
                .catch(err => {
                    this.valid = true;
                    console.log(err);
                    this.errMsg = err.response.data;
                });
        },
    }
});