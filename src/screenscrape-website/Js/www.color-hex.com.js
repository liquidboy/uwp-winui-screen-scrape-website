// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        var foundColorDiv = document.getElementsByClassName('palettecontainer');
        var foundColors = foundColorDiv[0].getElementsByClassName('palettecolordivc');
        var foundAuthor = undefined;

        alert(`${foundColors.length} colors found`);
        if (foundColorDiv !== undefined && foundColorDiv.length === 1) {
            foundAuthor = foundColorDiv[0].previousElementSibling.innerText;
        };
        
        window.chrome.webview.postMessage(`clear-textbox`);
        alert(`sending ${foundColors.length} colors from webview2 to uwp host \n`);
        if (foundAuthor !== undefined) window.chrome.webview.postMessage(`author ${foundAuthor} \n`);

        var foundElements = Array.prototype.filter.call(foundColors, function (xe) {
            window.chrome.webview.postMessage(`${xe.style['background-color']}; ${xe.title} \n`);
            return xe.style;
        });

        window.chrome.webview.postMessage(`finished-scraping`);
    };
    alert('js injected from UWP & function created in webview2');
} else {
    alert('js has already been injected into webview2');
};

// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};