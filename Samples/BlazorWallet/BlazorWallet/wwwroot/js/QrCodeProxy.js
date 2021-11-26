export function create(elementId) {
    return new window.QRCode(elementId);
}

export function makeCode(instance, address) {
    instance.makeCode(`bitcoin:${address}`);
}
