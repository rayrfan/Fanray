// media store
let store = new Vuex.Store({
    strict: true,
    state: {
        selectedImages: [],
        errMsg: '',
    },
    mutations: {
        setSelectedImages(state, newSelectedImages) {
            state.selectedImages = newSelectedImages;
        },
        addSelectedImage(state, image) {
            state.selectedImages.push(image);
        },
        removeSelectedImage(state, idx) {
            state.selectedImages.splice(idx, 1);
        },
        setErrMsg(state, newMsg) {
            if (!newMsg) state.errMsg = newMsg;
            else state.errMsg += newMsg;
        },
    },
    actions: {
        selectImage: function ({ commit }, image) {
            commit('addSelectedImage', image);
        },
        deselectImage: function ({ commit }, idx) {
            commit('removeSelectedImage', idx);
        },
        emptySelectedImages({ commit }) {
            commit('setSelectedImages', []);
        },
        setErrMsg({ commit }, msg) {
            commit('setErrMsg', msg);
        },
        emptyErrMsg({ commit }) {
            commit('setErrMsg', '');
        },
    },
});

// media component
Vue.component('blog-media', {
    template: '#blog-media-template',
    mixins: [mediaMixin],
    props: ['mode'],
    store, // required when media runs solo
    data: () => ({
        editDialogVisible: false,
        progressDialog: false,
        pageNumber: 1,
        selectedImageIdx: 0,
        totalFileCount: 0,
        isEditor: false,
    }),
    mounted() {
        this.initWindowDnd();

        if (this.mode === 'editor') {
            this.isEditor = true;
            console.log("media gallery in editor mode, loading images...");
            this.initImages();
        }
    },
    computed: {
        showMoreVisible: function () {
            return this.count > this.images.length;
        },
        leftArrowVisible: function () {
            return this.selectedImages.length > 1 && this.selectedImageIdx > 0;
        },
        rightArrowVisible() {
            return this.selectedImages.length > 1 && this.selectedImageIdx < this.selectedImages.length - 1;
        },
        selectedImages() { // from store
            return this.$store.state.selectedImages;
        },
        errMsg() {
            return this.$store.state.errMsg;
        },
    },
    methods: {
        /**
         * Initialize window drag drop events.
         */
        initWindowDnd() {
            window.addEventListener("dragenter", function (e) {
                document.querySelector("#dropzone").style.visibility = "";
                document.querySelector("#dropzone").style.opacity = 1;
            });

            window.addEventListener("dragleave", function (e) {
                e.preventDefault();
                document.querySelector("#dropzone").style.visibility = "hidden";
                document.querySelector("#dropzone").style.opacity = 0;
            });

            window.addEventListener("dragover", function (e) {
                e.preventDefault();
                document.querySelector("#dropzone").style.visibility = "";
                document.querySelector("#dropzone").style.opacity = 1;
            });

            let self = this;
            window.addEventListener("drop", function (e) {
                e.preventDefault();
                document.querySelector("#dropzone").style.visibility = "hidden";
                document.querySelector("#dropzone").style.opacity = 0;
                self.dragFilesUpload(e.dataTransfer.files);
            });
        },
        /**
         * Drag files to drop over browser to upload.
         * @param {any} files
         */
        dragFilesUpload(files) {
            if (!files.length) return;
            this.sendImages(this.getFormData(files));
        },
        /**
         * Click Upload button to choose files to upload.
         * Validate files for type and size
         */
        chooseFilesUpload() {
            const input = document.createElement('input');
            input.setAttribute('type', 'file');
            input.setAttribute('accept', 'image/*');
            input.setAttribute('multiple', null);
            input.click();
            input.onchange = () => {
                this.sendImages(this.getFormData(input.files));
            };
        },
        /**
         * Helper to get formData for dragFilesUpload and chooseFilesUpload.
         * It also valid files for type and size and update errMsg as necessary.
         */
        getFormData(fileList) {
            this.progressDialog = true;
            const formData = new FormData();
            this.totalFileCount = fileList.length;
            this.$store.dispatch('emptyErrMsg');

            let invalidTypeCount = 0;
            let invalidSizeCount = 0;
            let fileArray = [];
            for (let i = 0; i < this.totalFileCount; i++) {
                let file = fileList[i];
                let valid = false;

                // check file type
                this.validFileTypes.forEach(type => {
                    if (file.name.substr(file.name.length - type.length, type.length).toLowerCase() === type.toLowerCase()) {
                        valid = true;
                        return;
                    }
                });

                // add file if valid type and size
                if (valid) {
                    if (file.size <= this.maxImageFileSize) fileArray.push(file);
                    else invalidSizeCount++;
                }
                else invalidTypeCount++;
            }

            console.log('invalidTypeCount: ', invalidTypeCount);
            console.log('invalidSizeCount: ', invalidSizeCount);
            console.log('fileArray: ', fileArray);

            // add good files to formData
            fileArray.forEach(file => formData.append('images', file));

            // append appropriate error messages
            if (invalidTypeCount > 0) this.$store.dispatch('setErrMsg', this.errFileType + ' ');
            if (invalidSizeCount > 0) this.$store.dispatch('setErrMsg', this.errFileSize + ' ');

            return formData;
        },
        /**
         * Send files to server. The API returns ImageData with a list of media just uploaded and errorMessage if
         * there is any media not able to be uploaded.
         * @param {any} formData
         */
        sendImages(formData) {
            axios.post('/admin/media?handler=image', formData, this.$root.headers)
                .then(resp => {
                    let uploadedCount = resp.data.images.length;

                    if (uploadedCount > 0) {
                        resp.data.images.forEach(img => this.images.unshift(img));
                        this.count += uploadedCount; // inc total number of image by the number of added images
                        console.log(`${uploadedCount} images uploaded, now you have total ${this.count} images.`);
                        this.$root.toast('Image uploaded.');
                    }

                    // handle err msgs
                    if (resp.data.errorMessages.length > 0)
                        resp.data.errorMessages.forEach(msg => this.$store.dispatch('setErrMsg', msg + ' '));

                    if (this.errMsg)
                        this.$store.dispatch('setErrMsg', `${uploadedCount} of ${this.totalFileCount} files were uploaded.`);

                    this.progressDialog = false;
                })
                .catch(err => {
                    this.progressDialog = false;
                    this.$root.toastError('Image upload failed.');
                    console.log(err);
                });
        },
        insertImages() {
            this.$root.insertImages();
        },
        /**
        * Retrieve first page of images in editor mode.
        */
        initImages() {
            let url = `/admin/media?handler=images`;
            axios.get(url).then(resp => {
                this.images = resp.data.medias;
                this.count = resp.data.count;
            }).catch(err => console.log(err));
        },
        /**
         * Clicks on an image to select it.
         * @param {any} image
         */
        clickImage(image) {
            let idx = this.selectedImages.findIndex(img => img.id === image.id);

            if (idx !== -1) {
                image.selected = false;
                this.$store.dispatch('deselectImage', idx);
            }
            else {
                image.selected = true;
                this.$store.dispatch('selectImage', image);
            }
        },
        /**
         * When you select and edit multiple images, you can click arrows to traverse among them.
         */
        leftArrow() {
            this.selectedImageIdx--;
            console.log('leftArrow selectedImageIdx: ', this.selectedImageIdx)
        },
        rightArrow() {
            this.selectedImageIdx++;
            console.log('rightArrow selectedImageIdx: ', this.selectedImageIdx)
        },
        /**
         * Clicks show more button to return next page of images.
         */
        showMore() {
            this.pageNumber++;
            // if user deletes an image, this will ensure to re-get current page before moving to next page
            if (this.images.length < this.pageSize) this.pageNumber--;
            let url = `/admin/media?handler=more&pageNumber=${this.pageNumber}`;
            axios.get(url).then(resp => {
                // returned data is the list of images
                for (var i = 0; i < resp.data.length; i++) {
                    // first make sure returned item is not already on the page
                    var found = this.images.some(function (img) {
                        return img.id === resp.data[i].id;
                    });

                    if (!found)
                        this.images.push(resp.data[i]); // only append to images if not found
                }
            }).catch(err => console.log(err));
        },
        /**
         * When user selects one or more images and clicks on edit button.
         */
        editImages() {
            this.editDialogVisible = true;
        },
        deleteImages() {
            if (confirm('Are you sure you want to delete the image(s)? They will no longer appear anywhere on your website. This cannot be undone!')) {
                this.$store.dispatch('emptyErrMsg');

                const selectedCount = this.selectedImages.length;
                let ids = [];
                for (var i = 0; i < selectedCount; i++) {
                    ids.push(this.selectedImages[i].id);
                }

                let url = `/admin/media?handler=delete`;
                axios.post(url, ids, this.$root.headers)
                    .then(resp => {
                        // remove selected images from images since they are deleted
                        for (var i = 0; i < selectedCount; i++) {
                            let idx = this.images.findIndex(img => img.id === this.selectedImages[i].id);
                            this.images.splice(idx, 1);
                        }

                        // set selectedImages to empty
                        this.$store.dispatch('emptySelectedImages');

                        // dec total number of images
                        this.count -= selectedCount;
                        this.$root.toast('Image deleted.');
                    })
                    .catch(err => {
                        this.$root.toastError('Image delete failed.');
                        console.log(err);
                    });
            }
        },
        /**
         * Updates an image info, it does not close the dialog.
         */
        updateImage() {
            let url = `/admin/media?handler=update`;
            axios.post(url, this.selectedImages[this.selectedImageIdx], this.$root.headers)
                .then(resp => {
                    this.$root.toast('Image updated.');
                })
                .catch(err => {
                    this.$root.toastError('Image update failed.');
                    console.log(err);
                });
        },
        /**
         * When user closes the edit dialog by hitting esc or close.
         */
        closeEditDialog() {
            this.selectedImageIdx = 0;
            this.editDialogVisible = false;
        },
        closeMediaDialog() {
            this.$root.closeMediaDialog();
        },
    },
});