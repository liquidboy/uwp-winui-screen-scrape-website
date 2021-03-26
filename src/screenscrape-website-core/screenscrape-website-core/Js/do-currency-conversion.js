// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        doWork();
    };
    //alert('js injected from UWP & function created in webview2');
} else {
    //alert('js has already been injected into webview2');
    doWork();
};

function doWork() {
    var amt = document.forms[0].children[1].children[0].children[1].innerText;
    let result = { "result": amt };
    window.chrome.webview.postMessage( JSON.stringify(result));
}

// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};