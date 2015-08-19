var mediaPlugins = "";

if (mediaPickerEnabled) {
    mediaPlugins += " mediapicker";
}

if (mediaLibraryEnabled) {
    mediaPlugins += " medialibrary";
}

tinymce.init({
	language: 'en',
    selector: ".tinymce",  
    plugins: [
        "advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker",
        "searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking",
        "save table contextmenu directionality emoticons template paste textcolor",
        mediaPlugins
    ],
    //content_css: "css/content.css",    
    toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | forecolor backcolor emoticons | " + mediaPlugins
});
