/*
Filename:  home.js
Purpose:   JavaScript used for Home Controller - Index View
Contains:  JavaScript to facilitate product filtering
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

$(document).ready(function () {

    // Add event listener to perform filter operation when drop down is changed
    document.getElementById("filterBy").addEventListener('change', function () {
        document.getElementById("filterByButton").click();
    });

});
