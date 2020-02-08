Vue.component('navigation', {
    mixins: [navigationMixin],
    data: () => ({
        dialogVisible: false,
        dialogTitle: '',
        settingsUrl: null,
        infoPanels: [true, false, false, false],
        custLinkValid: false,
        custLinkUrl: '',
        custLinkText: '',
        rules: {
            required: value => !!value || 'Required.', // trim handled on server side
            url: value => {
                //const pattern = 'TODO url regex that includes root /'
                //return pattern.test(value) || 'Invalid URL.'
                return true;
            }
        },
        infoOptions: {
            group: { name: 'navs', pull: 'clone', put: false },
            sort: false
        },
        menuOptions: {
            group: 'navs',
            ghostClass: 'sortable-ghost'
        }
    }),
    mounted() {
        this.initSettingsUpdatedHandler();
    },
    methods: {
        async add(evt) {
            let im = {
                fromId: evt.from.id,
                menuId: evt.to.id,
                index: evt.newIndex,
                oldIndex: evt.oldIndex,
            };

            if (evt.from.id === 'page-navs') {
                let nav = this.pages[evt.oldIndex];
                im.id = nav.id;
                im.type = nav.type;
                im.text = nav.text;
            }
            else if (evt.from.id === 'app-navs') {
                let nav = this.apps[evt.oldIndex];
                im.id = nav.id;
                im.type = nav.type;
                im.text = nav.text;
            }
            else if (evt.from.id === 'cat-navs') {
                let nav = this.cats[evt.oldIndex];
                im.id = nav.id;
                im.type = nav.type;
                im.text = nav.text;
            }
            else { // from another menu
                // since the nav is dragged to the new menu, use to.id
                let menu = this.menus.find(m => m.id.toString() === evt.to.id);
                let nav = menu.navs[evt.newIndex];
                im.id = nav.id;
                im.type = nav.type;
                im.text = nav.text;
                im.title = nav.title;
                im.url = nav.url;
                im.origNavName = nav.origNavName;
            }

            console.log('Nav being added: ', im);

            try {
                let resp = await axios.post('/admin/navigation?handler=add', im, this.$root.headers);
                let menu = this.menus.find(m => m.id.toString() === evt.to.id);
                let nav = menu.navs[evt.newIndex];
                nav.settingsUrl = resp.data.settingsUrl;
                nav.origNavName = im.origNavName;
            } catch (e) {
                console.error(e);
                this.$root.toastError('Add menu item failed.');
            }
        },
        async sort(evt) {
            if (evt.from.id !== evt.to.id) return;

            try {
                let im = {
                    menuId: evt.to.id,
                    index: evt.newIndex,
                    oldIndex: evt.oldIndex,
                };

                console.log('sort: ', im);
                await axios.post('/admin/navigation?handler=sort', im, this.$root.headers);
            } catch (e) {
                console.error(e);
                this.$root.toastError('Sort menu item failed.');
            }
        },
        async deleteNav(menuId, index) {
            if (confirm(`Are you sure to delete this menu item?`)) {
                try {
                    await axios.delete(`/admin/navigation?menuId=${menuId}&index=${index}`, this.$root.headers);
                    let menu = this.menus.find(m => m.id === menuId);
                    menu.navs.splice(index, 1);
                } catch (e) {
                    console.error(e);
                    this.$root.toastError('Delete menu item failed.');
                }
            }
        },
        async addCustLink() {
            try {
                console.log('custLinkUrl: ', this.custLinkUrl);
                console.log('custLinkText: ', this.custLinkText);
                console.log('selectedMenuId', this.selectedMenuId);
                console.log('menu', this.menus.find(m => m.id === this.selectedMenuId).navs);

                let nav = {
                    url: this.custLinkUrl,
                    text: this.custLinkText,
                    menuId: this.selectedMenuId,
                    index: this.menus.find(m => m.id === this.selectedMenuId).navs.length,
                };

                let resp = await axios.post('/admin/navigation?handler=customLink', nav, this.$root.headers);
                console.log(resp.data);
                nav.settingsUrl = resp.data.settingsUrl;
                nav.type = resp.data.type;
                this.menus.find(m => m.id === this.selectedMenuId).navs.push(nav);

                // reset form unfortunately clears v-select, need to figure out how to avoid
                //this.$refs.custLinkForm.reset();
                //console.log('selectedMenuId', this.selectedMenuId);
                //this.selectedMenuId = 1; // reset the select
            } catch (e) {
                console.error(e);
                this.$root.toastError('Add menu item failed.');
            }
        },
        async setHome(nav) {
            try {
                console.log('Set nav as home: ', nav);
                await axios.post(`/admin/navigation?handler=home`, nav, this.$root.headers);
                this.home = nav;
                this.$root.toast('Home updated.');
            } catch (e) {
                console.error(e);
                this.$root.toastError('Set as home failed.');
            }
        },
        viewSettings(nav) {
            console.log(nav);
            this.dialogTitle = nav.text;
            this.dialogVisible = true;
            this.settingsUrl = nav.settingsUrl;
        },
        closeDialog() {
            this.dialogVisible = false;
        },
        initSettingsUpdatedHandler() {
            let self = this;
            window.document.addEventListener('ExtSettingsUpdated', e => {
                console.log('nav settings updated: ', e.detail);

                // update the menu nav text
                let menu = this.menus.find(m => m.id === e.detail.menuId);
                let nav = menu.navs[e.detail.index];
                nav.text = e.detail.text;

                // show toast and close dialog
                self.$root.toast(e.detail.msg);
                self.closeDialog();
            });
        },
    }
});
