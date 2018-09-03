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
    }),
    methods: {
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
        uploadImages() {
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

                axios.post('/admin/media?handler=image', formData, { headers: { 'XSRF-TOKEN': this.$root.tok } })
                    .then(resp => {
                        this.images = resp.data;
                        this.progressDialog = false;
                        this.$root.toast('Image uploaded.');
                    })
                    .catch(err => {
                        this.progressDialog = false;
                        this.$root.toast('Image upload failed.', 'red');
                        console.log(err);
                    });
            };
        },
        deleteImage() {
            if (confirm('Are you sure you want to delete this image? Deleted image will no longer appear anywhere on your website. This cannot be undone!')) {
                console.log('deleting image: ', this.selectedImage);
                let url = `/admin/media?id=${this.selectedImage.id}`;
                axios.delete(url, { headers: { 'XSRF-TOKEN': this.$root.tok } })
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
            axios.post(url, this.selectedImage, { headers: { 'XSRF-TOKEN': this.$root.tok } })
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