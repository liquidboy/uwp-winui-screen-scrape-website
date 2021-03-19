// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        var foundFromTos = document.getElementsByClassName('ctxt b_focusTextMedium');

        alert(`${foundFromTos.length} text inputs found`);
        
    };
    alert('js injected from UWP & function created in webview2');
} else {
    alert('js has already been injected into webview2');
};

// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};