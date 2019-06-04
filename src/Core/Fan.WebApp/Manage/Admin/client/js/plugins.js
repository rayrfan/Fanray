Vue.component('plugins', {
    mixins: [pluginsMixin],
    data: () => ({
        headers: [
            { text: 'Plugin', value: 'plugin' },
            { text: 'Description', value: 'description', sortable: false },
            { text: 'Actions', value: 'actions', sortable: false }
        ],
        dialogVisible: false,
        dialogTitle: '',
        settingsUrl: null,
    }),
    mounted() {
        this.initSettingsUpdatedHandler();
    },
    methods: {
        activate(plugin) {
            console.log('activate: ', plugin);
            axios.post('/admin/plugins?handler=activate', { folder: plugin.folder }, this.$root.headers)
                .then(resp => {
                    console.log('activated: ', resp.data);
                    plugin.active = true;
                    plugin.id = resp.data;
                })
                .catch(err => {
                    console.log(err);
                    this.$root.toastError('Activate plugin failed.');
                });
        },
        deactivate(plugin) {
            console.log('deactivate: ', plugin);
            axios.post('/admin/plugins?handler=deactivate', { id: plugin.id }, this.$root.headers)
                .then(resp => {
                    console.log('deactivated: ', resp.data);
                    plugin.active = false;
                })
                .catch(err => {
                    console.log(err);
                    this.$root.toastError('Deactivate plugin failed.');
                });
        },
        viewSettings(plugin) {
            this.dialogTitle = plugin.name;
            this.dialogVisible = true;
            this.settingsUrl = plugin.settingsUrl;
        },
        closeDialog() {
            this.dialogVisible = false;
        },
        initSettingsUpdatedHandler() {
            let self = this;
            window.document.addEventListener('ExtSettingsUpdated', e => {
                self.$root.toast(e.detail.msg);
                self.closeDialog();
            });
        },
    }
});
