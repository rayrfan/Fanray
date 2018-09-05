/**
 * The component for Admin/Pages/Media.cshtml
 */
Vue.component('blog-media', {
    mixins: [blogMediaMixin],
    data: () => ({
        dialogVisible: false,
        selectedImage: null,
        snackbar: false,
        snackbarText: '',
        snackbarColor: '',
        progressDialog: false,
        errMsg: '',
    }),
    mounted() {
        this.initWindowDnd();
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
            this.progressDialog = true;

            const formData = new FormData();
            if (!files.length) return;
            Array.from(Array(files.length).keys())
                 .map(x => {
                     formData.append('images', files[x]);
                 });

            this.sendImages(formData);
        },
        /**
         * Click Upload button to choose files to upload.
         */
        chooseFilesUpload() {
            const input = document.createElement('input');
            input.setAttribute('type', 'file');
            input.setAttribute('accept', 'image/*');
            input.setAttribute('multiple', null);
            input.click();
            input.onchange = () => {
                this.progressDialog = true;
                const formData = new FormData();
                for (let i = 0; i < input.files.length; i++) {
                    formData.append('images', input.files[i]);
                }

                this.sendImages(formData);
            };
        },
        /**
         * Send files to server. The API returns ImageData with a list of media just uploaded and errormsg if
         * there is any media not able to be uploaded.
         * @param {any} formData
         */
        sendImages(formData) {
            axios.post('/admin/media?handler=image', formData, this.$root.headers)
                .then(resp => {

                    console.log(resp.data);
                    if (resp.data.images.length > 0) {
                        for (var i = 0; i < resp.data.images.length; i++) {
                            this.images.unshift(resp.data.images[i]);
                        }
                        this.$root.toast('Image uploaded.');
                    }
                    if (resp.data.errorMessage) this.errMsg = resp.data.errorMessage;
                    this.progressDialog = false;
                })
                .catch(err => {
                    this.progressDialog = false;
                    this.$root.toast('Image upload failed.', 'red');
                    console.log(err);
                });
        },
        selectImage(image) {
            this.dialogVisible = true;
            this.selectedImage = image;
            console.log("selecting image: ", image);
        },
        getImages() {
            let url = `/admin/media?handler=images`;
            axios.get(url).then(resp => {
                this.images = resp.data;
            }).catch(err => console.log(err));
        },
        deleteImage() {
            if (confirm('Are you sure you want to delete this image? Deleted image will no longer appear anywhere on your website. This cannot be undone!')) {
                console.log('deleting image: ', this.selectedImage);
                let url = `/admin/media?id=${this.selectedImage.id}`;
                axios.delete(url, this.$root.headers)
                    .then(resp => {
                        this.dialogVisible = false;
                        this.images = resp.data;
                        this.$root.toast('Image deleted.');
                    })
                    .catch(err => {
                        this.$root.toast('Image delete failed.', 'red');
                        console.log(err);
                    });
            }
        },
        updateImage() {
            let url = `/admin/media?handler=update`;
            axios.post(url, this.selectedImage, this.$root.headers)
                .then(resp => {
                    this.dialogVisible = false;
                    this.images = resp.data;
                    this.$root.toast('Image updated.');
                })
                .catch(err => {
                    this.$root.toast('Image update failed.', 'red');
                    console.log(err);
                });
        },
    },
});