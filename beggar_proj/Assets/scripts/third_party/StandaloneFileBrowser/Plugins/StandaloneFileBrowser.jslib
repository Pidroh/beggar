var StandaloneFileBrowserWebGLPlugin = {
    // Open file.
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filter: Filter files. Example filters:
    //     Match all image files: "image/*"
    //     Match all video files: "video/*"
    //     Match all audio files: "audio/*"
    //     Custom: ".plist, .xml, .yaml"
    // multiselect: Allows multiple file selection
    UploadFile: function(gameObjectNamePtr, methodNamePtr, filterPtr, multiselect) {
    gameObjectName = UTF8ToString(gameObjectNamePtr);
    methodName = UTF8ToString(methodNamePtr);
    filter = UTF8ToString(filterPtr);

    // Delete if element exists
    var fileInput = document.getElementById(gameObjectName);
    if (fileInput) {
        document.body.removeChild(fileInput);
    }

    fileInput = document.createElement('input');
    fileInput.setAttribute('id', gameObjectName);
    fileInput.setAttribute('type', 'file');
    fileInput.style.display = 'none';
    fileInput.style.visibility = 'hidden';
    
    if (multiselect) {
        fileInput.setAttribute('multiple', '');
    }
    if (filter) {
        fileInput.setAttribute('accept', filter);
    }

    fileInput.onclick = function(event) {
        // Reset file input on click
        this.value = null;
    };

    fileInput.onchange = function(event) {
        var urls = [];
        for (var i = 0; i < event.target.files.length; i++) {
            urls.push(URL.createObjectURL(event.target.files[i]));
        }
        SendMessage(gameObjectName, methodName, urls.join());

        // Remove file input after use
        document.body.removeChild(fileInput);
    };

    document.body.appendChild(fileInput);

    // Use touchend for mobile compatibility
    var triggerClick = function() {
        fileInput.click();
        document.removeEventListener('mouseup', triggerClick);
        document.removeEventListener('touchend', triggerClick); // For mobile
    };

    document.addEventListener('mouseup', triggerClick);
    document.addEventListener('touchend', triggerClick); // Add touch event
},

    // Save file
    // DownloadFile method does not open SaveFileDialog like standalone builds, its just allows user to download file
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filenamePtr: Filename with extension
    // byteArray: byte[]
    // byteArraySize: byte[].Length
    DownloadFile: function(gameObjectNamePtr, methodNamePtr, filenamePtr, byteArray, byteArraySize) {
        gameObjectName = UTF8ToString(gameObjectNamePtr);
        methodName = UTF8ToString(methodNamePtr);
        filename = UTF8ToString(filenamePtr);

        var bytes = new Uint8Array(byteArraySize);
        for (var i = 0; i < byteArraySize; i++) {
            bytes[i] = HEAPU8[byteArray + i];
        }

        var downloader = window.document.createElement('a');
        downloader.setAttribute('id', gameObjectName);
        downloader.href = window.URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' }));
        downloader.download = filename;
        document.body.appendChild(downloader);

        document.onmouseup = function() {
            downloader.click();
            document.body.removeChild(downloader);
        	document.onmouseup = null;

            SendMessage(gameObjectName, methodName);
        }
        document.ontouchend = function() {
            downloader.click();
            document.body.removeChild(downloader);
            document.ontouchend = null;

            SendMessage(gameObjectName, methodName);
        }
    }
};

mergeInto(LibraryManager.library, StandaloneFileBrowserWebGLPlugin);