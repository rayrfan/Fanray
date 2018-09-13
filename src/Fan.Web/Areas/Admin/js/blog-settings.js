Vue.component('blog-settings', {
    mixins: [settingsMixin],
    data: () => ({
        radioGroup: 1,
        siteSettingsValid: false,
        blogSettingsValid: false,
        titleRules: [
            v => !!v.trim() || 'Required',
        ],
        postPerPageRules: [
            v => !!v || 'Required',
            v => /^[0-9]+$/.test(v) || 'Integer only',
            v => (parseInt(v) >= 1 && parseInt(v) <= 50) || 'Must be between 1 and 50',
        ],
        errMsg: '',
    }),
    methods: {
        saveSiteSettings() {
            let payload = {
                title: this.title,
                tagline: this.tagline,
                timeZoneId: this.selectedTimeZoneId,
                googleAnalyticsTrackingID: this.ga,
            };
            axios.post('/admin/settings?handler=siteSettings', payload, this.$root.headers)
                .then(resp => {
                    this.$root.toast('Site settings saved!', 0, 'green');
                })
                .catch(err => {
                    console.log(err);
                    this.errMsg = err.response.data[0].errorMessage;
                });
        },
        saveBlogSettings() {
            let payload = {
                disqusShortname: this.disqusShortname,
                postListDisplay: this.selectedPostListDisplay,
                allowComments: this.allowComments,
                postPerPage: this.postPerPage,
            };
            axios.post('/admin/settings?handler=blogSettings', payload, this.$root.headers)
                .then(resp => {
                    this.$root.toast('Blog settings saved!', 0, 'green');
                })
                .catch(err => {
                    console.log(err);
                    this.errMsg = err.response.data[0].errorMessage;
                });
        },
    },
});