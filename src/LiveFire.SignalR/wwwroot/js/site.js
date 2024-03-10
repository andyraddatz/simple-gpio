"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/fireStateHub").build();
const fireButton = document.getElementById("firePinsButton")
const resetButton = document.getElementById("resetPinsButton")
const runActsButton = document.getElementById("runActsButton")
const resetActsButton = document.getElementById("resetActsButton")

fireButton.disabled = true
resetButton.disabled = true
runActsButton.disabled = true
resetActsButton.disabled = true
connection.start().then(function() {
    connection.invoke("SendLivePinStates", null);
    connection.invoke("SendLiveActStates", null);
    fireButton.disabled = false
    resetButton.disabled = false
    runActsButton.disabled = false
    resetActsButton.disabled = false
}).catch(function(err) {
    return console.error(err.toString());
});

fireButton.addEventListener("click", function(event) {
    connection.invoke("FirePins");
});
resetButton.addEventListener("click", function(event) {
    connection.invoke("ResetPins");
});
runActsButton.addEventListener("click", function(event) {
    connection.invoke("RunActs");
});
resetActsButton.addEventListener("click", function(event) {
    connection.invoke("ResetActs");
    connection.invoke("ResetPins");
    let updateBox = document.getElementById("showUpdates")
    updateBox.innerHTML = ''
});

connection.on("ReceiveShowUpdate", function(update) {
    let updateBox = document.getElementById("showUpdates")
    var para = document.createElement("p");
    var updateString = 'T: ' + update.seconds + ' seconds'
    if (update.addressesFired && update.addressesFired.length) 
    {
        updateString = updateString + ', Fired ' + update.addressesFired
    }
    para.innerHTML = updateString
    updateBox.appendChild(para);
})
connection.on("ReceiveLiveActStates", function(acts) {
    acts.forEach(act => {

        // debugger
        let actSwitch = document.getElementById("act-" + act.actNumber)
        if (!actSwitch) return;
        let badge = actSwitch.parentNode.querySelector('label span');
        if (!badge) return;

        badge.textContent = act.displayName
        colorizeFromState(act.firingState, badge, actSwitch);

    })
})

connection.on("ReceiveLivePinStates", function(pins) {
    pins.forEach(pin => {

        // debugger
        let pinSwitch = document.getElementById("pin-" + pin.gpioPin)
        if (!pinSwitch) return;
        let badge = pinSwitch.parentNode.querySelector('label span');
        if (!badge) return;

        badge.textContent = pin.displayName
        colorizeFromState(pin.firingState, badge, pinSwitch);
    })
});

document.querySelectorAll('input.form-check-input').forEach(switchInput => {
    switchInput.addEventListener('input', function(event) {

        // debugger
        const uiSwitch = event.currentTarget;

        // assume checked means going from ready to armed 
        // because input is disabled when firing or fired
        if (uiSwitch.checked) {
            if (uiSwitch.dataset.gpiopin) connection.invoke("ArmPin", uiSwitch.dataset.gpiopin)
            else if (uiSwitch.dataset.actnumber) connection.invoke("ArmAct", uiSwitch.dataset.actnumber)
        } else {
            if (uiSwitch.dataset.gpiopin) connection.invoke("DisarmPin", uiSwitch.dataset.gpiopin)
            else if (uiSwitch.dataset.actnumber) connection.invoke("DisarmAct", uiSwitch.dataset.actnumber)
        }

    });
});

function colorizeFromState(firingState, badge, inputSwitch) {
    switch (firingState) {

        case 0: // ready
            badge.classList.add('bg-secondary');
            badge.classList.remove('bg-success', 'bg-warning', 'bg-danger');
            inputSwitch.classList.remove('bg-secondary', 'bg-success', 'bg-warning', 'bg-danger', 'border-secondary', 'border-success', 'border-warning', 'border-danger');
            inputSwitch.checked = false;
            inputSwitch.disabled = false;
            break;

        case 1: // armed
            badge.classList.add('bg-warning');
            badge.classList.remove('bg-secondary', 'bg-success', 'bg-danger');
            inputSwitch.classList.add('bg-warning', 'border-warning');
            inputSwitch.classList.remove('bg-secondary', 'bg-success', 'bg-danger', 'border-secondary', 'border-success', 'border-danger');
            inputSwitch.checked = true;
            inputSwitch.disabled = false;
            break;

        case 2: // firing
            badge.classList.add('bg-success');
            badge.classList.remove('bg-secondary', 'bg-warning', 'bg-danger');
            inputSwitch.classList.add('bg-success', 'border-success', 'disabled');
            inputSwitch.classList.remove('bg-secondary', 'bg-warning', 'bg-danger', 'border-secondary', 'border-warning', 'border-danger');
            inputSwitch.disabled = true;
            inputSwitch.checked = true;
            break;

        case 3: // fired
            badge.classList.add('bg-danger');
            badge.classList.remove('bg-secondary', 'bg-success', 'bg-warning');
            inputSwitch.classList.add('bg-danger', 'border-danger', 'disabled');
            inputSwitch.classList.remove('bg-secondary', 'bg-success', 'bg-warning', 'border-secondary', 'border-success', 'border-warning');
            inputSwitch.disabled = true;
            inputSwitch.checked = true;
            break;

        default:
            break;
    }
}