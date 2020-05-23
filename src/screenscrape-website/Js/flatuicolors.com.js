// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        var foundColors = document.getElementsByClassName('color');
        var foundAuthors = document.getElementsByClassName('author');
        var foundAuthor = undefined;

        alert(`${foundColors.length} colors found`);
        if (foundAuthors !== undefined && foundAuthors.length === 1) {
            foundAuthor = foundAuthors[0].text;
        }

        window.chrome.webview.postMessage(`clear-textbox`);
        window.chrome.webview.postMessage(`sending ${foundColors.length} colors from webview2 to uwp host \n\r`);
        if (foundAuthor !== undefined) window.chrome.webview.postMessage(`author ${foundAuthor} \n\r`);

        var foundElements = Array.prototype.filter.call(foundColors, function (xe) {
            var colorName = '';
            if (xe.children !== undefined && xe.children.length > 0) {
                colorName = xe.children[1].innerText;
            }
            window.chrome.webview.postMessage(`${xe.style.background}; ${colorName} \n`);
            return xe.style;
        });

    };
    alert('js injected from UWP & function created in webview2');
};

// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};