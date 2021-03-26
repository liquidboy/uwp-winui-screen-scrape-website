// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        doWork();
    };
    alert('js injected from UWP & function created in webview2');
} else {
    alert('js has already been injected into webview2');
    doWork();
};

function doWork() {
    var foundFromTos = document.querySelectorAll('input[id="amount"]');


    //alert(`${foundFromTos.length} text inputs found`);

    var input = foundFromTos[0];
    input.focus();
    //input.setAttribute("value", "5.99");
    input.value = 7.99;


    var amt = document.forms[0].children[1].children[0].children[1].innerText;
    alert(amt);
}


// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};