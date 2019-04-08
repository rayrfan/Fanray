Vue.component('site-users', {
    mixins: [siteUsersMixin],
    data: () => ({
        dialog: false,
        editDialog: false,
        headers: [
            { text: '', value: 'avatar', sortable: false },
            { text: 'Display Name', value: 'displayName' },
            { text: 'Username', value: 'userName' },
            { text: 'Joined On', value: 'joinedOn' },
            { text: 'Locked Out', value: 'lockedOut' },
            { text: 'Role', value: 'role' },
            { text: 'Actions', value: 'name', sortable: false }
        ],
        editedIndex: -1,
        editedUser: {
            email: '',
            displayName: '',
            userName: '',
            password: '',
            passwordVisible: false,
            role: '',
        },
        defaultUser: {
            email: '',
            displayName: '',
            userName: '',
            password: '',
            passwordVisible: false,
            role: '',
        },
        rules: {
            emailRules: [
                v => !!v || 'Required',
                v => /.+@.+/.test(v) || 'Email must be valid',
            ],
            userNameRules: [
                v => !!v || 'Required',
                v => (!!v && v.length >= 2) || 'Min 2 characters',
                v => (!!v && v.length <= 24) || 'Max 20 characters',
                v => /^[a-zA-Z0-9-_]+$/.test(v) || 'Alphanumeric, dash, underscore only',
            ],
            displayNameRules: [
                v => !!v || 'Required',
                v => (!!v && v.length >= 2) || 'Min 2 characters',
                v => (!!v && v.length <= 24) || 'Max 24 characters',
            ],
            passwordRules: {
                required: v => (!!v && !!v.trim()) || 'Required',
                min: v => (!!v && v.length >= 8) || 'Min 8 characters',
            },
        },
        pagination: {},
        loading: false,
        errMsg: '',
    }),
    methods: {
        /**
         * Open dialog to add a new user.
         */
        addUser() {
            this.dialog = true;
        },
        /**
         * Close dialog and reset form.
         */
        close() {
            this.dialog = false;
            this.editedUser = Object.assign({}, this.defaultUser);
            this.editedIndex = -1;
            this.$refs.form.reset();
            this.errMsg = '';
        },
        /**
         * Upsert a user.
         */
        save() {
            this.editedUser.role = this.selectedRole;
            console.log('Edited User: ', this.editedUser);
            axios.post('/admin/users', this.editedUser, this.$root.headers)
                .then(resp => {
                    console.log('Returned user: ', resp.data);
                    if (this.editedIndex > -1) { // update todo
                        Object.assign(this.users[this.editedIndex], this.editedUser)
                    } else { // insert
                        this.users.push(resp.data);
                    }
                    this.close();
                })
                .catch(err => {
                    console.log(err);
                    this.errMsg = err.response.data;
                    this.$root.toastError('Add user failed.');   
                });
        },
        /**
         * Edit user role.
         * @param {any} user
         */
        editUser(user) {
            alert(`This feature is coming soon.`);
            //this.editedIndex = this.users.indexOf(user)
            //this.editedUser = Object.assign({}, user)
            //this.editDialog = true
        },
        /**
         * Lock user out.
         * @param {any} user
         */
        lockUser(user) {
            alert(`This feature is coming soon.`);
        }
    },
});
