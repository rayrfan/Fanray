Vue.component('widgets', {
    mixins: [widgetsMixin],
    data: () => ({
        dialogVisible: false,
        dialogTitle: '',
        settingsUrl: null,
    }),
    mounted() {
        this.initSettingsUpdatedHandler();
    },
    methods: {
        /**
         * When user drags a widget from info's section or an area to an area.
         * @@param evt
         */
        add(evt) {
            let fromInfos = evt.from.id === 'widget-infos';
            let dto = {
                areaToId: evt.to.id,
                index: evt.newIndex,
                folder: null,
                widgetId: 0,
                areaFromId: null,
                name: null,
                title: null,
            };

            if (fromInfos) {
                dto.folder = this.widgetInfos[evt.oldIndex].folder;
            }
            else {
                let area = this.widgetAreas.find(a => a.id === evt.from.id);
                let widgetIns = area.widgetInstances[evt.oldIndex];

                dto.folder = widgetIns.folder;
                dto.widgetId = widgetIns.id;
                dto.areaFromId = evt.from.id;
                dto.name = widgetIns.name;
                dto.title = widgetIns.title;
            }

            axios.post('/admin/widgets?handler=add', dto, this.$root.headers)
                .then(resp => {
                    let areaTo = this.widgetAreas.find(a => a.id === evt.to.id);
                    if (fromInfos) {
                        areaTo.widgetInstances.splice(evt.newIndex, 0, resp.data);
                    }
                    else {
                        // remove from widget from old area
                        let areaFrom = this.widgetAreas.find(a => a.id === evt.from.id);
                        areaFrom.widgetInstances.splice(evt.oldIndex, 1);
                        // add to new area
                        areaTo.widgetInstances.splice(evt.newIndex, 0, resp.data);
                    }
                })
                .catch(err => {
                    console.log(err);
                    this.$root.toastError('Add widget failed.');
                });
        },
        /**
         * When user drags a widget instance to reorder it within an area.
         * @@param evt
         */
        sort(evt) {
            if (evt.from.id !== evt.to.id) return;
            console.log("ordering... ");

            let area = this.widgetAreas.find(a => a.id === evt.from.id);
            let widgetInst = area.widgetInstances[evt.oldIndex];
            let dto = {
                index: evt.newIndex,
                widgetId: widgetInst.id,
                areaId: evt.from.id,
            };

            axios.post('/admin/widgets?handler=reorder', dto, this.$root.headers)
                .then(() => {
                    // remove from old index and add to new index
                    area.widgetInstances.splice(evt.oldIndex, 1);
                    area.widgetInstances.splice(evt.newIndex, 0, widgetInst);
                })
                .catch(err => {
                    console.log(err);
                    this.$root.toastError('Order widget failed.');
                });
        },
        /**
         * When user clicks on edit icon on an widget instance.
         * @@param widget
         */
        viewSettings(widget) {
            this.dialogTitle = widget.name;
            this.dialogVisible = true;
            this.settingsUrl = widget.settingsUrl;
        },
        /**
         * When user clicks on delete icon on an widget instance.
         * @@param widget
         * @@param areaId
         */
        deleteWidget(widget, areaId) {
            console.log("deleting widget: ", widget);
            if (confirm(`Are you sure to delete the widget?`)) {
                axios.delete(`/admin/widgets?widgetId=${widget.id}&areaId=${areaId}`, this.$root.headers)
                    .then(() => {
                        let area = this.widgetAreas.find(a => a.id === areaId);
                        let widx = area.widgetInstances.indexOf(widget);
                        area.widgetInstances.splice(widx, 1);
                    })
                    .catch(err => {
                        console.log(err);
                        this.$root.toastError('Delete widget failed.');
                    });
            }
        },
        closeDialog() {
            this.dialogVisible = false;
        },
        /**
         * Handles ExtSettingsUpdated event.
         */
        initSettingsUpdatedHandler() {
            let self = this;
            window.document.addEventListener('ExtSettingsUpdated', e => {
                console.log('widget settings updated: ', e.detail);

                // update the widget title
                let area = self.widgetAreas.find(a => a.id === e.detail.areaId);
                let widgetIns = area.widgetInstances.find(w => w.id === e.detail.widgetId);
                widgetIns.title = e.detail.title;

                // show toast and close dialog
                self.$root.toast(e.detail.msg);
                self.closeDialog();
            });
        },
    }
});
