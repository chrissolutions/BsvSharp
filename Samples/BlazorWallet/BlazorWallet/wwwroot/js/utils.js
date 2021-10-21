function copyText(elementId) {
    const inputText = document.getElementById(elementId);
    inputText.select();
    inputText.setSelectionRange(0, 99999);
    document.execCommand("copy");
}

function sweetAlert(message) {
    window.Swal.fire("", message, "success");
}